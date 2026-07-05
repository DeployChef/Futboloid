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
│  Root.unity (единственная сцена в Build)    │
│  ├─ Startup + RootLifetimeScope             │
│  ├─ AudioPlaybackHost                       │
│  ├─ EventSystem                             │
│  ├─ UIService / Canvas (меню, пауза)        │
│  └─ GameDirector (DI)                       │
├─────────────────────────────────────────────┤
│  Game.unity (additive из AppGameState)      │
│  ├─ Поле, ворота, мяч, защитники            │
│  ├─ Match HUD, Run XP, BonusPick, Tournament│
│  └─ Game LifetimeScope (child)              │
└─────────────────────────────────────────────┘
```

## Build Settings (актуально)

| # | Сцена | Назначение |
|---|-------|------------|
| 0 | `Root.unity` | **Единственная** сцена в билде |
| — | `Game.unity` | Загружается **аддитивно** из `AppGameState` |

## Root.unity / Startup

Сцена `Root.unity` — не создаётся в runtime. Один объект с `Startup` + `RootLifetimeScope`:

```csharp
// Startup.Awake — актуально
rootScope.Build();
rootScope.Container.Resolve<IGameDirector>().InitializeGame();
```

Guard от двойного init: статический флаг `_started` в `Startup`.

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
