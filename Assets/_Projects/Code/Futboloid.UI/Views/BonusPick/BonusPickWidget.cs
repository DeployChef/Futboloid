using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Core.Audio;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Core.Run;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Futboloid.UI.Views.BonusPick
{
    /// <summary>
    /// Overlay выбора перка (1 из 3) на сцене Game. Показывается в фазе BonusPick.
    /// A/D — выбор карточки, Space — подтвердить. Клик мышью тоже выбирает сразу.
    /// </summary>
    public class BonusPickWidget : MonoBehaviour
    {
        [SerializeField] private PerkCardView[] cards = new PerkCardView[3];

        private readonly List<IDisposable> _subscriptions = new();
        private readonly List<int> _offeredCardIndices = new();

        private IRunProgressionService _run;
        private IAudioManager _audio;
        private int _selectedIndex = -1;

        private void Awake()
        {
            gameObject.SetActive(false);

            for (var i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null)
                    continue;

                var index = i;
                cards[i].Clicked += () => OnCardClicked(index);
                cards[i].PointerEntered += () => OnCardHovered(index);
            }
        }

        [Inject]
        public void Construct(IGameEventBus bus, IRunProgressionService run, IAudioManager audio)
        {
            _run = run;
            _audio = audio;

            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            _subscriptions.Clear();

            _subscriptions.Add(bus.Subscribe<PitchPhaseChangedEvent>(OnPitchPhaseChanged));
            _subscriptions.Add(bus.Subscribe<BonusPickOfferedEvent>(OnBonusPickOffered));
        }

        private void Update()
        {
            if (!gameObject.activeSelf || _run == null || !_run.IsBonusPickActive)
                return;

            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.aKey.wasPressedThisFrame)
                MoveSelection(-1);

            if (keyboard.dKey.wasPressedThisFrame)
                MoveSelection(1);

            if (keyboard.spaceKey.wasPressedThisFrame)
                ConfirmSelection();
        }

        private void OnDestroy()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }

        private void OnPitchPhaseChanged(PitchPhaseChangedEvent e)
        {
            var active = e.Phase == PitchPhase.BonusPick;
            gameObject.SetActive(active);

            if (!active)
                ClearSelection();
        }

        private void OnBonusPickOffered(BonusPickOfferedEvent e)
        {
            BindCard(cards.Length > 0 ? cards[0] : null, e.Offer0, e.LevelAfterPick0);
            BindCard(cards.Length > 1 ? cards[1] : null, e.Offer1, e.LevelAfterPick1);
            BindCard(cards.Length > 2 ? cards[2] : null, e.Offer2, e.LevelAfterPick2);

            RebuildOfferedIndices();
            SelectCard(GetDefaultCardIndex(), playFocusSound: false);
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

        private void RebuildOfferedIndices()
        {
            _offeredCardIndices.Clear();

            for (var i = 0; i < cards.Length; i++)
            {
                if (cards[i] != null && cards[i].IsOffered)
                    _offeredCardIndices.Add(i);
            }
        }

        private int GetDefaultCardIndex()
        {
            if (_offeredCardIndices.Count == 0)
                return -1;

            return _offeredCardIndices[_offeredCardIndices.Count / 2];
        }

        private void MoveSelection(int direction)
        {
            if (_offeredCardIndices.Count == 0 || direction == 0)
                return;

            var currentOfferIndex = _offeredCardIndices.IndexOf(_selectedIndex);
            if (currentOfferIndex < 0)
                currentOfferIndex = 0;

            var nextOfferIndex = (currentOfferIndex + direction) % _offeredCardIndices.Count;
            if (nextOfferIndex < 0)
                nextOfferIndex += _offeredCardIndices.Count;

            SelectCard(_offeredCardIndices[nextOfferIndex]);
        }

        private void SelectCard(int index, bool playFocusSound = true)
        {
            if (index == _selectedIndex)
                return;

            _selectedIndex = index;

            for (var i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null)
                    continue;

                cards[i].SetSelected(i == _selectedIndex);
            }

            if (playFocusSound)
                _audio?.Play(AudioCatalog.Ids.BonusPickCardFocus);
        }

        private void ClearSelection()
        {
            _selectedIndex = -1;
            _offeredCardIndices.Clear();

            for (var i = 0; i < cards.Length; i++)
                cards[i]?.SetSelected(false);
        }

        private void ConfirmSelection()
        {
            if (_selectedIndex < 0)
                return;

            TryPickCard(_selectedIndex);
        }

        private void OnCardClicked(int index) => TryPickCard(index);

        private void OnCardHovered(int index)
        {
            if (!gameObject.activeSelf || _run == null || !_run.IsBonusPickActive)
                return;

            if (index < 0 || index >= cards.Length || cards[index] == null || !cards[index].IsOffered)
                return;

            SelectCard(index);
        }

        private void TryPickCard(int index)
        {
            if (_run == null || !_run.IsBonusPickActive)
                return;

            if (index < 0 || index >= cards.Length || cards[index] == null)
                return;

            if (!cards[index].IsOffered)
                return;

            var perkId = cards[index].PerkId;
            if (string.IsNullOrEmpty(perkId))
                return;

            _run.ApplyPerkPick(perkId);
        }
    }
}
