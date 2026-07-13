using System;
using Cysharp.Threading.Tasks;
using Futboloid.Core.Leaderboards;
using Futboloid.Core.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Futboloid.UI.Views.Leaderboards
{
    /// <summary>
    /// Переиспользуемый ввод и сохранение ника (главное меню, экран конца забега).
    /// </summary>
    public sealed class PlayerNicknameControl : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nicknameInput;
        [SerializeField] private Button saveButton;
        [SerializeField] private TextMeshProUGUI saveButtonLabel;

        private ILeaderboardService _leaderboard;
        private IPlayerNicknameStore _nicknameStore;
        private ILocalizationService _localization;

        public event Action NicknameSaved;

        [Inject]
        public void Construct(
            ILeaderboardService leaderboard,
            IPlayerNicknameStore nicknameStore,
            ILocalizationService localization)
        {
            _leaderboard = leaderboard;
            _nicknameStore = nicknameStore;
            _localization = localization;
        }

        private void Awake()
        {
            if (saveButton != null)
                saveButton.onClick.AddListener(OnSaveClicked);
        }

        private void OnEnable()
        {
            ApplyStaticTexts();
            LoadFromStore();
        }

        private void OnDestroy()
        {
            if (saveButton != null)
                saveButton.onClick.RemoveListener(OnSaveClicked);
        }

        public void LoadFromStore()
        {
            if (nicknameInput == null || _nicknameStore == null)
                return;

            nicknameInput.text = _nicknameStore.Nickname;
        }

        private void OnSaveClicked() => SaveAsync().Forget();

        private async UniTaskVoid SaveAsync()
        {
            if (_leaderboard == null || nicknameInput == null)
                return;

            await _leaderboard.SetNicknameAsync(nicknameInput.text);
            NicknameSaved?.Invoke();
        }

        private void ApplyStaticTexts()
        {
            if (saveButtonLabel != null)
                saveButtonLabel.text = GetText(LocalizationKeys.LeaderboardSaveNick);
        }

        private string GetText(string key) =>
            _localization != null && _localization.IsReady
                ? _localization.Get(LocalizationTables.UI, key)
                : key;
    }
}
