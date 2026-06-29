---
tags:
  - architecture
  - ui
---

# UI и оверлеи

← [[Обзор архитектуры]] | [[Машины состояний]]

Главный принцип: **все экраны — оверлеи на Root Canvas**, игровая сцена всегда снизу.

## Слои canvas (sorting order)

| Order | Слой | Когда виден |
|-------|------|-------------|
| 0 | Game world | Всегда (после boot) |
| 100 | Match HUD | `Navigation.OnField` |
| 200 | Main Menu | `Navigation.MainMenu` |
| 300 | Tournament | `Navigation.Tournament` |
| 400 | Pause | `Navigation.Pause` |
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
uiService.Show<MatchHudWidget>();
uiService.Close<MainMenuWidget>();
```

Виджеты — prefab в `Resources/UI/` или Addressables позже.

## Match HUD

Элементы из [[../GDD/06 HUD и визуальный фидбек|GDD §6]]:

- Таймер 90 с
- Счёт + комбо
- Стек баффов

Подписывается на `MatchFlow` / `ComboScoreService`, не на `Update` напрямую.

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
