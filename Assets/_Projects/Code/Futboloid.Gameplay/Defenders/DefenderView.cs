using System.Collections.Generic;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Gameplay.Ball;
using TMPro;
using UnityEngine;
using VContainer;

namespace Futboloid.Gameplay.Defenders
{
    public class DefenderView : MonoBehaviour, IDefenderBallContact
    {
        [SerializeField] private int slotId;
        [SerializeField] private DefenderRole role = DefenderRole.Field;
        [SerializeField] private int maxHp = 3;
        [SerializeField] private DefenderHitBehavior hitBehavior;
        [SerializeField] private Collider2D bodyCollider;
        [SerializeField] private TextMeshProUGUI hpLabel;

        private IGameEventBus _bus;
        private DefenderGridRegistry _registry;
        private int _hp;
        private bool _isAlive = true;
        private Vector2 _homePosition;

        public int SlotId => slotId;
        public DefenderRole Role => role;
        public bool IsAlive => _isAlive;
        public Vector2 HomePosition => _homePosition;

        private void Awake()
        {
            if (bodyCollider == null)
                bodyCollider = GetComponent<Collider2D>();

            _homePosition = transform.position;
            _hp = maxHp;
            RefreshHpLabel();
        }

        [Inject]
        public void Construct(IGameEventBus bus)
        {
            _bus = bus;
        }

        private void Start()
        {
            _registry = FindAnyObjectByType<DefenderGridRegistry>();
            _registry?.Register(this);
        }

        private void OnDestroy()
        {
            _registry?.Unregister(this);
        }

        public bool TryHandleBallHit(BallMotion motion, RaycastHit2D hit, HashSet<int> hitsThisFrame)
        {
            if (!_isAlive || motion == null)
                return false;

            ResolveBallResponse(motion, hit);

            if (!hitsThisFrame.Contains(slotId))
            {
                hitsThisFrame.Add(slotId);
                ApplyDamage(1, isIncomingPass: false);
            }

            _bus?.Publish(new DefenderHitEvent(slotId));
            return true;
        }

        private void ResolveBallResponse(BallMotion motion, RaycastHit2D hit)
        {
            var type = hitBehavior != null ? hitBehavior.HitType : DefenderHitType.Reflect;

            // MVP-1: только Reflect. Остальные типы — в следующих итерациях.
            if (type != DefenderHitType.Reflect)
                Debug.LogWarning($"[DefenderView] Hit type {type} not implemented yet on slot {slotId}; using Reflect.");

            motion.ReflectFromHit(hit);
        }

        private void ApplyDamage(int amount, bool isIncomingPass)
        {
            if (isIncomingPass || !_isAlive)
                return;

            _hp = Mathf.Max(0, _hp - amount);
            RefreshHpLabel();
            _bus?.Publish(new DefenderDamagedEvent(slotId, _hp, transform.position));

            if (_hp <= 0)
                Die();
        }

        private void Die()
        {
            if (!_isAlive)
                return;

            _isAlive = false;

            if (bodyCollider != null)
                bodyCollider.enabled = false;

            if (hpLabel != null)
                hpLabel.gameObject.SetActive(false);

            _bus?.Publish(new DefenderDestroyedEvent(slotId));
            Debug.Log($"[DefenderView] Slot {slotId} destroyed.");
        }

        private void RefreshHpLabel()
        {
            if (hpLabel == null)
                return;

            hpLabel.text = _hp.ToString();
        }
    }
}
