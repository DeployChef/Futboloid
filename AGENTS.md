# Правила работы с Koda AI

## Основной принцип

**Мое слово — закон.** При каждом обращении Валентина к выполнению задачи:

1. **Обращаюсь по имени:** Говорю "Валентин, я делаю [что нужно]"
2. **Выполняю прямо:** Если сказано найти — ищу. Если сказано проверить — проверяю. Если сказано сделать — делаю.
3. **Никаких альтернатив:** Не предлагаю других вариантов, не придумываю, не усложняю. Делаю именно то, что сказано.

## Принцип взаимодействия

- **Я говорю что сделать** → **Ты делаешь**
- **Я проверяю** → **Говорю "всё хорошо" или "не хорошо"**
- **Если не хорошо** → **Ищу ошибки и исправляю**
- **Задачи:** дебаггинг, оптимизация, расстановка инструментов, разработка

**После каждой выполненной задачи:**
- **ПРОВЕРЯЮ КОМПИЛЯЦИЮ** — запускаю компиляцию/проверяю ошибки
- **ЕСЛИ ОШИБКИ** — немедленно исправляю все ошибки компиляции
- **ПЕРЕД ОТЧЁТОМ** — убеждаюсь, что ошибок компиляции нет
- **ВНОШУ В ПРАВИЛА** — все найденные ошибки API вношу в AGENTS.md чтобы не повторять

## Прямые указания на действие

При формулировках-командах Koda действует мгновенно, без вопросов:

- **"Сделай"** / **"Сделай это"** → делаю
- **"Быстро найди в интернете"** / **"Ищи в интернете"** → ищу
- **"Что нет, так не надо делать"** → это не делаю
- **"Внеси документацию"** → вношу документацию
- **"Поменяй правила"** → меняю правила

## Обсуждение и мнение

- Если Валентин спрашивает **мнение** — Koda говорит своё мнение: насколько это рентабельно, как это войдёт в текущую архитектуру и т.д.
- Мы общаемся друг с другом, но Koda не навязывает своё мнение — просто отвечает на вопрос.
- Когда задача задана как обсуждение — Koda не делает, а рассуждает.
- Когда задача задана как команда — Koda не рассуждает, а делает.

## FMOD (F-мода) — общие правила

**Версия FMOD Engine:** 2.03

**Документация FMOD для Unity:**
- Главная: https://fmod.com/docs/2.03/unity
- API Reference (Unity): https://fmod.com/docs/2.03/unity/api.html
- Common API: https://fmod.com/docs/2.03/unity/api-common.html
- Core API (System): https://fmod.com/docs/2.03/api/core-api-system.html
- Studio API: https://fmod.com/docs/2.03/api/studio-api.html
- Q&A Forum: https://qa.fmod.com/

**Важно:** ВСЕГДА проверяю актуальность API по ссылкам выше перед тем как что-либо предлагать. Не предлагаю устаревшие методы или классы, которых больше нет.

### API Справка (FMOD 2.03 для Unity) — **ПРАВИЛЬНО**

**Основные классы:**
- `FMODUnity.RuntimeManager` — главный менеджер для работы с FMOD (namespace `FMODUnity`)
- `FMOD.Studio.EventInstance` — экземпляр события (namespace `FMOD.Studio`)
- `FMOD.Studio.EventDescription` — описание события
- `FMOD.Studio.Bus` — шина (master, bus)
- `FMOD.Studio.Bank` — банк звуков
- `FMODUnity.StudioEventEmitter` — компонент для драг-н-дроп событий в Unity
- `FMODUnity.EventRef` — ссылка на событие
- `FMOD.ATTRIBUTES_3D` — структура 3D-атрибутов (позиция, скорость, forward, up)
- `FMOD.VECTOR` — координаты x, y, z (структура)

**Методы RuntimeManager:**
- `RuntimeManager.CreateInstance(string path)` — создать инстанс события по пути (возвращает `EventInstance`)
- `RuntimeManager.CreateInstance(EventReference reference)` — создать инстанс по EventReference
- `RuntimeManager.GetEvent(string path)` — получить EventInstance по пути
- `RuntimeManager.GetBus(string path)` — получить Bus по пути
- `RuntimeManager.AttachInstanceToGameObject(EventInstance instance, Transform transform)` — привязать к GameObject
- `RuntimeManager.DetachInstanceFromGameObject(EventInstance instance)` — от detach от GameObject
- `RuntimeManager.LoadBank(string name, bool loadSamples)` — загрузить банк
- `RuntimeManager.HasBankLoaded(string name)` — проверить загружен ли банк
- `RuntimeManager.PlayOneShot(EventReference reference, Vector3 position)` — проиграть один раз в позиции

**Методы EventInstance (FMOD.Studio.EventInstance) — СТРУКТУРА, НЕ КЛАСС:**
- `isValid()` — проверить валидность (возвращает bool). **ИСПОЛЬЗОВАТЬ ВМЕСТО `== null`**
- `start()` — запустить событие
- `stop(STOP_MODE mode)` — остановить (полный путь: `FMOD.Studio.STOP_MODE.ALLOWFADEOUT`)
- `setParameterByName(string name, float value, bool ignoreseekspeed = false)` — установить параметр по имени
- `setParameterByID(PARAMETER_ID id, float value, bool ignoreseekspeed = false)` — установить параметр по ID
- `getParameterByName(string name, out float value)` — получить параметр по имени
- `setParameterByNameWithLabel(string name, string label, bool ignoreseekspeed = false)` — установить параметр с лейблом
- `set3DAttributes(FMOD.ATTRIBUTES_3D attributes)` — установить 3D атрибуты
- `get3DAttributes(out FMOD.ATTRIBUTES_3D attributes)` — получить 3D атрибуты
- `release()` — освободить ресурс
- `setPaused(bool paused)` — поставить на паузу
- `setVolume(float volume)` — установить громкость
- `getVolume(out float volume, out float volumeDirty)` — получить громкость

**STOP_MODE (enum):**
- Полный путь: `FMOD.Studio.STOP_MODE.ALLOWFADEOUT` или `FMOD.Studio.STOP_MODE.IMMEDIATE`
- **Не использовать** `STOP_MODE` без префикса — конфликт с `FMODUnity.STOP_MODE`

**FMOD.ATTRIBUTES_3D (структура):**
```csharp
FMOD.ATTRIBUTES_3D attributes = new FMOD.ATTRIBUTES_3D
{
    position = new FMOD.VECTOR { x = posX, y = posY, z = posZ },
    velocity = new FMOD.VECTOR { x = velX, y = velY, z = velZ },
    forward = new FMOD.VECTOR { x = fwdX, y = fwdY, z = fwdZ },
    up = new FMOD.VECTOR { x = upX, y = upY, z = upZ }
};
eventInstance.set3DAttributes(attributes);
```

**3D атрибуты:**
- `FMOD.ATTRIBUTES_3D` — структура с полями: `position`, `velocity`, `forward`, `up`
- `FMOD.VECTOR` — структура с полями: `x`, `y`, `z`

**Основные паттерны использования:**
1. **Статический звук:** `RuntimeManager.PlayOneShot(eventRef, transform.position)`
2. **Динамическое событие:**
   ```csharp
   var evt = RuntimeManager.CreateInstance(eventPath);
   evt.start();
   evt.set3DAttributes(attributes);
   evt.setParameterByName("RTPC_Name", value);
   ```
3. **Получить и запустить:**
   ```csharp
   var evt = RuntimeManager.GetEvent("event:/path");
   evt.start();
   ```
4. **Громкость шины:**
   ```csharp
   var bus = RuntimeManager.GetBus("bus:/Master");
   bus.setVolume(vol);
   ```
5. **3D звук:**
   ```csharp
   var attributes = new FMOD.ATTRIBUTES_3D
   {
       position = new FMOD.VECTOR { x = pos.x, y = pos.y, z = pos.z },
       velocity = new FMOD.VECTOR { x = 0f, y = 0f, z = 0f },
       forward = new FMOD.VECTOR { x = 0f, y = 0f, z = 1f },
       up = new FMOD.VECTOR { x = 0f, y = 1f, z = 0f }
   };
   evtInstance.set3DAttributes(attributes);
   ```

**Где хранятся пути событий:**
- `event:/` — путь к событию в FMOD Studio
- `bus:/` — путь к шине

**Unity компонент StudioEventEmitter:**
- `Play()` — воспроизвести
- `Stop()` — остановить
- `SetParameter()` — установить параметр
- Перетаскиваешь событие из FMOD Studio в инспектор

### Правила EventInstance (СТРУКТУРА)

- ❌ **НЕЛЬЗЯ:** `if (instance == null)` — EventInstance это struct, не class
- ❌ **НЕЛЬЗЯ:** `instance = null` — struct нельзя присвоить null
- ✅ **МОЖНО:** `if (instance.isValid())` — проверка валидности
- ✅ **МОЖНО:** `instance = default` — сброс в дефолтное значение

### Правила RTPC

- ❌ **НЕЛЬЗЯ:** `instance.setParameter("name", value)` — такого метода нет
- ❌ **НЕЛЬЗЯ:** `instance.setParameterFloat("name", value)` — такого метода нет
- ✅ **МОЖНО:** `instance.setParameterByName("name", value)` — правильный метод
- ⚠️ **Параметр в FMOD Studio должен быть Game Parameter (exposed).** Автоматические параметры FMOD (Distance/Elevation/Event Orientation и т.п.) и параметры с `isExposedRecursively=false` НЕ управляются из кода: `setParameterByName` молча не работает, значение висит на initialValue. Проверка по метаданным FMOD-проекта: `Metadata/ParameterPreset/*.xml` → `GameParameter` → `parameterType` (0 = GAME_CONTROLLED — правильно; 1–9 = AUTOMATIC_* — из кода не управляется) и `isExposedRecursively` (false = не виден в Event Browser и API). Типы: 0=GameControlled, 1=AutomaticDistance, 2=AutomaticEventConeAngle, 3=AutomaticEventOrientation, 4=AutomaticDirection, 5=AutomaticElevation, 6=AutomaticListenerOrientation, 7=AutomaticSpeed, 8=AutomaticSpeedAbsolute, 9=AutomaticDistanceNormalized.

### Правила STOP_MODE

- ❌ **НЕЛЬЗЯ:** `STOP_MODE.ALLOWFADEOUT` — конфликт имён (FMODUnity vs FMOD.Studio)
- ✅ **МОЖНО:** `FMOD.Studio.STOP_MODE.ALLOWFADEOUT` — полный путь

### Правила Bank (СТРУКТУРА)

- ❌ **НЕЛЬЗЯ:** `bank.getName(out name)` — такого метода НЕТ у Bank
- ✅ **МОЖНО:** `bank.getPath(out path)` — возвращает `bank:/ИмяБанка`; имя вырезать: `System.IO.Path.GetFileNameWithoutExtension(path.Replace("bank:/", ""))`
- ✅ Проверка валидности: `bank.isValid()` (структура, как EventInstance)
- ❌ **НЕЛЬЗЯ:** `eventDescription.getBankList(out banks)` — у EventDescription в FMOD 2.03 такого метода НЕТ (ошибка компиляции CS1061)
- ✅ **МОЖНО:** найти банки ивента перебором: `RuntimeManager.StudioSystem.getBankList(out Bank[] banks)` → для каждого `bank.getEventList(out EventDescription[] events)` → сравнить `event.getPath()` с путём ивента
- ✅ У Bank есть `getEventList`/`getEventCount` — ивенты внутри банка
- ⚠️ Вызов `RuntimeManager.LoadBank(...)` из своей asmdef-сборки требует ссылки на **Unity.Addressables** в asmdef (у RuntimeManager есть перегрузка с `AssetReference`) — иначе `CS0012: The type 'AssetReference' is defined in an assembly that is not referenced`

### Правила глобальных RTPC-параметров

- ❌ **НЕЛЬЗЯ:** слепо `StudioSystem.setParameterByName("Name", v)` — матч имени строгий; имя параметра в FMOD-проекте может содержать скрытые пробелы → параметр не находится, readback даёт `ERR_EVENT_NOTFOUND`, set молча не применяется
- ✅ **МОЖНО:** резолвить `PARAMETER_ID` при старте: `getParameterDescriptionList` → сравнить имена через `Trim()` + `OrdinalIgnoreCase` → сохранить ID → дальше `setParameterByID(id, v, ignoreseekspeed: true)` / `getParameterByID(id, out v)`
- ✅ **Readback-паттерн:** после set читать значение обратно и логировать `sent X / readback Y`. Совпало → значение дошло (если звук не меняется — проблема в кривой automation в FMOD). Не совпало → автодамп всех глобальных параметров (`getParameterDescriptionCount` + `getParameterDescriptionList`)
- ✅ `ignoreseekspeed: true` — обязательно, иначе ненулевой Seek Speed параметра растягивает установку (значение «ползёт», а не ставится)
- Глобальный параметр ставить через `RuntimeManager.StudioSystem`, инстансный — через `instance`. Не смешивать: у ивента может быть scoped-двойник с тем же именем
- RTPC из кода управляет только Game Parameter (`parameterType: 0`); автоматические (1–9) и unexposed (`isExposedRecursively: false`) — нет
- Имя параметра в FMOD Studio держать чистым (без хвостовых пробелов) — код устойчив к пробелам через Trim+ID, но гигиена обязательна

### Банки — пути, сборка, копирование

**Пути (общие):**
- Проект FMOD Studio: `<путь к проекту>/<имя>.fspro`
- Сборка банков (Build): `<папка проекта>/Build/<Платформа>/`
- Unity (куда должны попасть): `<Unity-проект>/Assets/StreamingAssets/<Платформа>/`
- **Банки НЕ копируются автоматически** после Build (Ctrl+B) в FMOD Studio — копировать вручную или настраивать post-build step

**Копирование (шаблон):**
```powershell
Copy-Item "<папка FMOD-проекта>\Build\<Платформа>\*.bank" "<Unity-проект>\Assets\StreamingAssets\<Платформа>\" -Force
```

**Состав банков:**
- `Master.bank` + `Master.strings.bank` + ивентовые банки. В StreamingAssets должны быть все банки из Build.
- RuntimeManager при старте **сам грузит все банки из StreamingAssets** (ImportType=StreamingAssets) — ручной список банков нужен только как fallback.
- Hot reload: выгружать только ивентовые банки. **Master и Master.strings НЕ выгружать** — Master.strings хранит пути ивентов, без него GetEventDescription перестаёт резолвить.
- Runtime-проверка загрузки: `RuntimeManager.HasBankLoaded(name)`
- ⚠️ **`HasBankLoaded` ≠ сэмплы в памяти.** При `BankLoadType=All` RuntimeManager зовёт `LoadBank(name)` с `loadSamples=false`. На WebGL метаданные (Master.bank) готовы раньше, чем sample data из `Master.assets.bank`. `CreateInstance`/`start`/`setParameter` работают, звука нет. Нужно: `AutomaticSampleLoading=1` и/или повторный `LoadBank(name, loadSamples: true)` даже если `HasBankLoaded==true` (иначе `loadSampleData` не вызовется), ждать `HasBankLoaded("Master.assets")` + `!AnySampleDataLoading()` + `EventDescription.getSampleLoadingState == LOADED` перед созданием инстансов.
- ⚠️ **`LoadBank("Master", loadSamples: true)` ДО `HasBankLoaded("Master.assets")`** → `EventModelPreloadVisitor::visitWaveformInstrumentModel : WaveformResourceModel (Asset) not found` + `assertion: 'resource' failed`. Сэмплы дожимать только когда ВСЕ банки (Master + strings + assets) уже в `loadedBanks`.
- ⚠️ **WebGL unlock**: `mixerSuspend`/`Resume` только после **реального клика в Unity**, когда FMOD уже init. HTML «Играть» для этого бесполезен (user activation сгорает за загрузку wasm). Ранний suspend + `OnApplicationPause` → спам `FMOD_OS_CriticalSection_* NULL` каждый кадр. Паттерн: банки в лоадере → `WaitUntilUnlockedAsync` (клик по лоадеру) → fade. `RuntimeManager.AllowMixerSuspendResume=false` до unlock.
- Лоадер (`Startup` + `loadingScreen`): держать экран до `await audio.WaitUntilReadyAsync()`, потом `InitializeGameAsync` и fade-out. Ждать `HaveAllBanksLoaded` + `HasBankLoaded(Master/Master.assets)` + `!AnySampleDataLoading()` + sample state LOADED (BankLoadType=All, AutomaticSampleLoading=1 уже в Settings).
- ⚠️ **`IsPlaying(MusicMatch)` ≠ `_persistentReady`.** Инстансы после WaitUntilReady уже валидны, но `start()` музыки ещё нет. Если `IsPlaying` смотрит только на ready — `AudioService` пропускает `Play(MusicMatch)` → SFX есть, музыки нет. Правильно: `_musicStarted`.

### Как проверить путь ивента (по метаданным FMOD-проекта)

1. **Реальный путь ивента:** искать имя ивента в `Metadata/Event/*.xml` FMOD-проекта (`<property name="name">`), затем relationship `folder` → destination → файл в `Metadata/EventFolder/*.xml`. Если папка — `MasterEventFolder`, ивент в корне → путь `event:/ИмяИвента`; если ивент в подпапке → путь включает папки (`event:/Папка/ИмяИвента`).
2. **Строки внутри .bank сжаты** — regex по байтам Master.strings.bank пути ивентов НЕ находит. Пути ивентов проверять ТОЛЬКО по метаданным FMOD-проекта (Metadata/Event/*.xml), не по байтам банка.

### Ссылки в сценах Unity (.unity)

- Ссылку компонента проверять в YAML сцены: найти блок MonoBehaviour по имени поля. `{fileID: 0}` = ссылка пустая (null). Guid ассета/скрипта из ссылки искать по `*.meta` файлам.
- **Cross-scene сериализованные ссылки невозможны** — ссылка из одной сцены на объект другой сцены всегда `{fileID: 0}`. При additive-загрузке сцены компоненты должны сами находить кросс-сценовые зависимости (`FindFirstObjectByType`) или лениво кешировать.

### Мониторинг в реальном времени (Live Update)

1. В Unity: **FMOD → Edit Settings** → в секции нужной платформы (или Default) включить **Live Update** (галочка tri-state), под ней появится **Live Update Port** — порт по умолчанию **9264**. Галочки per-platform — включать в секции той платформы, на которой идёт игра.
2. Запустить Play в Unity.
3. В FMOD Studio: **Window → Connect** (F5) → выбрать игру в списке → Connect.
4. Открыть ивент — в микшере в реальном времени видно уровни дорожек и текущие значения RTPC-параметров.
5. **Window → Profiler** — голоса, CPU, все инстансы с их параметрами.
6. Оверлей поверх игры: там же в FMOD Settings включить **Debug Overlay** — в углу Game View видно играющие ивенты и их параметры.

### Сводка известных ошибок API FMOD 2.03

- ❌ `bank.getName(out name)` — метода нет у Bank → ✅ `bank.getPath(out path)`
- ❌ `eventDescription.getBankList(out banks)` — нет в FMOD 2.03 → ✅ `RuntimeManager.StudioSystem.getBankList` → перебор → `bank.getEventList`
- ❌ `instance.setParameter("name", value)` — нет → ✅ `instance.setParameterByName("name", value)`
- ❌ `if (instance == null)` — EventInstance struct → ✅ `if (instance.isValid())`
- ❌ `STOP_MODE.ALLOWFADEOUT` — конфликт имён → ✅ `FMOD.Studio.STOP_MODE.ALLOWFADEOUT`
- ❌ Печать `desc.name` напрямую — увидишь `FMOD.StringWrapper` вместо имени → ✅ `(string)desc.name` — каст PARAMETER_DESCRIPTION.name к string

## Запрещено

- Предлагать альтернативные решения без запроса
- Изменять логику без прямого указания
- Предлагать вкладки/методы, которые уже удалены из FMOD/Unity
- Игнорировать прямые инструкции

## Разрешено

- Искать ошибки и указывать на них (по запросу)
- Оптимизировать код (по запросу)
- Дебаггинг (по запросу)
- Разрабатывать инструменты (по запросу)
