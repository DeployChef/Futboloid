---
tags:
  - architecture
  - ui
---

# UI и оверлеи

← [[Обзор архитектуры]] | [[Машины состояний]]

Главный принцип: **Root** — только глобальные оверлеи (меню, магазин…). **Game** — всё, что относится к полю: Match HUD, турнирная сетка после матча.

## Слои canvas (sorting order)

| Order | Слой | Сцена | Когда виден |
|-------|------|-------|-------------|
| 0 | Game world | Game | Пока загружена Game |
| 100 | **Match HUD** | **Game** | Пока на поле: `MainMenu`, `OnField`, `Pause` |
| 150 | **Tournament** | **Game** | `Navigation.Tournament` (после матча) |
| 200 | Main Menu / пауза | Root | `Navigation.MainMenu` |
| 300 | Shop / прочее | Root | по Navigation |
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
- Стек баффов (позже, на том же Canvas Game)

**Часть поля, не Root-оверлей.** Живёт на **`Game.unity`**, sorting order **100**. Главное меню и пауза (Root, order 200) рисуются **поверх** HUD — таймер и счёт остаются видны под полупрозрачным меню (или по краям, если меню не перекрывает).

| Navigation | Match HUD |
|------------|-----------|
| `MainMenu` (старт, не пауза) | **Виден** — поле + боты под меню |
| `OnField` | **Виден** |
| `Pause` / `MainMenu` + `IsMatchPausedInMenu` | **Виден** — меню поверх поля |
| `Tournament` | **Скрыт** — ушли с экрана матча |

`MatchHudController` обновляет слайдер/счёт с шины; скрывает HUD только при `Tournament` (сетка перекрывает). Детали: [[MatchFlow и таймер#HUD: слайдер на сцене Game]].

## Tournament overlay (после матча)

Тоже на **`Game.unity`**, не Root — часть игрового поля, как HUD.

| Navigation | Tournament overlay |
|------------|-------------------|
| `Tournament` | **Виден** — сетка, счёт матча, кнопка **МАТЧ!** |
| Остальное | Скрыт (`TournamentController` по `NavigationChangedEvent`) |

`TournamentController` + `TournamentWidget` / `TournamentLayout`. Данные — `IGameDirector.TournamentBracket` (App `TournamentRunService`). Кнопка **МАТЧ!** → `GoOnField()`.

Root `UIService` турнир **не** показывает — только закрывает MainMenu при переходе в `Tournament`.

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
