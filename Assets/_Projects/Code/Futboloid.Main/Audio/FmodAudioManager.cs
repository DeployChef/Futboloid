using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using Futboloid.Core.Audio;
using UnityEngine;

namespace Futboloid.Main.Audio
{
    /// <summary>
    /// FMOD-реализация <see cref="IAudioManager"/> по схеме проекта Futboloid_WebGL:
    /// 4 события-контейнера (event:/Music, event:/SFX, event:/perc, event:/UI) — по одному вечному инстансу.
    /// One-shot — «клик» параметром: значение → один FMOD-апдейт → возврат в none (non-click-non).
    /// Music — стейт-машина на глобальном game_state (game/menu/start/stop), голы — глобальный goals (me/enemy).
    /// ball/perc/ui — локальные параметры инстансов.
    /// Громкости — volume инстансов (всё через master bus, шин нет).
    /// WebGL: банки грузятся асинхронно, инстансы создаются когда банк готов; до этого вызовы буферизуются.
    /// </summary>
    [DefaultExecutionOrder(1000)] // Update строго ПОСЛЕ RuntimeManager.Update (System::update):
    // значение клика гарантированно доезжает до FMOD до сброса в none
    public sealed class FmodAudioManager : MonoBehaviour, IAudioManager
    {
        private const string PrefsMusicVolume = "futboloid.audio.music_volume";
        private const string PrefsSfxVolume = "futboloid.audio.sfx_volume";
        private const float DefaultVolume = 0.8f;

        private const string EventMusicPath = "event:/Music";
        private const string EventSfxPath = "event:/SFX";
        private const string EventPercPath = "event:/perc";
        private const string EventUiPath = "event:/UI";

        private const string ParamGameState = "game_state";
        private const string ParamGoals = "goals";
        private const string ParamBall = "ball";
        private const string ParamPerc = "perc";
        private const string ParamUi = "ui";

        private const string LabelNone = "none";

        [SerializeField] private string[] banksToLoad = { "Master" };
        [Tooltip("Логировать readback глобальных параметров (sent/readback) для проверки доставки значений")]
        [SerializeField] private bool debugGlobalReadback = true;

        private EventInstance _music;
        private EventInstance _sfx;
        private EventInstance _perc;
        private EventInstance _ui;

        private readonly List<PendingReset> _pendingResets = new();
        private readonly Dictionary<string, float> _lastPlayTimes = new();
        private readonly List<string> _pendingPlays = new();

        private bool _persistentReady;
        private bool _musicPaused;
        private bool _musicStarted;
        private float _nextInstanceRetryTime;

        public bool IsMusicPaused => _musicPaused;

        public bool HasMusicClip => _persistentReady;

        private void Start()
        {
            // WebGL: загрузка асинхронная (UnityWebRequest внутри интеграции).
            // Повторный вызов безопасен: RuntimeManager дедуплицирует по имени банка.
            foreach (var bank in banksToLoad)
            {
                if (!RuntimeManager.HasBankLoaded(bank))
                    RuntimeManager.LoadBank(bank, loadSamples: true);
            }
        }

        private void Update()
        {
            if (!_persistentReady)
                TryCreatePersistentInstances();

            ProcessPendingResets();
        }

        private void OnDestroy()
        {
            ReleasePersistentInstances();
        }

        // ===== Persistent instances =====

        /// <summary>Пауза между попытками создать инстансы: CreateInstance бросает исключение, если события нет в банке.</summary>
        private const float InstanceRetryCooldown = 3f;

        private void TryCreatePersistentInstances()
        {
            if (banksToLoad.Length == 0 || !RuntimeManager.HasBankLoaded(banksToLoad[0]))
                return;

            if (Time.unscaledTime < _nextInstanceRetryTime)
                return;

            _nextInstanceRetryTime = Time.unscaledTime + InstanceRetryCooldown;

            var music = CreateInstanceSafe(EventMusicPath);
            var sfx = CreateInstanceSafe(EventSfxPath);
            var perc = CreateInstanceSafe(EventPercPath);
            var ui = CreateInstanceSafe(EventUiPath);

            if (!music.isValid() || !sfx.isValid() || !perc.isValid() || !ui.isValid())
            {
                // Не все события в банке — тихо ждём пересборки, без спама и без лишних start()
                ReleaseInstance(music);
                ReleaseInstance(sfx);
                ReleaseInstance(perc);
                ReleaseInstance(ui);
                return;
            }

            // One-shot инстансы стартуют в none — ничего не должно звучать при старте
            sfx.setParameterByNameWithLabel(ParamBall, LabelNone, ignoreseekspeed: true);
            perc.setParameterByNameWithLabel(ParamPerc, LabelNone, ignoreseekspeed: true);
            ui.setParameterByNameWithLabel(ParamUi, LabelNone, ignoreseekspeed: true);
            sfx.start();
            perc.start();
            ui.start();

            // Music-инстанс молчит до первого выхода на поле — стартуем по MusicMatch
            _music = music;
            _sfx = sfx;
            _perc = perc;
            _ui = ui;

            ApplyVolumesToInstances();
            _persistentReady = true;

            Debug.Log($"[FMOD] Инстансы готовы (id={GetInstanceID()}). musicVol={GetMusicVolume():F2} sfxVol={GetSfxVolume():F2}");

            // Проигрываем то, что пришло до готовности (например, музыку на выходе на поле)
            if (_pendingPlays.Count > 0)
            {
                var pending = _pendingPlays.ToArray();
                _pendingPlays.Clear();

                foreach (var soundId in pending)
                    Play(soundId);
            }
        }

        private static EventInstance CreateInstanceSafe(string eventPath)
        {
            try
            {
                return RuntimeManager.CreateInstance(eventPath);
            }
            catch (EventNotFoundException)
            {
                Debug.LogWarning($"[FMOD] Событие '{eventPath}' не найдено в банке — добавь его в банк Master в FMOD Studio и пересобери (Ctrl+B)");
                return default;
            }
        }

        private void StartMusicIfNeeded()
        {
            if (_musicStarted || !_music.isValid())
                return;

            _music.start();
            _musicStarted = true;
        }

        private void ReleasePersistentInstances()
        {
            ReleaseInstance(_music);
            ReleaseInstance(_sfx);
            ReleaseInstance(_perc);
            ReleaseInstance(_ui);

            _music = default;
            _sfx = default;
            _perc = default;
            _ui = default;
            _persistentReady = false;
            _musicPaused = false;
            _musicStarted = false;
            _pendingResets.Clear();
        }

        private static void ReleaseInstance(EventInstance instance)
        {
            if (!instance.isValid())
                return;

            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance.release();
        }

        // ===== Playback =====

        public void Play(string soundId, float? pitch = null, float? pitchRandomRange = null)
        {
            if (!_persistentReady)
            {
                // Банк ещё грузится (WebGL — асинхронно) — не теряем вызов, проиграем когда инстансы будут готовы
                _pendingPlays.Add(soundId);
                return;
            }

            switch (soundId)
            {
                // Стейт-машина музыки
                case AudioCatalog.Ids.MusicMatch:
                    StartMusicIfNeeded();
                    SetGameState("game");
                    return;
                case AudioCatalog.Ids.UiPauseOpen:
                    SetGameState("menu");
                    return;
                case AudioCatalog.Ids.MatchStart:
                    SetGameState("start");
                    // Стейт-машина: после стартового сигнала возвращаемся в game, иначе музыка не возобновляется
                    _pendingResets.Add(new PendingReset(ParamGameState, Time.frameCount + ResetDelayFrames, "game"));
                    return;
                case AudioCatalog.Ids.MatchEnd:
                    SetGameState("stop");
                    return;

                // Голы — клик глобальным параметром
                case AudioCatalog.Ids.GoalScored:
                    ClickGlobal(ParamGoals, "me", 0.45f);
                    return;
                case AudioCatalog.Ids.GoalConceded:
                    ClickGlobal(ParamGoals, "enemy", 0.5f);
                    return;

                // Мяч — клик ball
                case AudioCatalog.Ids.BallHit:
                    ClickLocal(_sfx, ParamBall, "ball", 0.2f);
                    return;
                case AudioCatalog.Ids.BallHitMan:
                    ClickLocal(_sfx, ParamBall, "man", 0.3f);
                    return;
                case AudioCatalog.Ids.DefenderDestroyed:
                    ClickLocal(_sfx, ParamBall, "def_die", 0.01f);
                    return;
                case AudioCatalog.Ids.PerkPick:
                    ClickLocal(_sfx, ParamBall, "perc", 0.5f);
                    return;

                // Перкуссия — клик perc
                case AudioCatalog.Ids.LevelUp:
                    ClickLocal(_perc, ParamPerc, "perc_move", 1f);
                    return;
                case AudioCatalog.Ids.BonusPickCardFocus:
                    ClickLocal(_perc, ParamPerc, "perc_move", 0.05f);
                    return;

                // UI — клик ui
                case AudioCatalog.Ids.UiMove:
                    ClickLocal(_ui, ParamUi, "ui_move", 0.05f);
                    return;
                case AudioCatalog.Ids.UiClick:
                case AudioCatalog.Ids.UiStart:
                    ClickLocal(_ui, ParamUi, "accept", 0.05f);
                    return;

                default:
                    return; // Звук не замаплен на схему FMOD — тихо пропускаем
            }
        }

        public void Stop(string soundId)
        {
            // One-shot-клики доигрывают сами; пер-шот инстансов в схеме нет
        }

        public void StopMusic()
        {
            if (!_persistentReady)
                return;

            _musicPaused = false;
            SetGameState("stop");
        }

        public void PauseMusic()
        {
            if (!_persistentReady || !_musicStarted)
                return;

            _musicPaused = true;
            SetGameState("menu");
        }

        public void ResumeMusic()
        {
            if (!_persistentReady || !_musicStarted)
                return;

            _musicPaused = false;
            SetGameState("game");
        }

        public void StopAll()
        {
            if (!_persistentReady)
                return;

            StopMusic();

            // Мгновенно глушим незавершённые клики
            _sfx.setParameterByNameWithLabel(ParamBall, LabelNone, ignoreseekspeed: true);
            _perc.setParameterByNameWithLabel(ParamPerc, LabelNone, ignoreseekspeed: true);
            _ui.setParameterByNameWithLabel(ParamUi, LabelNone, ignoreseekspeed: true);
            _pendingResets.Clear();
        }

        public bool IsPlaying(string soundId)
        {
            // Музыка — вечный инстанс стейт-машины; клики отслеживания не требуют
            return _persistentReady && soundId == AudioCatalog.Ids.MusicMatch;
        }

        // ===== Warmup (для FMOD — no-op: сэмплы грузятся вместе с банком) =====

        public IEnumerable<AudioClip> EnumerateClips()
        {
            yield break;
        }

        public void WarmupClip(AudioClip clip)
        {
        }

        // ===== Volume control =====

        public void SetMusicVolume(float volume)
        {
            PlayerPrefs.SetFloat(PrefsMusicVolume, Mathf.Clamp01(volume));
            PlayerPrefs.Save();

            if (_music.isValid())
                _music.setVolume(GetMusicVolume());
        }

        public void SetSfxVolume(float volume)
        {
            PlayerPrefs.SetFloat(PrefsSfxVolume, Mathf.Clamp01(volume));
            PlayerPrefs.Save();

            ApplySfxVolumeToInstances();
        }

        public float GetMusicVolume() =>
            PlayerPrefs.GetFloat(PrefsMusicVolume, DefaultVolume);

        public float GetSfxVolume() =>
            PlayerPrefs.GetFloat(PrefsSfxVolume, DefaultVolume);

        private void ApplyVolumesToInstances()
        {
            if (_music.isValid())
                _music.setVolume(GetMusicVolume());

            ApplySfxVolumeToInstances();
        }

        private void ApplySfxVolumeToInstances()
        {
            var volume = GetSfxVolume();

            if (_sfx.isValid())
                _sfx.setVolume(volume);
            if (_perc.isValid())
                _perc.setVolume(volume);
            if (_ui.isValid())
                _ui.setVolume(volume);
        }

        // ===== Параметры =====

        /// <summary>Стейт-машина музыки: game_state ставится глобально и держится до следующего стейта.</summary>
        private void SetGameState(string label)
        {
            RuntimeManager.StudioSystem.setParameterByNameWithLabel(ParamGameState, label, ignoreseekspeed: true);

            if (debugGlobalReadback)
                LogGlobalReadback(ParamGameState, label);
        }

        /// <summary>Клик локальным параметром инстанса: значение → следующий кадр → none.</summary>
        private void ClickLocal(EventInstance instance, string parameter, string label, float cooldown)
        {
            if (!instance.isValid() || !PassCooldown(parameter + label, cooldown))
                return;

            instance.setParameterByNameWithLabel(parameter, label, ignoreseekspeed: true);
            _pendingResets.Add(new PendingReset(parameter, Time.frameCount + ResetDelayFrames, instance));
            Debug.Log($"[FMOD] клик: {parameter} = {label}");
        }

        /// <summary>Клик глобальным параметром: значение → следующий кадр → none.</summary>
        private void ClickGlobal(string parameter, string label, float cooldown)
        {
            if (!PassCooldown(parameter + label, cooldown))
                return;

            RuntimeManager.StudioSystem.setParameterByNameWithLabel(parameter, label, ignoreseekspeed: true);
            _pendingResets.Add(new PendingReset(parameter, Time.frameCount + ResetDelayFrames));

            if (debugGlobalReadback)
                LogGlobalReadback(parameter, label);
        }

        /// <summary>
        /// Сброс в none — на следующий кадр: FMOD должен увидеть переход none → значение
        /// в одном System::update, иначе клик не сработает.
        /// </summary>
        private const int ResetDelayFrames = 1;

        private bool PassCooldown(string key, float cooldown)
        {
            if (cooldown <= 0f)
                return true;

            if (_lastPlayTimes.TryGetValue(key, out var lastTime)
                && Time.unscaledTime - lastTime < cooldown)
                return false;

            _lastPlayTimes[key] = Time.unscaledTime;
            return true;
        }

        private void ProcessPendingResets()
        {
            if (_pendingResets.Count == 0)
                return;

            var frame = Time.frameCount;

            for (var i = _pendingResets.Count - 1; i >= 0; i--)
            {
                var reset = _pendingResets[i];
                if (frame < reset.ResetFrame)
                    continue;

                if (reset.IsGlobal)
                    RuntimeManager.StudioSystem.setParameterByNameWithLabel(reset.Parameter, reset.ResetLabel, ignoreseekspeed: true);
                else if (reset.Instance.isValid())
                    reset.Instance.setParameterByNameWithLabel(reset.Parameter, reset.ResetLabel, ignoreseekspeed: true);

                _pendingResets.RemoveAt(i);
            }
        }

        /// <summary>Readback-проверка по правилам AGENTS.md: sent/readback глобального параметра.</summary>
        private static void LogGlobalReadback(string parameter, string sent)
        {
            var result = RuntimeManager.StudioSystem.getParameterByName(parameter, out var value, out _);

            if (result != FMOD.RESULT.OK)
            {
                Debug.LogWarning($"[FMOD] {parameter}: sent '{sent}', readback failed ({result}) — параметр не виден System API");
                return;
            }

            Debug.Log($"[FMOD] {parameter}: sent '{sent}', readback {value}");
        }

        private readonly struct PendingReset
        {
            public PendingReset(string parameter, int resetFrame)
                : this(parameter, resetFrame, LabelNone, isGlobal: true, instance: default)
            {
            }

            public PendingReset(string parameter, int resetFrame, EventInstance instance)
                : this(parameter, resetFrame, LabelNone, isGlobal: false, instance)
            {
            }

            public PendingReset(string parameter, int resetFrame, string resetLabel)
                : this(parameter, resetFrame, resetLabel, isGlobal: true, instance: default)
            {
            }

            private PendingReset(string parameter, int resetFrame, string resetLabel, bool isGlobal, EventInstance instance)
            {
                Parameter = parameter;
                ResetFrame = resetFrame;
                ResetLabel = resetLabel;
                IsGlobal = isGlobal;
                Instance = instance;
            }

            public string Parameter { get; }
            public int ResetFrame { get; }
            public string ResetLabel { get; }
            public bool IsGlobal { get; }
            public EventInstance Instance { get; }
        }
    }
}
