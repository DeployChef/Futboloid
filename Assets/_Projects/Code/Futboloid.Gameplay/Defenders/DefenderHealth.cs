using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using TMPro;
using UnityEngine;
using VContainer;

namespace Futboloid.Gameplay.Defenders
{
    public sealed class DefenderHealth : MonoBehaviour
    {
        [SerializeField] private int maxHp = 3;
        [SerializeField] private TextMeshProUGUI hpLabel;
        [SerializeField] private DefenderView defender;
        [SerializeField] private DefenderDeathVfx deathVfx;

        private IGameEventBus _bus;

        public bool IsAlive { get; private set; } = true;
        public int CurrentHp { get; private set; }
        public int MaxHp => maxHp;

        [Inject]
        public void Construct(IGameEventBus bus) => _bus = bus;

        public void Configure(int maxHpValue)
        {
            maxHp = maxHpValue;
            Reset();
        }

        public void Reset()
        {
            IsAlive = true;
            CurrentHp = maxHp;
            RefreshLabel();

            if (hpLabel != null)
                hpLabel.gameObject.SetActive(true);
        }

        public void Heal(int amount)
        {
            if (amount <= 0)
                return;

            CurrentHp = Mathf.Min(maxHp, CurrentHp + amount);
            RefreshLabel();
        }

        public void ApplyDamage(int amount, int slotId, Vector3 worldPosition)
        {
            if (!IsAlive)
                return;

            CurrentHp = Mathf.Max(0, CurrentHp - amount);
            RefreshLabel();
            _bus.Publish(new DefenderDamagedEvent(slotId, CurrentHp, worldPosition));

            if (CurrentHp <= 0)
                Die(slotId);
        }

        private void Die(int slotId)
        {
            if (!IsAlive)
                return;

            IsAlive = false;

            if (hpLabel != null)
                hpLabel.gameObject.SetActive(false);

            var wasGoalkeeper = defender.Role == DefenderRole.Goalkeeper;
            _bus.Publish(new DefenderDestroyedEvent(slotId, wasGoalkeeper));
            deathVfx?.Play();
            defender.NotifyDestroyed(wasGoalkeeper);
        }

        private void RefreshLabel()
        {
            if (hpLabel == null)
                return;

            hpLabel.text = CurrentHp.ToString();
        }
    }
}
