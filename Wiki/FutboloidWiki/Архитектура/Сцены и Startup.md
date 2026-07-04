---
tags:
  - architecture
  - scenes
  - startup
aliases:
  - Сцены и Bootstrap
  - Bootstrap
---

# Сцены и Startup

← [[Обзор архитектуры]] | [[Индекс архитектуры]]

## Принцип

Несколько сцен **одновременно загружены**. Глобальное — в **root**, геймплей — в **отдельной аддитивной** сцене.

```
┌─────────────────────────────────────────────┐
│  RootScene (создаётся в runtime)            │
│  ├─ [DI] Root LifetimeScope                 │
│  ├─ AudioService                            │
│  ├─ EventSystem                             │
│  ├─ SceneTransition (DontDestroyOnLoad UI)  │
│  ├─ UIService / Canvas overlay root         │
│  └─ GameDirector                            │
├─────────────────────────────────────────────┤
│  GameScene (additive, всегда после startup) │
│  ├─ Поле, ворота, мяч                       │
│  ├─ Вратарь игрока                          │
│  ├─ Боты (фоновая симуляция)                │
│  ├─ View на поле (Ball, Goalkeeper, Defenders) │
│  └─ [DI] Game LifetimeScope (child)         │
└─────────────────────────────────────────────┘
```

## Build Settings (целевые)

| # | Сцена | Назначение |
|---|-------|------------|
| 0 | `Startup.unity` | **Единственная** стартовая сцена в билде |
| — | `Game.unity` | Загружается **аддитивно** из кода (не в build index обязательно, но можно для превью) |

> Сейчас в проекте: `MainMenu.unity` + `SampleScene.unity`. При миграции `MainMenu` **убираем из билда** — UI переезжает в оверлей. См. [[Миграция с текущего кода]].

## Startup.unity

Минимальная сцена. Один объект с компонентом `Startup`:

```csharp
// Псевдокод — целевая форма
public class Startup : MonoBehaviour
{
    void Awake() => GameStartupHandler.OnGameStart();
}
```

Дублирование защиты: `[RuntimeInitializeOnLoadMethod]` + `Startup.Awake` с guard «уже инициализировано».

## RootScene (runtime)

Создаётся в `AppRootState.Enter()`:

1. `SceneManager.CreateScene("RootScene")`
2. `SetActiveScene(RootScene)` — активная сцена для создания новых объектов
3. Перенос / создание persistent-объектов
4. Создание `Root LifetimeScope`

### Что живёт в Root

| Объект / сервис | Зачем глобально |
|-----------------|-----------------|
| `LifetimeScope` (root) | DI-контейнер на всё приложение |
| `AudioService` | Музыка между «режимами» |
| `EventSystem` | UI input |
| `UIService` + overlay canvas | Меню, пауза, HUD, турнир |
| `SceneTransitionView` | Шторки + мяч |
| `GameDirector` | FSM приложения |
| `ISaveStorage` | Сохранения / лидерборд |

## GameScene (additive)

Загружается в `AppGameState.Enter()` / `GameState.Enter()`:

```csharp
await SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
SceneManager.SetActiveScene(gameScene);
// …
await Overlay.SetState(NavigationState.OnField);  // cold start — сразу матч, без MainMenu
```

### Что живёт в Game

| Содержимое | Примечание |
|------------|------------|
| Поле, коллайдеры / хитбоксы | Статика уровня (не dynamic physics для мяча) |
| View + registry | Мяч, вратарь, защитники — см. [[Связь сцены с кодом]] |
| Вратарь, мяч, защитники | Геймплейные объекты |
| `BotSimulationController` | Боты играют, когда игрок в меню или не управляет (post-MVP) |
| `Game LifetimeScope` | Child DI: `MatchFlow`, `PitchStateMachine` |

**Главное меню здесь НЕ сцена** — только контент поля. Меню рисуется из Root UI поверх.

Детальная расстановка объектов, ворот и якорей: [[Сборка поля Game]].

## Когда нужна перезагрузка сцены

| Ситуация | Действие |
|----------|----------|
| Рестарт матча | **Не** выгружать GameScene — `PitchResetRequestedEvent` → `PitchStateMachine.Reset()` + `MatchFlow.Reset()` |
| Рестарт турнира | `AppGameState.Exit()` → `Enter()` или soft reset |
| Выход в «главное меню» | Показать overlay, боты продолжают / перезапускают фоновую игру |
| Полный рестарт приложения | Только dev / крайний случай |

Тяжёлые переходы (если появится отдельный контент турнира) — через [[../GDD/05 Меню UI и переходы#5.3. Анимация перехода между сценами (Scene Transition)|Scene Transition]].

**SceneLinks не используем** — view на поле через registry / Bind. См. [[Связь сцены с кодом]].
