---
tags:
  - audio
  - architecture
  - manager
aliases:
  - Звук
  - Audio
  - Аудио система
---

# Аудио система

← [[Home|Главная]] | [[Архитектура/Индекс архитектуры|Архитектура]] | [[Архитектура/DI и LifetimeScope|DI]]

> [!date] Обновлено
> **05.07.2026** — Рефакторинг: шина событий, DI, AudioCatalog SO, без синглтона.

---

## Краткая выжимка

Геймплей **не вызывает звук напрямую**. Сервис слушает `IGameEventBus` и воспроизводит клипы из `AudioCatalog` (ScriptableObject).

```
BallMotion → bus.Publish(BallContactEvent)
                    ↓
              AudioService (App scope)
                    ↓
              AudioCatalog → SoundDefinition
                    ↓
              AudioPlaybackHost (Root scene)
                    ↓
              MusicChannel / SfxPool → Mixer
```

---

## Слои

| Слой | Класс | Где | Задача |
|------|-------|-----|--------|
| CMS | `AudioCatalog` + `SoundDefinition` | `Resources/Data/Settings/` | Клипы, микшер, приоритет, fade |
| Логика | `AudioService` | App scope (DI) | Подписки на шину, cooldown, voice limit |
| Unity | `AudioPlaybackHost` | Root.unity | Пул `AudioSource`, музыкальный канал |

**Не два менеджера** (меню / игра) — один `AudioService`, контекст через `NavigationChangedEvent` и группы микшера.

---

## Каналы воспроизведения

| Канал | AudioSource | Для чего |
|-------|-------------|----------|
| `Music` | 1 источник | Loop, fade, pause/resume |
| `GameplaySfx` | Пул (8) | Удары, голы, свистки |
| `UiSfx` | Пул (3) | UI (позже) |

---

## Маппинг событий → звуки

**Полный справочник для звукаря:** [[Каталог событий и звуков]].

Кратко — всё, на что подписан `AudioService`:

| Событие | Условие | Sound ID |
|---------|---------|----------|
| `BallContactEvent` | любой контакт | `BallHit` |
| `GoalScoredEvent` | `IsPlayerGoal` | `GoalScored` / `GoalConceded` |
| `MatchStartedEvent` | — | `MatchStart` + `MusicMatch` |
| `MatchEndedEvent` | — | `MatchEnd` + stop music |
| `PitchResetRequestedEvent` | — | stop `MusicMatch` |
| `MatchTimeAdjustedEvent` | Δt &gt; 0 / &lt; 0 | `TimeBonus` / `TimePenalty` |
| `DefenderHitEvent` | — | `DefenderHit` |
| `DefenderDestroyedEvent` | — | `DefenderDestroyed` |
| `DefenderPromotionStartedEvent` | — | `PromotionStarted` |
| `DefenderPromotionCompletedEvent` | — | `PromotionCompleted` |
| `DefenderReturnedHomeEvent` | — | `DefenderReturned` |
| `DefenderRoleChangedEvent` | `IsGoalkeeper` | `DefenderRoleChanged` |
| `PerkPickedEvent` | — | `PerkPick` |
| `RunProgressionUpdatedEvent` | уровень вырос | `LevelUp` |
| `PitchPhaseChangedEvent` | `Reshuffle` / `BonusPick` | `ReshuffleStart` / `BonusPickOpen` |
| `NavigationChangedEvent` | OnField → MainMenu | pause music |
| `NavigationChangedEvent` | `ResumingPausedMatch` | resume music |
| `NavigationChangedEvent` | вход в MainMenu / Pause / Tournament | `UiMenuOpen` / `UiPauseOpen` / `UiTournamentOpen` |

Константы id: `AudioCatalog.Ids` в `AudioCatalog.cs`.

---

## Файлы

| Файл | Путь |
|------|------|
| `AudioCatalog.cs` | `Futboloid.Core/Audio/` |
| `SoundDefinition.cs` | `Futboloid.Core/Audio/` |
| `AudioService.cs` | `Futboloid.Core/Audio/` |
| `IAudioPlayback.cs` | `Futboloid.Core/Audio/` |
| `AudioPlaybackHost.cs` | `Futboloid.Main/Audio/` |
| `MatchStartedEvent.cs` | `Futboloid.Core/Bus/Events/` |

---

## DI

```csharp
// RootScopeExtensions
builder.RegisterComponentInHierarchy<AudioPlaybackHost>().As<IAudioPlayback>();

// AppScopeExtensions
builder.RegisterInstance(AudioCatalog.Load());
builder.Register<AudioService>(Lifetime.Singleton);
```

`AudioService` резолвится при старте App scope и подписывается на шину. При `AppGameState.Exit` — `Dispose`, отписка, `StopAll`.

---

## Настройка в Unity

Подробно: [[Инструкция по настройке|Инструкция по настройке]].

1. **Root.unity** — объект `Audio` + компонент `AudioPlaybackHost`
2. **AudioCatalog** asset — `Resources/Data/Settings/AudioCatalog`
3. **Game.unity** — удалить старый `AudioManager` и `Speaker_*`

---

## Связанные заметки

- [[Каталог событий и звуков]]
- [[Инструкция по настройке]]
- [[Система приоритетов и наложения]]
- [[Архитектура/Шина событий|Шина событий]]
- [[Архитектура/DI и LifetimeScope|DI и LifetimeScope]]
- [[Контекст/Контекст чата 05.07.2026 аудио рефакторинг|Контекст рефакторинга]]
