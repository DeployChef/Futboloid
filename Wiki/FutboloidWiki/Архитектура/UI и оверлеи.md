---
tags:
  - architecture
  - ui
---

# UI и оверлеи

← [[Обзор архитектуры]] | [[Машины состояний]]

Главный принцип: **навигационные оверлеи (меню, турнир, магазин…) — Root Canvas**; **Match HUD — только на сцене Game**, потому что матч есть не везде.

## Слои canvas (sorting order)

| Order | Слой | Сцена | Когда виден |
|-------|------|-------|-------------|
| 0 | Game world | Game | Всегда (после boot) |
| 100 | Match HUD | **Game** | `Navigation.OnField` |
| 200 | Main Menu | Root | `Navigation.MainMenu` |
| 300 | Tournament | Root | `Navigation.Tournament` |
| 400 | Pause / Shop / … | Root | по Navigation |
| 1000 | Scene Transition | Во время переходов |
| 1100 | Loading / blocking | Startup |

## Главное меню ≠ отдельная сцена

### Поведение

- Поле **уже загружено** под меню
- **Боты** играют матч без участия игрока (упрощённый AI)
- Игрок видит «живой» фон — атмосфера стадиона

### Отличие главного меню от паузы

| | Main Menu | Pause (Escape) |
|---|-----------|----------------|
| Когда | Старт / выход из турнира | Во время матча |
| Лидерборд | **Да** — панель рекордов | Нет |
| Фон | Боты играют, timeScale 1 | timeScale **0** |
| Кнопки | Играть, Настройки, (Зал славы встроен) | Продолжить, Рестарт, Настройки, В меню |

> [!note] GDD
> В [[../GDD/05 Меню UI и переходы]] зал славы и меню описаны для отдельной сцены. В архитектуре зал славы — **панель внутри Main Menu overlay**.

## UIService

Один сервис, generic Show/Close:

```csharp
uiService.Show<MainMenuWidget>();
uiService.Close<MainMenuWidget>();
```

Только **Root-оверлеи** регистрируются в `UIService`. Match HUD — **не здесь** (см. [[MatchFlow и таймер#HUD: слайдер на сцене Game]]).

## Match HUD

Элементы из [[../GDD/06 HUD и визуальный фидбек|GDD §6]]:

- Таймер 90 с (слайдер)
- Счёт + комбо (комбо — позже)
- Стек баффов

**Не** опрашивает `MatchFlow` в `Update`. На сцене **Game**: `MatchHudController` слушает шину (`MatchTimerChangedEvent`, `MatchScoreChangedEvent`, `NavigationChangedEvent`). Детали: [[MatchFlow и таймер#HUD: слайдер на сцене Game]].

Стек баффов — тот же принцип «события + локальная анимация», см. [[Прогрессия и эффекты#7. HUD — события + анимация кольца]].

## Scene Transition

Используется **не** для главного меню (оно уже на месте), а для:

- Вход / выход из **турнирной сетки** (если визуально «уезжаем» с поля)
- Рестарт турнира с полной перезагрузкой (редко)

Алгоритм из GDD: шторки → крутящийся мяч → `LoadSceneAsync` (если нужно) → шторки открываются.

При нашей модели чаще достаточно **оверлея без load** — transition для polish.

## Пауза и DOTween

При `Time.timeScale = 0` все tween UI:

```csharp
.SetUpdate(true); // unscaled
```

Зафиксировать в коде `PauseOverlay` и `SceneTransitionView`.

## BotSimulationController

Когда `Navigation == MainMenu`:

- Включить AI вратаря и простых ботов
- Отключить `GoalkeeperController` игрока
- Мяч в автономном режиме (ограниченные правила, без прогрессии игрока)

Когда переход в `OnField` — боты player-side отключаются, управление игроку.
