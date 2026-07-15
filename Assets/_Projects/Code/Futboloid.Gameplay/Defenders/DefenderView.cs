using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Gameplay.Ball;
using Futboloid.Gameplay.Characters;
using Futboloid.Gameplay.Match;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace Futboloid.Gameplay.Defenders
{
    public class DefenderView : MonoBehaviour
    {
        [SerializeField] private int slotId;
        [SerializeField] private DefenderRole role = DefenderRole.Field;
        [SerializeField] private Collider2D bodyCollider;
        [SerializeField] private CharacterAnimationPresenter animationPresenter;
        [SerializeField] private DefenderBehaviorVisual behaviorVisual;
        [SerializeField] private DefenderHealth health;

        [Header("Hit")]
        [SerializeField] private DefenderHitType hitType = DefenderHitType.Reflect;
        [SerializeField] private float launchSpeed = 12f;
        [Tooltip("Шанс 0–100 пнуть дальше от вратаря (в противоположный угол). Иначе — ближе к вратарю.")]
        [FormerlySerializedAs("openGoalWeight")]
        [SerializeField] [Range(0, 100)] private int openGoalChancePercent = 70;
        [Tooltip("Минимальный интервал между взаимодействиями с мячом: отскок и урон (сек).")]
        [FormerlySerializedAs("damageCooldown")]
        [SerializeField] private float interactionCooldown = 0.1f;
        [SerializeField] private int pointValue = 10;

        [Header("Movement")]
        [SerializeField] private DefenderMovementType movementType = DefenderMovementType.Idle;
        [SerializeField] private int patrolPointCount = 4;
        [SerializeField] private float patrolRadius = 1.5f;
        [SerializeField] private float wanderRadius = 1.5f;
        [SerializeField] private float separationRadius = 0.6f;
        [SerializeField] private float fieldMoveSpeed = 1.6f;
        [SerializeField] private float fieldAcceleration = 12f;
        [SerializeField] private float fieldArriveThreshold = 0.12f;

        [Header("Goalkeeper")]
        [FormerlySerializedAs("paramSpeed")]
        [Tooltip("Скорость слежения по параметру t (−1…1) в секунду — насколько быстро GK едет по дуге к X мяча.")]
        [SerializeField] private float trackSpeed = 1.25f;

        private IGameEventBus _bus;
        private DefenderGridRegistry _registry;
        private DefenderLogic _logic;
        private BallView _ball;
        private PitchBounds _pitchBounds;
        private GoalAnchor _goalAnchor;
        private readonly List<IDisposable> _subscriptions = new();
        private bool _onField;
        private bool _simulating;
        private bool _runningToGoal;
        private float _runSpeed = 4f;
        private float _runAcceleration = 18f;
        private float _runArriveThreshold = 0.08f;
        private Vector2 _runTarget;
        private Vector2 _homePosition;
        private float _lastInteractionTime = float.NegativeInfinity;
        private readonly List<Vector2> _neighborPositions = new();
        private float _previousFlipPositionX;
        private readonly DefenderReshuffleMotion _reshuffle = new();

        public int SlotId => slotId;
        public int PointValue => pointValue;
        public Collider2D ContactCollider => bodyCollider;
        public DefenderRole Role => role;
        public DefenderHitType HitType => hitType;
        public float LaunchSpeed => launchSpeed;
        public int OpenGoalChancePercent => openGoalChancePercent;
        public bool IsAlive => health.IsAlive;
        public bool RunningToGoal => _runningToGoal;
        public Vector2 HomePosition => _homePosition;
        public DefenderMovementType MovementType => movementType;
        public DefenderBehaviorKind BehaviorKind => DefenderBehaviorMapping.From(hitType, movementType);
        public int PatrolPointCount => patrolPointCount;
        public float PatrolRadius => patrolRadius;
        public float WanderRadius => wanderRadius;
        public float SeparationRadius => separationRadius;

        public void ApplySpawnSetup(in DefenderBuild build, Vector2 home)
        {
            slotId = build.SlotId;
            role = build.Role;
            hitType = build.HitType;
            movementType = build.MovementType;
            patrolPointCount = build.PatrolPointCount;
            patrolRadius = build.PatrolRadius;
            wanderRadius = build.WanderRadius;
            separationRadius = build.SeparationRadius;
            fieldMoveSpeed = build.FieldMoveSpeed;
            fieldAcceleration = build.FieldAcceleration;
            fieldArriveThreshold = build.FieldArriveThreshold;
            launchSpeed = build.LaunchSpeed;
            openGoalChancePercent = build.OpenGoalChancePercent;
            interactionCooldown = build.InteractionCooldown;
            pointValue = Mathf.Max(1, build.PointValue);

            if (build.TrackSpeed > 0f)
                trackSpeed = build.TrackSpeed;

            transform.position = new Vector3(home.x, home.y, transform.position.z);
            _homePosition = home;
            _runningToGoal = false;
            health.Configure(build.MaxHp);
            behaviorVisual?.Apply(hitType, movementType, role);
        }

        private void Awake()
        {
            if (behaviorVisual == null)
                behaviorVisual = GetComponentInChildren<DefenderBehaviorVisual>(true);

            if (bodyCollider == null)
                Debug.LogWarning("[DefenderView] bodyCollider is not assigned.", this);

            _homePosition = transform.position;
            _previousFlipPositionX = transform.position.x;
            health.Reset();
        }

        [Inject]
        public void Construct(
            IGameEventBus bus,
            PitchStateMachine pitch,
            MatchFlow matchFlow,
            DefenderGridRegistry registry,
            DefenderLogic logic,
            BallView ball,
            PitchBounds pitchBounds,
            GoalAnchor goalAnchor)
        {
            _bus = bus;
            _registry = registry;
            _logic = logic;
            _ball = ball;
            _pitchBounds = pitchBounds;
            _goalAnchor = goalAnchor;
            _subscriptions.Add(bus.Subscribe<PitchPhaseChangedEvent>(OnPitchPhaseChanged));
            _subscriptions.Add(bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));

            SyncPitchState(pitch.Current);
            _onField = matchFlow.IsOnField;
            _logic.InitializeFieldMovement(_homePosition, slotId, patrolPointCount, patrolRadius);
            _registry?.Register(this);
        }

        internal void NotifyDestroyed(bool wasGoalkeeper)
        {
            _runningToGoal = false;
            SyncRunningAnimation(false);
            KillReshuffleTween();

            if (bodyCollider != null)
                bodyCollider.enabled = false;

            _registry?.Unregister(this);
            Debug.Log($"[DefenderView] Slot {slotId} destroyed{(wasGoalkeeper ? " (GK)" : "")}.");
            Destroy(gameObject);
        }

        private void SyncPitchState(PitchPhase phase) => _simulating = phase == PitchPhase.Simulating;

        private void Update()
        {
            if (!health.IsAlive)
            {
                SyncRunningAnimation(false);
                return;
            }

            if (_reshuffle.IsActive)
            {
                var deltaX = transform.position.x - _previousFlipPositionX;
                _previousFlipPositionX = transform.position.x;
                SyncRunningAnimation(true, deltaX / Time.deltaTime);
                return;
            }

            _previousFlipPositionX = transform.position.x;

            if (_runningToGoal)
            {
                TickRunningToGoal();
                return;
            }

            if (!_onField || !_simulating)
            {
                SyncRunningAnimation(false);
                return;
            }

            if (role == DefenderRole.Goalkeeper)
            {
                TickGoalkeeper();
                return;
            }

            if (role == DefenderRole.Field)
                TickFieldMovement();
        }

        private void SyncRunningAnimation(bool isRunning, float velocityX = 0f) =>
            animationPresenter?.SetLocomotion(isRunning, velocityX);

        private void TickGoalkeeper()
        {
            var current = (Vector2)transform.position;
            var ballX = ResolveBallWorldX();
            var result = _logic.TickGoalkeeperOnParabola(current, _goalAnchor, ballX, trackSpeed, Time.deltaTime);
            var position = _pitchBounds != null ? _pitchBounds.Clamp(result.Position) : result.Position;
            transform.position = new Vector3(position.x, position.y, transform.position.z);
            SyncRunningAnimation(result.IsMoving, result.Velocity.x);
        }

        private void TickFieldMovement()
        {
            CollectNeighborPositions();

            Vector2? ballPosition = _ball != null ? _ball.Position : null;
            var current = (Vector2)transform.position;
            var result = _logic.TickFieldMovement(
                current,
                movementType,
                _homePosition,
                wanderRadius,
                ballPosition,
                fieldMoveSpeed,
                fieldAcceleration,
                fieldArriveThreshold,
                separationRadius,
                _neighborPositions,
                Time.deltaTime);
            transform.position = new Vector3(result.Position.x, result.Position.y, transform.position.z);
            SyncRunningAnimation(result.IsMoving, result.Velocity.x);
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

        public void BeginRunToGoal(float maxSpeed, float acceleration, float arriveThreshold)
        {
            if (!health.IsAlive || role != DefenderRole.Field)
                return;

            _runSpeed = maxSpeed;
            _runAcceleration = acceleration;
            _runArriveThreshold = arriveThreshold;
            _runTarget = ResolveRunTarget();
            _logic.ResetRunVelocity();
            _logic.ResetFieldVelocity();
            _runningToGoal = true;
            SyncRunningAnimation(true, _runTarget.x - transform.position.x);
        }

        public void SetRole(DefenderRole newRole)
        {
            if (role == newRole)
                return;

            role = newRole;
            _runningToGoal = false;
            behaviorVisual?.Apply(hitType, movementType, role);
            _bus?.Publish(new DefenderRoleChangedEvent(slotId, newRole == DefenderRole.Goalkeeper));

            if (newRole == DefenderRole.Goalkeeper)
                SyncGoalkeeperMotorFromPosition();
        }

        private void TickRunningToGoal()
        {
            var current = (Vector2)transform.position;
            var result = _logic.TickRunTowards(
                current,
                _runTarget,
                _runSpeed,
                _runAcceleration,
                _runArriveThreshold,
                Time.deltaTime,
                out var arrived);
            transform.position = new Vector3(result.Position.x, result.Position.y, transform.position.z);
            SyncRunningAnimation(result.IsMoving, result.Velocity.x);

            if (!arrived)
                return;

            CompleteRunToGoal();
        }

        public async UniTask PlayReshuffleTweenAsync(
            float moveDuration,
            float arriveThreshold,
            CancellationToken ct)
        {
            if (!health.IsAlive)
                return;

            KillReshuffleTween();
            _previousFlipPositionX = transform.position.x;

            var completePromotion = _runningToGoal;
            var target = completePromotion ? _runTarget : ResolveReshuffleTarget();
            SyncRunningAnimation(true, target.x - transform.position.x);
            ApplyReshuffleHeal();

            var arrived = await _reshuffle.PlayToAsync(
                transform,
                target,
                arriveThreshold,
                moveDuration,
                ct);

            if (!health.IsAlive || !arrived)
            {
                SyncRunningAnimation(false);
                return;
            }

            if (completePromotion)
                CompleteRunToGoal();
            else
                _bus?.Publish(new DefenderReturnedHomeEvent(slotId));
        }

        public void KillReshuffleTween()
        {
            _reshuffle.Kill(transform);
            SyncRunningAnimation(false);
        }

        private Vector2 ResolveReshuffleTarget()
        {
            if (role == DefenderRole.Goalkeeper)
                return _goalAnchor.PositionOnParabola(0f);

            return _homePosition;
        }

        private void ApplyReshuffleHeal()
        {
            var heal = Mathf.CeilToInt(health.MaxHp * 0.25f);
            health.Heal(heal);
        }

        private void CompleteRunToGoal()
        {
            _runningToGoal = false;
            SetRole(DefenderRole.Goalkeeper);
            _bus?.Publish(new DefenderPromotionCompletedEvent(slotId));
        }

        private Vector2 ResolveRunTarget() => _goalAnchor.PositionOnParabola(0f);

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
            _logic.ResetGoalkeeperParam(_goalAnchor.ParamFromWorldX(transform.position.x));
        }

        private void SnapGoalkeeperToBall()
        {
            var t = _goalAnchor.ParamFromWorldX(ResolveBallWorldX());
            _logic.ResetGoalkeeperParam(t);
            var position = _goalAnchor.PositionOnParabola(t);
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }

        private float ResolveBallWorldX() =>
            _ball != null ? _ball.Position.x : transform.position.x;

        public void HandleBallContact(BallMotion motion, RaycastHit2D hit)
        {
            if (Time.time - _lastInteractionTime < interactionCooldown)
                return;

            _lastInteractionTime = Time.time;
            _logic.ResolveBallHit(motion, hit, this);
            ApplyHitDamage();
        }

        /// <summary>
        /// Призрачный проход: урон без рикошета. GK не принимает.
        /// </summary>
        public bool TryApplyGhostPassHit()
        {
            if (role == DefenderRole.Goalkeeper)
                return false;

            if (Time.time - _lastInteractionTime < interactionCooldown)
                return false;

            _lastInteractionTime = Time.time;
            ApplyHitDamage();
            return true;
        }

        private void ApplyHitDamage()
        {
            var damage = _ball != null ? _ball.HitDamage : 1;
            health.ApplyDamage(damage, slotId, transform.position);
            _bus?.Publish(new DefenderHitEvent(slotId, pointValue));
        }
    }
}
