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
        [SerializeField] private CharacterAnimationPresenter animationPresenter;

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
        [FormerlySerializedAs("paramSpeed")]
        [Tooltip("Скорость слежения по параметру t (−1…1) в секунду — насколько быстро GK едет по дуге к X мяча.")]
        [SerializeField] private float trackSpeed = 1.25f;

        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private float gizmoLabelHeight = 0.35f;
        [SerializeField] private float gizmoLabelPadding = 0.12f;

        private IGameEventBus _bus;
        private DefenderGridRegistry _registry;
        private DefenderLogic _logic;
        private BallView _ball;
        private PitchBounds _pitchBounds;
        private GoalAnchor _goalAnchor;
        private readonly List<IDisposable> _subscriptions = new();
        private int _hp;
        private bool _isAlive = true;
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
        private readonly DefenderReshuffleMotion _reshuffle = new();

        public int SlotId => slotId;
        public Collider2D ContactCollider => bodyCollider;
        public DefenderRole Role => role;
        public DefenderHitType HitType => hitType;
        public float LaunchSpeed => launchSpeed;
        public int OpenGoalChancePercent => openGoalChancePercent;
        public bool IsAlive => _isAlive;
        public bool RunningToGoal => _runningToGoal;
        public Vector2 HomePosition => _homePosition;

        public void ApplySpawnSetup(in DefenderBuild build, Vector2 home)
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
                Debug.LogWarning("[DefenderView] bodyCollider is not assigned.", this);

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

        private void SyncPitchState(PitchPhase phase)
        {
            _simulating = phase == PitchPhase.Simulating;
        }

        private void Update()
        {
            if (!_isAlive)
            {
                SyncRunningAnimation(false);
                return;
            }

            if (_reshuffle.IsActive)
            {
                SyncRunningAnimation(true);
                return;
            }

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

        private void SyncRunningAnimation(bool isRunning) =>
            animationPresenter?.SetRunning(isRunning);

        private void TickGoalkeeper()
        {
            var current = (Vector2)transform.position;
            var ballX = ResolveBallWorldX();
            var result = _logic.TickGoalkeeperOnParabola(current, _goalAnchor, ballX, trackSpeed, Time.deltaTime);
            var position = _pitchBounds != null ? _pitchBounds.Clamp(result.Position) : result.Position;
            transform.position = new Vector3(position.x, position.y, transform.position.z);
            SyncRunningAnimation(result.IsMoving);
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
                chaseRadius,
                ballPosition,
                fieldMoveSpeed,
                fieldAcceleration,
                fieldArriveThreshold,
                separationRadius,
                _neighborPositions,
                Time.deltaTime);
            transform.position = new Vector3(result.Position.x, result.Position.y, transform.position.z);
            SyncRunningAnimation(result.IsMoving);
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
            if (!_isAlive || role != DefenderRole.Field)
                return;

            _runSpeed = maxSpeed;
            _runAcceleration = acceleration;
            _runArriveThreshold = arriveThreshold;
            _runTarget = ResolveRunTarget();
            _logic.ResetRunVelocity();
            _logic.ResetFieldVelocity();
            _runningToGoal = true;
            SyncRunningAnimation(true);
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
            var result = _logic.TickRunTowards(
                current,
                _runTarget,
                _runSpeed,
                _runAcceleration,
                _runArriveThreshold,
                Time.deltaTime,
                out var arrived);
            transform.position = new Vector3(result.Position.x, result.Position.y, transform.position.z);
            SyncRunningAnimation(result.IsMoving);

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
            SyncRunningAnimation(true);
            ApplyReshuffleHeal();

            var completePromotion = _runningToGoal;
            var target = completePromotion ? _runTarget : ResolveReshuffleTarget();

            var arrived = await _reshuffle.PlayToAsync(
                transform,
                target,
                arriveThreshold,
                moveDuration,
                ct);

            if (!_isAlive || !arrived)
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

        private float ResolveBallWorldX()
        {
            return _ball != null ? _ball.Position.x : transform.position.x;
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
            SyncRunningAnimation(false);
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
                return;

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
            if (bodyCollider != null)
                return new Vector3(center.x, bodyCollider.bounds.max.y + gizmoLabelPadding, center.z);

            return center + Vector3.up * gizmoLabelHeight;
        }
    }
}
