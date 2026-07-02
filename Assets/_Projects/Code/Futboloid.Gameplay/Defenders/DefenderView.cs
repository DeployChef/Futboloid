using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
        [SerializeField] private Collider2D bodyCollider;
        [SerializeField] private TextMeshProUGUI hpLabel;

        [Header("Hit")]
        [SerializeField] private DefenderHitType hitType = DefenderHitType.Reflect;
        [SerializeField] private float launchSpeed = 12f;
        [Tooltip("Шанс 0–100 пнуть дальше от вратаря (в противоположный угол). Иначе — ближе к вратарю.")]
        [FormerlySerializedAs("openGoalWeight")]
        [SerializeField] [Range(0, 100)] private int openGoalChancePercent = 70;
        [Tooltip("Минимальный интервал между взаимодействиями с мячом: отскок и урон (сек).")]
        [FormerlySerializedAs("damageCooldown")]
        [SerializeField] private float interactionCooldown = 0.1f;

        [Header("Movement")]
        [SerializeField] private DefenderMovementType movementType = DefenderMovementType.Idle;
        [SerializeField] private int patrolPointCount = 4;
        [SerializeField] private float patrolRadius = 1.5f;
        [SerializeField] private float wanderRadius = 1.5f;
        [SerializeField] private float chaseRadius = 3f;
        [SerializeField] private float separationRadius = 0.6f;
        [SerializeField] private float fieldMoveSpeed = 1.6f;
        [SerializeField] private float fieldAcceleration = 12f;
        [SerializeField] private float fieldArriveThreshold = 0.12f;

        [Header("Goalkeeper")]
        [SerializeField] private Transform goalAnchor;
        [FormerlySerializedAs("paramSpeed")]
        [Tooltip("Скорость слежения по параметру t (−1…1) в секунду — насколько быстро GK едет по дуге к X мяча.")]
        [SerializeField] private float trackSpeed = 1.25f;

        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private float gizmoLabelHeight = 0.35f;
        [SerializeField] private float gizmoLabelPadding = 0.12f;
        [SerializeField] private float gizmoGoalHalfWidth = 2f;
        [FormerlySerializedAs("gizmoHyperbolaA")]
        [SerializeField] private float gizmoParabolaHeight = 0.35f;

        private IGameEventBus _bus;
        private DefenderGridRegistry _registry;
        private DefenderLogic _logic;
        private BallView _ball;
        private PitchBounds _pitchBounds;
        private readonly List<IDisposable> _subscriptions = new();
        private int _hp;
        private bool _isAlive = true;
        private bool _onField;
        private bool _simulating;
        private bool _warnedMissingGoalAnchor;
        private bool _runningToGoal;
        private bool _reshuffleMoving;
        private float _runSpeed = 4f;
        private float _runAcceleration = 18f;
        private float _runArriveThreshold = 0.08f;
        private Vector2 _runTarget;
        private Vector2 _homePosition;
        private float _lastInteractionTime = float.NegativeInfinity;
        private readonly List<Vector2> _neighborPositions = new();

        public int SlotId => slotId;
        public Collider2D ContactCollider => bodyCollider != null ? bodyCollider : GetComponent<Collider2D>();
        public DefenderRole Role => role;
        public DefenderHitType HitType => hitType;
        public float LaunchSpeed => launchSpeed;
        public int OpenGoalChancePercent => openGoalChancePercent;
        public bool IsAlive => _isAlive;
        public bool RunningToGoal => _runningToGoal;
        public Vector2 HomePosition => _homePosition;

        public void ApplySpawnSetup(in DefenderBuild build, Vector2 home, Transform anchor)
        {
            slotId = build.SlotId;
            role = build.Role;
            maxHp = build.MaxHp;
            hitType = build.HitType;
            movementType = build.MovementType;
            patrolPointCount = build.PatrolPointCount;
            patrolRadius = build.PatrolRadius;
            wanderRadius = build.WanderRadius;
            chaseRadius = build.ChaseRadius;
            separationRadius = build.SeparationRadius;
            fieldMoveSpeed = build.FieldMoveSpeed;
            fieldAcceleration = build.FieldAcceleration;
            fieldArriveThreshold = build.FieldArriveThreshold;
            launchSpeed = build.LaunchSpeed;
            openGoalChancePercent = build.OpenGoalChancePercent;
            interactionCooldown = build.InteractionCooldown;

            if (build.TrackSpeed > 0f)
                trackSpeed = build.TrackSpeed;

            goalAnchor = anchor;
            transform.position = new Vector3(home.x, home.y, transform.position.z);
            _homePosition = home;
            _hp = maxHp;
            _isAlive = true;
            _runningToGoal = false;
            RefreshHpLabel();
        }

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
            DefenderGridRegistry registry,
            DefenderLogic logic,
            BallView ball,
            PitchBounds pitchBounds)
        {
            _bus = bus;
            _registry = registry;
            _logic = logic;
            _ball = ball;
            _pitchBounds = pitchBounds;
            _subscriptions.Add(bus.Subscribe<PitchPhaseChangedEvent>(OnPitchPhaseChanged));
            _subscriptions.Add(bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));

            SyncPitchState(pitch.Current);
            _onField = matchFlow.IsOnField;
            _logic.InitializeFieldMovement(_homePosition, slotId, patrolPointCount, patrolRadius);
        }

        private void SyncPitchState(PitchPhase phase)
        {
            _simulating = phase == PitchPhase.Simulating;
        }

        private Tween _reshuffleTween;

        private void Update()
        {
            if (!_isAlive || _reshuffleMoving)
                return;

            if (_runningToGoal)
            {
                TickRunningToGoal();
                return;
            }

            if (!_onField || !_simulating)
                return;

            if (role == DefenderRole.Goalkeeper)
            {
                TickGoalkeeper();
                return;
            }

            if (role == DefenderRole.Field)
                TickFieldMovement();
        }

        private void TickGoalkeeper()
        {
            var zone = ResolveGoalAnchor();
            if (zone == null)
                return;

            var ballX = ResolveBallWorldX();
            var position = _logic.TickGoalkeeperOnParabola(zone, ballX, trackSpeed, Time.deltaTime);
            if (_pitchBounds != null)
                position = _pitchBounds.Clamp(position);
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }

        private void TickFieldMovement()
        {
            CollectNeighborPositions();

            Vector2? ballPosition = _ball != null ? _ball.Position : null;
            var current = (Vector2)transform.position;
            var next = _logic.TickFieldMovement(
                current,
                movementType,
                _homePosition,
                wanderRadius,
                chaseRadius,
                ballPosition,
                fieldMoveSpeed,
                fieldAcceleration,
                fieldArriveThreshold,
                separationRadius,
                _neighborPositions,
                Time.deltaTime);
            transform.position = new Vector3(next.x, next.y, transform.position.z);
        }

        private void CollectNeighborPositions()
        {
            _neighborPositions.Clear();

            if (_registry == null)
                return;

            _registry.ForEachLiving(defender =>
            {
                if (defender == null || defender == this)
                    return;

                _neighborPositions.Add(defender.transform.position);
            });
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
            _logic.ResetRunVelocity();
            _logic.ResetFieldVelocity();
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
            var next = _logic.TickRunTowards(
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

        public async UniTask PlayReshuffleTweenAsync(
            float moveDuration,
            float arriveThreshold,
            CancellationToken ct)
        {
            if (!_isAlive)
                return;

            KillReshuffleTween();
            ApplyReshuffleHeal();

            var completePromotion = _runningToGoal;
            var target = completePromotion ? _runTarget : ResolveReshuffleTarget();
            var current = (Vector2)transform.position;
            var offset = target - current;

            if (offset.sqrMagnitude <= arriveThreshold * arriveThreshold)
            {
                SnapTo(target);
                if (completePromotion)
                    CompleteRunToGoal();
                else
                    _bus?.Publish(new DefenderReturnedHomeEvent(slotId));

                return;
            }

            _reshuffleMoving = true;

            try
            {
                var distance = offset.magnitude;
                var refDistance = 4f;
                var duration = moveDuration * (distance / refDistance);
                duration = Mathf.Clamp(duration, 0.12f, 1.4f);

                var end = new Vector3(target.x, target.y, transform.position.z);
                _reshuffleTween = transform
                    .DOMove(end, duration)
                    .SetEase(Ease.InOutQuad)
                    .SetLink(gameObject);

                await TweenAsync.Await(_reshuffleTween, ct);

                if (!_isAlive || ct.IsCancellationRequested)
                    return;

                SnapTo(target);
            }
            finally
            {
                _reshuffleMoving = false;
                _reshuffleTween = null;
            }

            if (!_isAlive)
                return;

            if (completePromotion)
                CompleteRunToGoal();
            else
                _bus?.Publish(new DefenderReturnedHomeEvent(slotId));
        }

        public void KillReshuffleTween()
        {
            if (_reshuffleTween != null && _reshuffleTween.IsActive())
                _reshuffleTween.Kill();

            _reshuffleTween = null;
            transform.DOKill();
            _reshuffleMoving = false;
        }

        private void SnapTo(Vector2 target)
        {
            transform.position = new Vector3(target.x, target.y, transform.position.z);
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
            KillReshuffleTween();
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

            _logic.ResetGoalkeeperParam(zone.ParamFromWorldX(transform.position.x));
        }

        private void SnapGoalkeeperToBall()
        {
            var zone = ResolveGoalAnchor();
            if (zone == null)
                return;

            var t = zone.ParamFromWorldX(ResolveBallWorldX());
            _logic.ResetGoalkeeperParam(t);
            var position = zone.PositionOnParabola(t);
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }

        private float ResolveBallWorldX()
        {
            return _ball != null ? _ball.Position.x : transform.position.x;
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
            if (Time.time - _lastInteractionTime < interactionCooldown)
                return;

            _lastInteractionTime = Time.time;
            _logic.ResolveBallHit(motion, hit, this);
            ApplyDamage(1);
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
            KillReshuffleTween();

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

            var label = $"#{slotId}  {role}\nHit: {hitType}";
            if (_runningToGoal)
                label += "\n→ GK";
            if (role == DefenderRole.Field && !_runningToGoal)
                label += $"\nMove: {movementType}";
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

            var alpha = selected ? 0.85f : 0.4f;
            DefenderGizmoDrawer.DrawWireCircle(
                center,
                separationRadius,
                new Color(1f, 0.55f, 0.1f, alpha * 0.7f));

            switch (movementType)
            {
                case DefenderMovementType.PatrolGenerated:
                    DefenderGizmoDrawer.DrawWireCircle(
                        center,
                        patrolRadius,
                        new Color(0.3f, 1f, 0.45f, alpha));
                    var path = PatrolPathGenerator.Generate(
                        home,
                        patrolPointCount,
                        patrolRadius,
                        slotId * 7919 + 17);
                    DefenderGizmoDrawer.DrawPatrolPath(
                        path,
                        new Color(1f, 0.92f, 0.2f, alpha),
                        closed: true);
                    break;

                case DefenderMovementType.WanderInRadius:
                    DefenderGizmoDrawer.DrawWireCircle(
                        center,
                        wanderRadius,
                        new Color(0.3f, 0.75f, 1f, alpha));
                    break;

                case DefenderMovementType.ChaseBallInRadius:
                    DefenderGizmoDrawer.DrawWireCircle(
                        center,
                        chaseRadius,
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
