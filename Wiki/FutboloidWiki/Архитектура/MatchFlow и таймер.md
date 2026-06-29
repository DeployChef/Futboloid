---
tags:
  - architecture
  - match
  - timer
  - hud
aliases:
  - MatchFlow
  - Таймер матча
---

# MatchFlow и таймер матча

← [[Индекс архитектуры]] | [[Машины состояний]] | [[Шина событий]]

**Scope:** Game (`MatchFlow` в Game LifetimeScope).

Отвечает за **счёт голов** и **обратный отсчёт 90 с**. `PitchStateMachine` не тикает таймер — только слушает **`MatchEndedEvent`** и переходит в `MatchEnded`.

Связано: [[UI и оверлеи#Match HUD]], [[../GDD/02 Игровой цикл#Окончание матча|GDD: окончание матча]], [[Прогрессия и эффекты#7. HUD — события + анимация кольца|HUD на событиях (аналогия с баффами)]].

> [!note] Статус
> **Реализовано (MVP):** корутина таймера, события шины, `MatchHudBridge` → слайдер. Комбо, доп. время по правилам футбола — позже.

---

## Разделение ответственности

| Класс | Что делает | Чего не делает |
|-------|------------|----------------|
| **`MatchFlow`** | Счёт, таймер, `MatchEndedEvent`, сдвиг времени | Не двигает мяч, не знает про UI |
| **`PitchStateMachine`** | Фазы поля (`KickoffWait`, `Simulating`…) | Не считает секунды |
| **`MatchHudController`** (Game) | Подписка на bus, Show при `OnField` | Не в `UIService` |
| **`MatchHudWidget`** | Слайдер, тексты | Не знает про Navigation напрямую |

**XP забега, перки, timed-баффы** — не здесь. См. [[Прогрессия и эффекты]].

---

## Принцип таймера (как у баффов в §7)

**Источник правды** — `MatchFlow` (одна фоновая **UniTask-корутина**).  
**HUD** не опрашивает сервис в `Update` и **нет** внешнего `ITickable` / `Tick(dt)` снаружи.

| Что | Кто решает |
|-----|------------|
| Сколько секунд осталось | `MatchFlow` (корутина + `AdjustTime`) |
| Как выглядит полоска | `MatchHudLayout` → `Slider` по **`MatchTimerChangedEvent`** |
| Матч реально кончен | `MatchFlow` → **`MatchEndedEvent`** → `PitchStateMachine` |

```mermaid
sequenceDiagram
    participant Nav as Navigation OnField
    participant MF as MatchFlow
    participant Bus as IGameEventBus
    participant Bridge as MatchHudBridge
    participant HUD as MatchHudWidget

    Nav->>MF: NavigationChangedEvent
    MF->>MF: StartTimerLoop (UniTask)

    loop каждый кадр на поле
        MF->>MF: remaining -= Time.deltaTime
        MF->>Bus: MatchTimerChangedEvent
        Bus->>Bridge: normalized, seconds
        Bridge->>HUD: SetTimer(slider)
    end

    Note over MF: или AdjustTime(-N) / до 0
    MF->>Bus: MatchEndedEvent
    Bus->>MF: PitchStateMachine → MatchEnded
```

### Почему корутина, а не внешний тик

- Один владелец времени — проще пауза, сброс, доп. время.
- Параллельно с геймплеем: корутина крутится, остальной код **только слушает события**.
- Сдвиг времени (`+15` доп. время) — одно событие, без правок HUD.

---

## Жизненный цикл корутины

| Событие | Поведение |
|---------|-----------|
| `Navigation → OnField` | `StartTimerLoop()` (если матч не кончен) |
| Пауза (`MainMenu`, `timeScale = 0`) | `StopTimerLoop()` — секунды **сохраняются** |
| Continue → `OnField` | снова `StartTimerLoop()` с тем же `RemainingSeconds` |
| `Pitch.Reset()` (новый Play) | стоп корутины, счёт и таймер → 90 с |
| `RemainingSeconds ≤ 0` | `MatchEndedEvent`, стоп корутины |
| `PitchPhase → MatchEnded` | стоп корутины, флаг конца матча |

Корутина использует `Time.deltaTime` → при `timeScale = 0` отсчёт замирает даже если цикл формально жив (на паузе цикл **останавливаем** через `CancellationToken`).

---

## События (шина)

```csharp
public readonly struct MatchTimerChangedEvent
{
    public float RemainingSeconds { get; }
    public float Normalized { get; }   // Remaining / TotalDuration (растёт при доп. времени)
}

public readonly struct MatchScoreChangedEvent
{
    public int PlayerScore { get; }
    public int OpponentScore { get; }
}

public readonly struct MatchEndedEvent
{
    public int PlayerScore { get; }
    public int OpponentScore { get; }
}

/// <summary>+N доп. время, −N штраф. Публикует любой геймплейный код.</summary>
public readonly struct MatchTimeAdjustedEvent
{
    public float DeltaSeconds { get; }
    public string Reason { get; }
}
```

`MatchFlow` публикует:

- **`MatchTimerChangedEvent`** — из корутины после каждого шага (для плавного слайдера) и после `AdjustTime` / `Reset`
- **`MatchScoreChangedEvent`** — при голе (`GoalScoredEvent` → `RecordGoal`)
- **`MatchEndedEvent`** — при `RemainingSeconds ≤ 0` (в т.ч. после отрицательного `AdjustTime`)
- слушает **`MatchTimeAdjustedEvent`** → `AdjustTime(delta, reason)`

### Доп. время

```csharp
_bus.Publish(new MatchTimeAdjustedEvent(15f, "stoppage"));
```

При **положительном** `DeltaSeconds` увеличивается `_totalDurationSeconds` — слайдер остаётся в диапазоне 0…1 (полоска может «подрасти» визуально).

Отрицательный сдвиг — штраф / ускорение конца матча.

---

## HUD: слайдер на сцене Game

Match HUD **не на Root** — он живёт только там, где есть матч (**`Game.unity`**). Магазин, турнирная сетка и прочие оверлеи на Root **не тащат** за собой HUD матча.

```
Game.unity
└── UI / MatchHud          ← Canvas (Screen Space Overlay)
    ├── MatchHudController  IGameSceneInitializable
    ├── MatchHudWidget
    └── TimerSlider, тексты…
```

Инициализация — как у `GoalkeeperView`: `GameState` → `Initialize(bus)`.

| Компонент | Где | Роль |
|-----------|-----|------|
| `MatchHudController` | Game scene | шина → виджет; виден только при `Navigation.OnField` |
| `MatchHudWidget` + `MatchHudLayout` | Game scene | слайдер 0…1, тексты счёта/секунд |

**Не** регистрируется в `UIService` на Root — показ/скрытие по `NavigationChangedEvent`.

**Inspector (слайдер):** Min = 0, Max = 1, Value = 1, Interactable = off.

Виджет **не** слушает `MatchEnded` — конец матча обрабатывает `PitchStateMachine`.

---

## Связь с Pitch FSM

```mermaid
flowchart LR
    MF[MatchFlow корутина] -->|MatchEndedEvent| PSM[PitchStateMachine]
    PSM -->|PitchPhaseChangedEvent| Views[BallView, Keeper…]
    GS[GoalScoredEvent] --> MF
    GS --> PSM
```

- Гол: `PitchStateMachine` → `Reshuffle` → `KickoffWait` (таймер **не** сбрасывается).
- Новый матч: `OverlayStateController` → `Pitch.Reset()` → `MatchFlow.Reset()` + `KickoffWait`.

---

## Чего нет в MVP (задел)

| Фича | Как добавить позже |
|------|---------------------|
| Доп. время при голе в концовке | подписчик публикует `MatchTimeAdjustedEvent` |
| Остановка таймера только в `KickoffWait` | условие в корутине по `PitchPhase` |
| Анимация счёта (bounce) | `MatchHudLayout` на `MatchScoreChangedEvent` + DOTween |
| `ComboScoreService` | отдельные события, тот же `MatchHudController` |

---

## Код

| Файл | Сборка |
|------|--------|
| `Futboloid.Gameplay/Match/MatchFlow.cs` | Game |
| `Futboloid.Gameplay/Bus/Events/Match*.cs` | Game |
| `Futboloid.Main/UI/MatchHudController.cs` | Game scene |
| `Futboloid.UI/Views/MatchHud/*` | Game scene UI |

См. также [[Шина событий]], [[DI и LifetimeScope#Game scope]].
