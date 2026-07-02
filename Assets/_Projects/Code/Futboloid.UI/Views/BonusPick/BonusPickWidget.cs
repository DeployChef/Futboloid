using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Core.Run;
using UnityEngine;
using VContainer;

namespace Futboloid.UI.Views.BonusPick
{
    /// <summary>
    /// Overlay выбора перка (1 из 3) на сцене Game. Показывается в фазе BonusPick.
    /// </summary>
    public class BonusPickWidget : MonoBehaviour
    {
        [SerializeField] private PerkCardView[] cards = new PerkCardView[3];

        private readonly List<IDisposable> _subscriptions = new();

        private IRunProgressionService _run;

        private void Awake()
        {
            gameObject.SetActive(false);

            for (var i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null)
                    continue;

                var index = i;
                cards[i].Clicked += () => OnCardClicked(index);
            }
        }

        [Inject]
        public void Construct(IGameEventBus bus, IRunProgressionService run)
        {
            _run = run;

            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            _subscriptions.Clear();

            _subscriptions.Add(bus.Subscribe<PitchPhaseChangedEvent>(OnPitchPhaseChanged));
            _subscriptions.Add(bus.Subscribe<BonusPickOfferedEvent>(OnBonusPickOffered));
        }

        private void OnDestroy()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }

        private void OnPitchPhaseChanged(PitchPhaseChangedEvent e) =>
            gameObject.SetActive(e.Phase == PitchPhase.BonusPick);

        private void OnBonusPickOffered(BonusPickOfferedEvent e)
        {
            BindCard(cards.Length > 0 ? cards[0] : null, e.Offer0, e.LevelAfterPick0);
            BindCard(cards.Length > 1 ? cards[1] : null, e.Offer1, e.LevelAfterPick1);
            BindCard(cards.Length > 2 ? cards[2] : null, e.Offer2, e.LevelAfterPick2);
        }

        private void BindCard(PerkCardView card, PerkDefinition perk, int levelAfterPick)
        {
            if (card == null)
                return;

            if (perk == null)
            {
                card.Hide();
                return;
            }

            card.Show(perk, levelAfterPick);
        }

        private void OnCardClicked(int index)
        {
            if (_run == null || !_run.IsBonusPickActive)
                return;

            if (index < 0 || index >= cards.Length || cards[index] == null)
                return;

            var perkId = cards[index].PerkId;
            if (string.IsNullOrEmpty(perkId))
                return;

            _run.ApplyPerkPick(perkId);
        }
    }
}
