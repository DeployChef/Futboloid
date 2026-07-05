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
| 100 | **Match HUD** | **Game** | Таймер, счёт — `MatchHudWidget` |
| 110 | **Run XP HUD** | **Game** | Шкала XP забега — `RunXpHudWidget` |
| 130 | **BonusPick** | **Game** | `PitchPhase.BonusPick` — 3 карты перков |
| 150 | **Tournament** | **Game** | `Navigation.Tournament` (после матча) |
| 200 | Main Menu | Root | `Navigation.MainMenu` |
| 210 | Pause | Root | `Navigation.Pause` — **только во время забега** |
| 300 | Shop / прочее | Root | по Navigation |
| 1000 | Scene Transition | Во время переходов |
| 1100 | Loading / blocking | Startup |

## Главное меню ≠ отдельная сцена

### Поведение (MVP)

- Поле **уже загружено** под меню
- **Заморозка:** `timeScale = 0` через `PauseCoordinator` (`PauseReasons.MainMenu`)
- Игрок видит **статичный кадр** поля под меню

> [!note] Post-MVP
> В оригинальном дизайне — живые боты на фоне (`BotSimulationController`, `timeScale = 1`). Для MVP сознательно выбрана **заморозка** — проще, меньше кода. Вернуть живой фон — отдельная задача после `BotSimulationController`.

### Отличие главного меню от паузы

> [!important] Главное меню и Escape-пауза — разные состояния
> Оба ставят `timeScale = 0`, но через **разные причины** в `PauseCoordinator`. Семантика и UI различаются.

| | Main Menu | Pause (Escape) |
|---|-----------|----------------|
| Когда | Старт приложения / выход из турнира в меню | **Только во время забега** (есть run) |
| Пауза симуляции | **Да** — `PauseReasons.MainMenu` | **Да** — `PauseReasons.EscapePause` |
| Match HUD (таймер, счёт) | **Скрыт** | **Виден** под оверлеем |
| Лидерборд | **Да** — панель рекордов (позже) | Нет |
| Фон | Замороженный кадр поля | Замороженный кадр матча |
| Кнопки | Играть, Настройки | Продолжить, Рестарт забега, В меню |

`Navigation.Pause` и `Navigation.MainMenu` — **разные** состояния и **разные** виджеты (`PauseWidget` ≠ `MainMenuWidget`). MVP мог временно шарить разметку, но семантика и поведение различаются.

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
| `OnField` (забег) | **Виден** |
| `Pause` (забег) | **Виден** — оверлей поверх |
| `MainMenu` | **Скрыт** — поле заморожено под меню |
| `Tournament` | **Скрыт** — сетка перекрывает |

`MatchHudController` обновляет слайдер/счёт с шины; скрывает HUD только при `Tournament` (сетка перекрывает). Детали: [[MatchFlow и таймер#HUD: слайдер на сцене Game]].

## BonusPick overlay (level-up в матче)

На **`Game.unity`**, sorting order **130** — между Match HUD (100) и Tournament (150).

| PitchPhase | BonusPick overlay |
|------------|-------------------|
| `BonusPick` | **Виден** — затемнение + 3 карточки |
| Остальные | Скрыт |

Показывается при level-up / `PitchPhase.BonusPick`. Пауза через `PauseCoordinator` (`PauseReasons.BonusPick`). Карточки — prefab + `PerkDefinition` SO.

Tween карточек при появлении — **`SetUpdate(true)`** (unscaled), т.к. игра на паузе.

## Tournament overlay (после матча)

Тоже на **`Game.unity`**, не Root — часть игрового поля, как HUD.
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

## Пауза и timeScale

Все паузы — через [[PauseCoordinator]]. Прямой `Time.timeScale = …` в новом коде **запрещён**.

При `timeScale = 0` tween UI:

```csharp
.SetUpdate(true); // unscaled
```

Зафиксировать в коде `PauseOverlay` и `SceneTransitionView`.

## BotSimulationController (post-MVP)

> [!info] Не в MVP
> Сейчас главное меню **замораживает** поле. Блок ниже — целевое поведение на будущее.

Когда `Navigation == MainMenu` и включён `BotSimulationController`:

- Включить AI вратаря и простых ботов — **фоновая атмосфера**
- Отключить управление игрока
- **Без** `MatchFlow`: нет счёта, нет 90-секундного таймера, нет `TournamentRunService` / забега
- Мяч в упрощённом автономном режиме (пинают друг друга, без голов в зачёт матча)

Когда «Играть» → `OnField` + старт **забега** (`TournamentRunService.ResetRun`) — боты player-side отключаются, включается Match HUD, полный матч.

Когда `Navigation == Pause` (только при активном забеге) — `BotSimulationController` **выключен**, поле заморожено (`timeScale = 0`).
