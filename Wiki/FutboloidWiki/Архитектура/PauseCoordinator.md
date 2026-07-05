---
tags:
  - architecture
  - pause
aliases:
  - Пауза
  - TimeScale
---

# PauseCoordinator

← [[UI и оверлеи]] | [[Машины состояний]] | [[DI и LifetimeScope]]

> [!date] Добавлено
> **05.07.2026** — единая точка управления `Time.timeScale`.

## Зачем

Раньше `Time.timeScale` выставляли напрямую в `OverlayStateController` и `BonusPickCoordinator`. При нескольких причинах паузы одна система могла снять паузу, пока другая ещё ждёт заморозки.

`PauseCoordinator` (App scope) ведёт **набор активных причин** и выставляет `timeScale = 0`, пока набор не пуст.

## API

```csharp
_pause.Request(PauseReasons.MainMenu);   // заморозить
_pause.Release(PauseReasons.MainMenu);   // снять эту причину
_pause.ReleaseAll();                     // сброс при выходе из AppGameState
```

Константы: `Futboloid.Core.Pause.PauseReasons`

| Причина | Кто запрашивает |
|---------|-----------------|
| `MainMenu` | `OverlayStateController` при `Navigation.MainMenu` |
| `EscapePause` | `OverlayStateController` при `Navigation.Pause` |
| `BonusPick` | `BonusPickCoordinator` на выбор перка |

## Правила

- **Не** вызывать `Time.timeScale` напрямую из нового кода — только через координатор.
- UI-твины при паузе: `SetUpdate(true)` (unscaled) — см. [[UI и оверлеи#Пауза и DOTween]].
- При `AppGameState.Exit` — `ReleaseAll()`, чтобы следующий заход не стартовал с `timeScale = 0`.

## DI

```csharp
// AppScopeExtensions
builder.Register<PauseCoordinator>(Lifetime.Singleton);
```

Game scope резолвит из parent App scope (`BonusPickCoordinator`).
