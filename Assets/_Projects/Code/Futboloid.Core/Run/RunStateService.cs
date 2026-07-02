using System;
using System.Collections.Generic;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using UnityEngine;

namespace Futboloid.Core.Run
{
    public class RunStateService : IRunProgressionService, IDisposable
    {
        private const string GoalkeeperSpeedPerkId = "gk_speed";

        private readonly IGameEventBus _bus;
        private readonly RunProgressionSettings _settings;
        private readonly PerkCatalog _catalog;
        private readonly Dictionary<string, int> _perkLevels = new();
        private readonly List<PerkDefinition> _rollBuffer = new();
        private readonly System.Random _random = new();

        private readonly IDisposable _hitSubscription;
        private readonly IDisposable _killSubscription;
        private readonly IDisposable _ballServedSubscription;
        private readonly IDisposable _matchEndedSubscription;
        private readonly IDisposable _pitchResetSubscription;

        private bool _collectingXp;
        private int _pendingPerkPicks;
        private bool _bonusPickActive;
        private PerkDefinition[] _currentOffers = new PerkDefinition[3];

        public int CurrentXp { get; private set; }
        public int RunLevel { get; private set; } = 1;
        public int XpToNextLevel { get; private set; }
        public float XpFill01 { get; private set; }
        public int PendingPerkPicks => _pendingPerkPicks;
        public bool IsBonusPickActive => _bonusPickActive;

        public RunStateService(
            IGameEventBus bus,
            RunProgressionSettings settings,
            PerkCatalog catalog)
        {
            _bus = bus;
            _settings = settings;
            _catalog = catalog;

            RecalculateXpThreshold();

            _hitSubscription = bus.Subscribe<DefenderHitEvent>(_ => TryAddXp(_settings.XpPerHit));
            _killSubscription = bus.Subscribe<DefenderDestroyedEvent>(_ => TryAddXp(_settings.XpPerKill));
            _ballServedSubscription = bus.Subscribe<BallServedEvent>(_ => _collectingXp = true);
            _matchEndedSubscription = bus.Subscribe<MatchEndedEvent>(_ => _collectingXp = false);
            _pitchResetSubscription = bus.Subscribe<PitchResetRequestedEvent>(_ => Reset());

            PublishXpChanged();
        }

        public int GetPerkLevel(string perkId) =>
            _perkLevels.TryGetValue(perkId, out var level) ? level : 0;

        public float GetGoalkeeperMoveSpeedMultiplier()
        {
            var level = GetPerkLevel(GoalkeeperSpeedPerkId);
            if (level <= 0)
                return 1f;

            var def = FindPerk(GoalkeeperSpeedPerkId);
            var perLevel = def != null ? def.ValuePerLevel : 0.15f;
            return 1f + perLevel * level;
        }

        public void Reset()
        {
            CurrentXp = 0;
            RunLevel = 1;
            _perkLevels.Clear();
            _pendingPerkPicks = 0;
            _bonusPickActive = false;
            _collectingXp = false;
            _currentOffers = new PerkDefinition[3];
            RecalculateXpThreshold();
            PublishXpChanged();
        }

        public void ApplyPerkPick(string perkId)
        {
            if (!_bonusPickActive || string.IsNullOrEmpty(perkId))
                return;

            if (!IsOffered(perkId))
            {
                Debug.LogWarning($"[RunStateService] Perk '{perkId}' is not in current offer.");
                return;
            }

            var def = FindPerk(perkId);
            if (def == null)
                return;

            var newLevel = GetPerkLevel(perkId) + 1;
            _perkLevels[perkId] = newLevel;

            _pendingPerkPicks = Mathf.Max(0, _pendingPerkPicks - 1);
            _bonusPickActive = false;
            _currentOffers = new PerkDefinition[3];

            _bus.Publish(new PerkPickedEvent(perkId, newLevel, def.CardColor));
            Debug.Log($"[RunStateService] Picked {perkId} → level {newLevel} ({def.CardColor}).");

            if (_pendingPerkPicks > 0)
                BeginBonusPick();
        }

        public void Dispose()
        {
            _hitSubscription?.Dispose();
            _killSubscription?.Dispose();
            _ballServedSubscription?.Dispose();
            _matchEndedSubscription?.Dispose();
            _pitchResetSubscription?.Dispose();
        }

        private void TryAddXp(int amount)
        {
            if (!_collectingXp || amount <= 0 || _bonusPickActive)
                return;

            CurrentXp += amount;
            ProcessLevelUps();
            PublishXpChanged();
        }

        private void ProcessLevelUps()
        {
            while (CurrentXp >= XpToNextLevel)
            {
                CurrentXp -= XpToNextLevel;
                RunLevel++;
                _pendingPerkPicks++;
                RecalculateXpThreshold();
            }

            if (_pendingPerkPicks > 0 && !_bonusPickActive)
                BeginBonusPick();
        }

        private void BeginBonusPick()
        {
            if (!RollOffers(out var offer0, out var offer1, out var offer2))
            {
                Debug.LogWarning("[RunStateService] No perks available for BonusPick.");
                _pendingPerkPicks = 0;
                return;
            }

            _bonusPickActive = true;
            _currentOffers = new[] { offer0, offer1, offer2 };

            var level0 = offer0 != null ? GetPerkLevel(offer0.Id) + 1 : 0;
            var level1 = offer1 != null ? GetPerkLevel(offer1.Id) + 1 : 0;
            var level2 = offer2 != null ? GetPerkLevel(offer2.Id) + 1 : 0;

            _bus.Publish(new BonusPickOfferedEvent(
                offer0, offer1, offer2, level0, level1, level2));
        }

        private bool RollOffers(
            out PerkDefinition offer0,
            out PerkDefinition offer1,
            out PerkDefinition offer2)
        {
            offer0 = null;
            offer1 = null;
            offer2 = null;

            _rollBuffer.Clear();
            foreach (var perk in _catalog.Perks)
            {
                if (perk == null || string.IsNullOrEmpty(perk.Id))
                    continue;

                if (GetPerkLevel(perk.Id) >= perk.MaxLevel)
                    continue;

                _rollBuffer.Add(perk);
            }

            if (_rollBuffer.Count == 0)
                return false;

            Shuffle(_rollBuffer);

            var count = Mathf.Min(_settings.OfferCount, _rollBuffer.Count);
            if (count > 0) offer0 = _rollBuffer[0];
            if (count > 1) offer1 = _rollBuffer[1];
            if (count > 2) offer2 = _rollBuffer[2];
            return offer0 != null;
        }

        private void Shuffle<T>(IList<T> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = _random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private bool IsOffered(string perkId)
        {
            foreach (var offer in _currentOffers)
            {
                if (offer != null && offer.Id == perkId)
                    return true;
            }

            return false;
        }

        private PerkDefinition FindPerk(string perkId)
        {
            foreach (var perk in _catalog.Perks)
            {
                if (perk != null && perk.Id == perkId)
                    return perk;
            }

            return null;
        }

        private void RecalculateXpThreshold()
        {
            XpToNextLevel = _settings.XpRequiredForLevel(RunLevel);
            XpFill01 = XpToNextLevel > 0 ? (float)CurrentXp / XpToNextLevel : 0f;
        }

        private void PublishXpChanged()
        {
            XpFill01 = XpToNextLevel > 0 ? (float)CurrentXp / XpToNextLevel : 0f;
            _bus.Publish(new RunXpChangedEvent(CurrentXp, XpToNextLevel, XpFill01));
        }
    }
}
