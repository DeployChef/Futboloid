using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Gameplay.Ball;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace Futboloid.Gameplay.Defenders
{
    public class DefenderView : MonoBehaviour, IDefenderBallContact
    {
        [SerializeField] private int slotId;
        [SerializeField] private DefenderRole role = DefenderRole.Field;
        [SerializeField] private int maxHp = 3;
        [SerializeField] private DefenderHitBehavior hitBehavior;
        [SerializeField] private DefenderMovementBehavior movementBehavior;
        [SerializeField] private Collider2D bodyCollider;
        [SerializeField] private TextMeshProUGUI hpLabel;

        [Header("Goalkeeper")]
        [SerializeField] private Transform goalAnchor;
        [SerializeField] private BallView ball;
        [SerializeField] private DefenderGoalkeeperBehavior goalkeeperBehavior;

        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private float gizmoLabelHeight = 0.35f;
        [SerializeField] private float gizmoLabelPadding = 0.12f;
        [SerializeField] private float gizmoGoalHalfWidth = 2f;
        [FormerlySerializedAs("gizmoHyperbolaA")]
        [SerializeField] private float gizmoParabolaHeight = 0.35f;

        private IGameEventBus _bus;
        private DefenderGridRegistry _registry;
        private readonly DefenderMotor _motor = new();
        private readonly List<IDisposable> _subscriptions = new();
        private int _hp;
        private bool _isAlive = true;
        private bool _onField;
        private bool _simulating;
        private bool _warnedMissingGoalAnchor;
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
            _subscriptions.Add(bus.Subscribe<PitchPhaseChangedEvent>(OnPitchPhaseChanged));
            _subscriptions.Add(bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));

            if (ball == null)
                ball = FindAnyObjectByType<BallView>();
        }

        private void Update()
        {
            if (!_onField || !_simulating || !_isAlive || role != DefenderRole.Goalkeeper)
                return;

            var zone = ResolveGoalAnchor();
            if (zone == null)
                return;

            var trackSpeed = goalkeeperBehavior != null ? goalkeeperBehavior.TrackSpeed : 2.5f;
            var ballX = ResolveBallWorldX();
            var position = _motor.TickGoalkeeperOnParabola(zone, ballX, trackSpeed, Time.deltaTime);
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }

        private void Start()
        {
            _registry = FindAnyObjectByType<DefenderGridRegistry>();
            _registry?.Register(this);
        }

        private void OnDestroy()
        {
            _registry?.Unregister(this);

            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }

        private void OnPitchPhaseChanged(PitchPhaseChangedEvent e)
        {
            _simulating = e.Phase == PitchPhase.Simulating;

            if (role != DefenderRole.Goalkeeper)
                return;

            if (e.Phase == PitchPhase.KickoffWait)
                SnapGoalkeeperToParabolaStart();
            else if (e.Phase == PitchPhase.Simulating)
                SnapGoalkeeperToBall();
        }

        private void OnNavigationChanged(NavigationChangedEvent e)
        {
            _onField = e.Current == NavigationState.OnField;

            if (_onField && role == DefenderRole.Goalkeeper)
                SnapGoalkeeperToParabolaStart();
        }

        private void SnapGoalkeeperToParabolaStart()
        {
            SnapGoalkeeperToBall();
        }

        private void SnapGoalkeeperToBall()
        {
            var zone = ResolveGoalAnchor();
            if (zone == null)
                return;

            var t = zone.ParamFromWorldX(ResolveBallWorldX());
            _motor.ResetGoalkeeperParam(t);
            var position = zone.PositionOnParabola(t);
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }

        private float ResolveBallWorldX()
        {
            return ball != null ? ball.Position.x : transform.position.x;
        }

        private GoalAnchor ResolveGoalAnchor()
        {
            if (goalAnchor == null)
                return null;

            var zone = goalAnchor.GetComponent<GoalAnchor>();
            if (zone == null && !_warnedMissingGoalAnchor)
            {
                _warnedMissingGoalAnchor = true;
                Debug.LogWarning(
                    $"[DefenderView] Slot {slotId}: goalAnchor '{goalAnchor.name}' has no GoalAnchor component — GK cannot move.",
                    goalAnchor);
            }

            return zone;
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

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos)
                return;

            DrawGizmos(selected: true);
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos)
                return;

            DrawGizmos(selected: false);
        }

        private void DrawGizmos(bool selected)
        {
            var home = Application.isPlaying ? _homePosition : (Vector2)transform.position;
            var center = new Vector3(home.x, home.y, transform.position.z);
            var labelPos = GetGizmoLabelPosition(center);

            var hitType = hitBehavior != null ? hitBehavior.HitType : DefenderHitType.Reflect;
            var moveType = movementBehavior != null
                ? movementBehavior.MovementType
                : DefenderMovementType.Idle;

            var label = $"#{slotId}  {role}\nHit: {hitType}";
            if (role == DefenderRole.Field)
                label += $"\nMove: {moveType}";
            if (Application.isPlaying)
                label += $"\nHP: {_hp}/{maxHp}";

            DefenderGizmoDrawer.DrawLabel(labelPos, label);

            if (role == DefenderRole.Goalkeeper)
            {
                var zone = ResolveGoalAnchor();
                var gkCenter = zone != null
                    ? new Vector3(zone.Center.x, zone.GoalLineY, transform.position.z)
                    : center;
                var halfWidth = zone != null ? zone.HalfWidth : gizmoGoalHalfWidth;
                var parabolaHeight = zone != null ? zone.ParabolaHeight : gizmoParabolaHeight;

                DefenderGizmoDrawer.DrawGoalkeeperParabola(
                    gkCenter,
                    halfWidth,
                    parabolaHeight,
                    selected ? new Color(1f, 0.4f, 1f, 0.9f) : new Color(1f, 0.4f, 1f, 0.45f),
                    selected);
                return;
            }

            if (movementBehavior == null)
                return;

            var alpha = selected ? 0.85f : 0.4f;
            DefenderGizmoDrawer.DrawWireCircle(
                center,
                movementBehavior.SeparationRadius,
                new Color(1f, 0.55f, 0.1f, alpha * 0.7f));

            switch (movementBehavior.MovementType)
            {
                case DefenderMovementType.PatrolGenerated:
                    DefenderGizmoDrawer.DrawWireCircle(
                        center,
                        movementBehavior.PatrolRadius,
                        new Color(0.3f, 1f, 0.45f, alpha));
                    var path = PatrolPathGenerator.Generate(
                        home,
                        movementBehavior.PatrolPointCount,
                        movementBehavior.PatrolRadius,
                        slotId * 7919 + 17);
                    DefenderGizmoDrawer.DrawPatrolPath(
                        path,
                        new Color(1f, 0.92f, 0.2f, alpha),
                        closed: true);
                    break;

                case DefenderMovementType.WanderInRadius:
                    DefenderGizmoDrawer.DrawWireCircle(
                        center,
                        movementBehavior.WanderRadius,
                        new Color(0.3f, 0.75f, 1f, alpha));
                    break;

                case DefenderMovementType.ChaseBallInRadius:
                    DefenderGizmoDrawer.DrawWireCircle(
                        center,
                        movementBehavior.ChaseRadius,
                        new Color(0.2f, 0.95f, 1f, alpha));
                    break;
            }
        }

        private Vector3 GetGizmoLabelPosition(Vector3 center)
        {
            var collider = bodyCollider != null ? bodyCollider : GetComponent<Collider2D>();
            if (collider != null)
                return new Vector3(center.x, collider.bounds.max.y + gizmoLabelPadding, center.z);

            return center + Vector3.up * gizmoLabelHeight;
        }
    }
}
