using System;
using System.Collections.Generic;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Gameplay.Ball;
using Futboloid.Gameplay.Match;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace Futboloid.Gameplay.Defenders
{
    public class DefenderView : MonoBehaviour
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
        private bool _runningToGoal;
        private bool _returningHome;
        private float _runSpeed = 4f;
        private float _runAcceleration = 18f;
        private float _runArriveThreshold = 0.08f;
        private Vector2 _runTarget;
        private Vector2 _homePosition;
        private int _lastDamageFrame = -1;

        public int SlotId => slotId;
        public Collider2D ContactCollider => bodyCollider != null ? bodyCollider : GetComponent<Collider2D>();
        public DefenderRole Role => role;
        public bool IsAlive => _isAlive;
        public bool RunningToGoal => _runningToGoal;
        public bool ReturningHome => _returningHome;
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
        public void Construct(
            IGameEventBus bus,
            PitchStateMachine pitch,
            MatchFlow matchFlow,
            DefenderGridRegistry registry)
        {
            _bus = bus;
            _registry = registry;
            _subscriptions.Add(bus.Subscribe<PitchPhaseChangedEvent>(OnPitchPhaseChanged));
            _subscriptions.Add(bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));

            SyncPitchState(pitch.Current);
            _onField = matchFlow.IsOnField;

            if (ball == null)
                ball = FindAnyObjectByType<BallView>();
        }

        private void SyncPitchState(PitchPhase phase)
        {
            _simulating = phase == PitchPhase.Simulating;
        }

        private void Update()
        {
            if (!_isAlive)
                return;

            if (_returningHome)
            {
                TickReturningHome();
                return;
            }

            if (_runningToGoal)
            {
                TickRunningToGoal();
                return;
            }

            if (!_onField || !_simulating || role != DefenderRole.Goalkeeper)
                return;

            var zone = ResolveGoalAnchor();
            if (zone == null)
                return;

            var trackSpeed = goalkeeperBehavior != null ? goalkeeperBehavior.TrackSpeed : 2.5f;
            var ballX = ResolveBallWorldX();
            var position = _motor.TickGoalkeeperOnParabola(zone, ballX, trackSpeed, Time.deltaTime);
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }

        public void BeginRunToGoal(Transform anchor, float maxSpeed, float acceleration, float arriveThreshold)
        {
            if (!_isAlive || role != DefenderRole.Field)
                return;

            goalAnchor = anchor;
            _runSpeed = maxSpeed;
            _runAcceleration = acceleration;
            _runArriveThreshold = arriveThreshold;
            _runTarget = ResolveRunTarget(anchor);
            _motor.ResetRunVelocity();
            _runningToGoal = true;
        }

        public void SetRole(DefenderRole newRole)
        {
            if (role == newRole)
                return;

            role = newRole;
            _runningToGoal = false;
            _bus?.Publish(new DefenderRoleChangedEvent(slotId, newRole == DefenderRole.Goalkeeper));

            if (newRole == DefenderRole.Goalkeeper)
                SyncGoalkeeperMotorFromPosition();
        }

        private void TickRunningToGoal()
        {
            var current = (Vector2)transform.position;
            var next = _motor.TickRunTowards(
                current,
                _runTarget,
                _runSpeed,
                _runAcceleration,
                _runArriveThreshold,
                Time.deltaTime,
                out var arrived);
            transform.position = new Vector3(next.x, next.y, transform.position.z);

            if (!arrived)
                return;

            CompleteRunToGoal();
        }

        /// <returns>true, если нужно ждать прибытия (событие придёт из <see cref="CompleteReturningHome"/>).</returns>
        public bool BeginReshuffleReturn(float maxSpeed, float acceleration, float arriveThreshold)
        {
            if (!_isAlive)
                return false;

            ApplyReshuffleHeal();

            // Замена вратаря: продолжает бежать к воротам, не уходит на field-слот.
            if (_runningToGoal)
                return false;

            if (_returningHome)
            {
                _runSpeed = maxSpeed;
                _runAcceleration = acceleration;
                _runArriveThreshold = arriveThreshold;
                _runTarget = ResolveReshuffleTarget();

                if (IsNearReshuffleTarget())
                {
                    CompleteReturningHome();
                    return false;
                }

                return true;
            }

            _runSpeed = maxSpeed;
            _runAcceleration = acceleration;
            _runArriveThreshold = arriveThreshold;
            _runTarget = ResolveReshuffleTarget();

            if (IsNearReshuffleTarget())
                return false;

            _motor.ResetRunVelocity();
            _returningHome = true;
            return true;
        }

        private void TickReturningHome()
        {
            var current = (Vector2)transform.position;
            var next = _motor.TickRunTowards(
                current,
                _runTarget,
                _runSpeed,
                _runAcceleration,
                _runArriveThreshold,
                Time.deltaTime,
                out var arrived);
            transform.position = new Vector3(next.x, next.y, transform.position.z);

            if (!arrived)
                return;

            CompleteReturningHome();
        }

        private void CompleteReturningHome()
        {
            _returningHome = false;
            _bus?.Publish(new DefenderReturnedHomeEvent(slotId));
        }

        private Vector2 ResolveReshuffleTarget()
        {
            if (role == DefenderRole.Goalkeeper)
            {
                var zone = ResolveGoalAnchor();
                if (zone != null)
                    return zone.PositionOnParabola(0f);
            }

            return _homePosition;
        }

        private bool IsNearReshuffleTarget()
        {
            var offset = _runTarget - (Vector2)transform.position;
            return offset.sqrMagnitude <= _runArriveThreshold * _runArriveThreshold;
        }

        private void ApplyReshuffleHeal()
        {
            var heal = Mathf.CeilToInt(maxHp * 0.25f);
            if (heal <= 0)
                return;

            _hp = Mathf.Min(maxHp, _hp + heal);
            RefreshHpLabel();
        }

        private void CompleteRunToGoal()
        {
            _runningToGoal = false;
            SetRole(DefenderRole.Goalkeeper);
            _bus?.Publish(new DefenderPromotionCompletedEvent(slotId));
        }

        private static Vector2 ResolveRunTarget(Transform anchor)
        {
            if (anchor == null)
                return Vector2.zero;

            var zone = anchor.GetComponent<GoalAnchor>();
            return zone != null ? zone.PositionOnParabola(0f) : anchor.position;
        }

        private void Start()
        {
            if (_registry == null)
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
            SyncPitchState(e.Phase);

            if (role != DefenderRole.Goalkeeper)
                return;

            if (e.Phase == PitchPhase.KickoffWait)
                SyncGoalkeeperMotorFromPosition();
            else if (e.Phase == PitchPhase.Simulating)
                SnapGoalkeeperToBall();
        }

        private void OnNavigationChanged(NavigationChangedEvent e)
        {
            _onField = e.Current == NavigationState.OnField;

            if (_onField && role == DefenderRole.Goalkeeper)
                SyncGoalkeeperMotorFromPosition();
        }

        private void SyncGoalkeeperMotorFromPosition()
        {
            var zone = ResolveGoalAnchor();
            if (zone == null)
                return;

            _motor.ResetGoalkeeperParam(zone.ParamFromWorldX(transform.position.x));
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

        public void HandleBallContact(BallMotion motion, RaycastHit2D hit)
        {
            DefenderHitResolver.Resolve(motion, hit, this, hitBehavior);

            if (Time.frameCount != _lastDamageFrame)
            {
                _lastDamageFrame = Time.frameCount;
                ApplyDamage(1);
            }

            _bus?.Publish(new DefenderHitEvent(slotId));
        }

        private void ApplyDamage(int amount)
        {
            if (!_isAlive)
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

            var wasGoalkeeper = role == DefenderRole.Goalkeeper;
            _isAlive = false;
            _runningToGoal = false;
            _returningHome = false;

            if (bodyCollider != null)
                bodyCollider.enabled = false;

            if (hpLabel != null)
                hpLabel.gameObject.SetActive(false);

            _bus?.Publish(new DefenderDestroyedEvent(slotId, wasGoalkeeper));
            _registry?.Unregister(this);
            Debug.Log($"[DefenderView] Slot {slotId} destroyed.");
            Destroy(gameObject);
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
            if (_runningToGoal)
                label += "\n→ GK";
            if (_returningHome)
                label += "\n↩ home";
            if (role == DefenderRole.Field && !_runningToGoal && !_returningHome)
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
