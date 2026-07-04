---
tags:
  - architecture
  - scene
  - field
  - goals
aliases:
  - Game.unity
  - Сборка поля
  - Ворота на сцене
---

# Сборка поля (`Game.unity`)

← [[Сцены и Startup]] | [[Связь сцены с кодом]] | [[Индекс архитектуры]]

Чеклист **редакторской** сборки игровой сцены. Код пишется отдельно; здесь — иерархия, слои, коллайдеры, сортировка, якоря.

Портрет: вратарь **внизу**, ворота соперника **вверху**.

---

## Иерархия

```text
Game
├── Main Camera
├── Environment
│   └── Field                    ← Sprite Renderer только
├── PlayArea
│   ├── WallLeft
│   ├── WallRight
│   └── WallTop
├── Goals
│   ├── GoalEnemy
│   │   ├── Visual               ← спрайт ворот (трапеция)
│   │   ├── Frame                ← Edge Collider 2D (рама, отскок)
│   │   └── ScoringZone          ← trigger гола игрока
│   └── GoalPlayer               ← trigger гола соперника (низ, без спрайта)
├── BallKickoffAnchor            ← фикс. старт мяча + стрелка направления
│   └── DirectionArrow           ← визуал; transform.up = куда полетит мяч
├── PlayerGoalkeeper
│   └── Visual
├── Ball
│   └── Visual
├── Defenders                    ← команда соперника (GDD §7)
│   ├── DefenderGridRegistry
│   ├── GoalAnchor               ← зона ворот (не юнит)
│   ├── DefenderSlots            ← DefenderSlotLayout, Slot_0…Slot_34 (5×7)
│   ├── Spawned                  ← Spawn Root: инстансы от DefenderSpawner
│   └── DefenderSpawner
└── UI / GameplayInputHost …     ← см. Root для Canvas
```

> [!important] Не вешать врагов руками
> Полевые и GK **спавнятся** при `PitchResetRequestedEvent`. На сцене только слоты и `GoalAnchor`. См. [[Генерация врагов]].

Старое имя `Opponents` **не** используем — корень `Defenders`.

На **Game** нет: `EventSystem`, Canvas меню, `GameManager`, старый `Ball` с `Rigidbody2D`. UI — на **Root**.

---

## Слои физики (User Layers)

| Layer | Кто |
|-------|-----|
| `Wall` | борта, **рама ворот** (`Frame`) |
| `Keeper` | коллайдер вратаря |
| `Defender` | футболисты соперника (поле и вратарь — один prefab, один collider) |
| `GoalEnemy` | `ScoringZone` — гол **игрока** (верх) |
| `GoalPlayer` | зона гола **соперника** (низ) |

### Теги

| Tag | Кто |
|-----|-----|
| `Player` | только `PlayerGoalkeeper` |
| остальное | `Untagged` |

Голы и отскоки определяем по **слоям**, не по тегам (`GoalMe` / `GoalEnemy` в коде **не** используем).

---

## Sorting Layers

| Layer | Кто |
|-------|-----|
| `Field` | спрайт поля |
| `Gameplay` | мяч, вратарь, ворота, защитники |
| `FX` | эффекты (позже) |

**Глубина по Y** внутри `Gameplay` (Renderer2D, ось Y) — мяч может быть **за** или **перед** вратарём/воротами. **Не** использовать отдельный слой «Ball всегда сверху».

**Sorting Group** — только на статике (`Field`). На мяче и вратаре **не** вешать (ломает перекрытие по Y).

---

## Борта (`PlayArea`)

Каждая стена: **Empty** + `BoxCollider2D`, **без** спрайта.

| Объект | Layer | Trigger |
|--------|-------|---------|
| `WallLeft`, `WallRight`, `WallTop` | `Wall` | OFF |

- **`WallTop`** — **не** перекрывает проём ворот (задняя стена выше рамы или сегменты по бокам).
- **Нижнего борта нет** — вместо него зона `GoalPlayer`.

---

## Ворота соперника (`GoalEnemy`)

Спрайт ворот **больше** триггера — нормально: картинка = рама + декор, триггер = только **проём**.

### `Visual`

- `Vorota.png`, Sorting Layer `Gameplay`
- **Без** коллайдера

### `Frame` — отскок (трапеция)

- **`Edge Collider 2D`**, несколько точек (П-образная ломаная: левая штанга → перекладина → правая штанга)
- **Не замыкать** низ — проём для гола открыт
- Layer **`Wall`**, Trigger **OFF**
- Один Edge на всю раму — допустимо

### `ScoringZone` — гол игрока

- `BoxCollider2D` или `PolygonCollider2D`
- Layer **`GoalEnemy`**, Trigger **ON**
- Размер **меньше** спрайта — только внутренний проём трапеции

```text
        1 ─────────── 2    ← перекладина (Frame, Wall)
        │   TRIGGER  │
        0           3    ← низ штанг (без линии 0→3)
```

### Глубина ворот (позже)

Для «за штангами / внутри ворот» по Y — разбить графику на **`GoalNet`** + **`Frame`**; в MVP достаточно Edge-рамы + Y-sort.

---

## Гол соперника (`GoalPlayer`)

- У игрока **нет** рамки ворот — только зона
- **Empty** + `BoxCollider2D`, широкая полоска **внизу**, за вратарём
- Layer **`GoalPlayer`**, Trigger **ON**
- **Не** отскакивает — только `GoalScoredEvent` в коде

---

## Вратарь (`PlayerGoalkeeper`)

| Компонент | Настройка |
|-----------|-----------|
| `BoxCollider2D` | по телу, Trigger OFF |
| Layer | `Keeper` |
| Tag | `Player` |
| `GoalkeeperView` | см. [[Связь сцены с кодом]] |

В **`KickoffWait`**: короткая амплитуда **A/D** (`kickoffMinX` / `kickoffMaxX`) — вратарь **не** тащит за собой мяч.

В **`Simulating`**: полное движение (позже + dive).

---

## Старт мяча (`BallKickoffAnchor`)

> [!important] Мяч **не** привязан к `GoalkeeperView`
> Позиция сброса и подачи — **фиксированный якорь** на сцене.

| Что | Зачем |
|-----|--------|
| `BallKickoffAnchor` | центр **X**, Y у ног; `IBallAnchor` |
| `DirectionArrow` (дочерний) | стрелка куда полетит мяч; направление = `transform.up` |
| `BallView` | ссылка на **`BallKickoffAnchor`**, не на вратаря |

После гола / в `KickoffWait`: мяч на **`BallKickoffAnchor.WorldPosition`**.

**Пробел:** `GoalkeeperView` → `BallView.TryServe(kickoffAnchor.ServeDirection)`.

### Ведение (post-MVP)

`IBallAnchor` + `BallMotion.AttachToAnchor` / `ReleaseFromAnchor` — отдельный якорь на вратаре или враге (`DribblePoint`), мяч следует за точкой, пока держат.

---

## Мяч (`Ball`)

| | |
|---|---|
| `BallView` | тикает `BallMotion` в `Simulating` |
| `BallSettings.radius` | хитбокс для CircleCast |
| **Нет** `Rigidbody2D` | вариант A, см. [[Мяч и коллайдеры]] |
| **Нет** `CircleCollider2D` | радиус только в коде |

---

## Камера

- Orthographic, кадр на всю игровую зону
- Game view: **9:16** (портрет)
- Одна камера на Game; активная сцена — `Game` после загрузки

---

## Компоненты кода на сцене

| Объект | Компонент |
|--------|-----------|
| `BallKickoffAnchor` | `BallKickoffAnchor` |
| `Ball` | `BallView` → ссылка на kickoff anchor |
| `PlayerGoalkeeper` | `GoalkeeperView` → `Ball`, `BallKickoffAnchor` |

---

## Чеклист «поле готово»

- [ ] 3 стены, Layer `Wall`
- [ ] `GoalEnemy`: Visual + Frame (Edge) + ScoringZone (trigger)
- [ ] `GoalPlayer` внизу (trigger)
- [ ] `BallKickoffAnchor` + стрелка
- [ ] `BallView`, `GoalkeeperView`
- [ ] Sorting: `Field` / `Gameplay`
- [ ] `Defenders`: `GoalAnchor`, `DefenderSlots` (сетка), `Spawned`, `DefenderSpawner` — см. [[Генерация врагов]], [[Враги и защитники#Сборка сцены (чеклист)]]
- [ ] Временные ручные `Defender_*` на сцене **удалены**
- [ ] Сохранена сцена

---

## Связанные заметки

- [[Движение мяча]] — CircleCast, подача, голы
- [[Мяч и коллайдеры]] — почему на мяче нет RB
- [[Машины состояний#Уровень 3]] — `KickoffWait` / `Simulating`
- [[../GDD/03 Физика и управление вратарём|GDD §3]]
