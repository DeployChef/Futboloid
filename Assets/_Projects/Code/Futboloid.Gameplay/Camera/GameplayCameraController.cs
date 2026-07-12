using System;
using System.Collections.Generic;
using Cinemachine;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using Futboloid.Gameplay.Ball;
using Futboloid.Gameplay.Keeper;
using Futboloid.Gameplay.Match;
using UnityEngine;
using VContainer;

namespace Futboloid.Gameplay.Camera
{
  /// <summary>
  /// Тактическая камера: сдвиг влево-вправо за вратарём игрока, зум по Y мяча, тряска через Impulse.
  /// Пинки — лёгкий шейк; стены и контакты — только когда мяч разогнан (≥ Fire Speed Threshold).
  /// </summary>
  public sealed class GameplayCameraController : MonoBehaviour
  {
    [SerializeField] private Transform cameraFocus;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private CinemachineImpulseSource impulseSource;

    [Header("Framing")]
    [SerializeField, Range(0f, 1.5f)] private float keeperInfluenceX = 0.6f;
    [SerializeField] private float focusSmoothTime = 0.35f;
    [SerializeField] private bool clearFramingDeadZone = true;

    [Header("Zoom")]
    [SerializeField] private float closeOrthoSize = 8.8f;
    [SerializeField] private float farOrthoSize = 10f;
    [SerializeField] private float zoomSmoothTime = 0.45f;

    [Header("Impulse")]
    [SerializeField] private float kickImpulseForce = 0.005f;
    [SerializeField] private float wallImpulseForce = 0.015f;
    [SerializeField] private float defenderContactImpulseForce = 0.008f;
    [SerializeField] private float defenderHitImpulseForce = 0.012f;
    [SerializeField] private float goalImpulseForce = 0.018f;
    [SerializeField] private float maxSpeedForImpulse = 20f;

    private readonly List<IDisposable> _subscriptions = new();

    private IGameEventBus _bus;
    private BallView _ball;
    private PitchBounds _pitchBounds;
    private GoalkeeperView _goalkeeper;

    private bool _onField;
    private Vector3 _focusVelocity;
    private float _orthoVelocity;
    private float _currentOrthoSize;

    [Inject]
    public void Construct(
      IGameEventBus bus,
      BallView ball,
      PitchBounds pitchBounds,
      GoalkeeperView goalkeeper,
      MatchFlow matchFlow)
    {
      _bus = bus;
      _ball = ball;
      _pitchBounds = pitchBounds;
      _goalkeeper = goalkeeper;
      _onField = matchFlow.IsOnField;
      _currentOrthoSize = ReadOrthoSize();
      ConfigureFramingTransposer();

      _subscriptions.Add(bus.Subscribe<BallContactEvent>(OnBallContact));
      _subscriptions.Add(bus.Subscribe<BallServedEvent>(OnBallServed));
      _subscriptions.Add(bus.Subscribe<DefenderHitEvent>(OnDefenderHit));
      _subscriptions.Add(bus.Subscribe<GoalScoredEvent>(OnGoalScored));
      _subscriptions.Add(bus.Subscribe<NavigationChangedEvent>(OnNavigationChanged));
    }

    private void LateUpdate()
    {
      if (!_onField || cameraFocus == null || _ball == null || _pitchBounds == null)
        return;

      UpdateFocus();
      UpdateZoom();
    }

    private void ConfigureFramingTransposer()
    {
      if (!clearFramingDeadZone || virtualCamera == null)
        return;

      var body = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
      if (body == null)
        return;

      // Dead zone в screen-space гасит небольшой сдвиг CameraFocus по X за вратарём.
      body.m_DeadZoneWidth = 0f;
      body.m_DeadZoneHeight = 0f;
    }

    private void UpdateFocus()
    {
      var pitchCenter = (Vector3)_pitchBounds.Center;
      var keeperX = _goalkeeper != null
        ? _goalkeeper.transform.position.x
        : pitchCenter.x;

      var offsetX = (keeperX - pitchCenter.x) * keeperInfluenceX;
      var target = new Vector3(
        pitchCenter.x + offsetX,
        pitchCenter.y,
        cameraFocus.position.z);

      cameraFocus.position = Vector3.SmoothDamp(
        cameraFocus.position,
        target,
        ref _focusVelocity,
        focusSmoothTime);
    }

    private void UpdateZoom()
    {
      var keeperY = _goalkeeper != null
        ? _goalkeeper.transform.position.y
        : _pitchBounds.MinY;

      var ballY = _ball.Position.y;
      var t = Mathf.InverseLerp(keeperY, _pitchBounds.MaxY, ballY);
      var targetOrtho = Mathf.Lerp(closeOrthoSize, farOrthoSize, t);

      _currentOrthoSize = Mathf.SmoothDamp(
        _currentOrthoSize,
        targetOrtho,
        ref _orthoVelocity,
        zoomSmoothTime);

      ApplyOrthoSize(_currentOrthoSize);
    }

    private void OnBallContact(BallContactEvent e)
    {
      if (!_onField || impulseSource == null || _ball == null)
        return;

      switch (e.Kind)
      {
        case BallContactKind.PlayerKeeper:
          FireImpulse(kickImpulseForce, e.Point);
          return;

        case BallContactKind.Wall:
          if (!WasAcceleratedAtContact(e))
            return;

          FireImpulse(wallImpulseForce * SpeedImpulseScale(ImpactSpeed(e)), e.Point);
          return;

        case BallContactKind.Defender:
          if (!WasAcceleratedAtContact(e))
            return;

          FireImpulse(defenderContactImpulseForce * SpeedImpulseScale(ImpactSpeed(e)), e.Point);
          return;
      }
    }

    private void OnBallServed(BallServedEvent _)
    {
      if (!_onField || impulseSource == null || _ball == null)
        return;

      FireImpulse(kickImpulseForce, _ball.Position);
    }

    private void OnDefenderHit(DefenderHitEvent _)
    {
      if (!_onField || impulseSource == null || _ball == null || !_ball.IsOnFire)
        return;

      FireImpulse(defenderHitImpulseForce * SpeedImpulseScale(_ball.Speed), _ball.Position);
    }

    private void OnGoalScored(GoalScoredEvent _)
    {
      if (!_onField || impulseSource == null || _ball == null)
        return;

      FireImpulse(goalImpulseForce, _ball.Position);
    }

    private void OnNavigationChanged(NavigationChangedEvent e) =>
      _onField = e.Current == NavigationState.OnField;

    private bool WasAcceleratedAtContact(BallContactEvent e) =>
      ImpactSpeed(e) >= _ball.Settings.FireSpeedThreshold;

    private float ImpactSpeed(BallContactEvent e)
    {
      var settings = _ball.Settings;

      return e.Kind switch
      {
        BallContactKind.Wall => e.Speed + settings.WallSpeedPenalty,
        BallContactKind.Defender => Mathf.Max(0f, e.Speed - settings.DefenderHitBoost),
        BallContactKind.PlayerKeeper => Mathf.Max(0f, e.Speed - settings.KeeperBoost),
        _ => e.Speed
      };
    }

    private float SpeedImpulseScale(float speed)
    {
      if (maxSpeedForImpulse <= 0.001f)
        return 1f;

      return Mathf.Lerp(0.9f, 1.25f, Mathf.Clamp01(speed / maxSpeedForImpulse));
    }

    private void FireImpulse(float force, Vector2 worldPoint)
    {
      if (force <= 0f)
        return;

      var direction = UnityEngine.Random.insideUnitCircle.normalized;
      if (direction.sqrMagnitude < 0.0001f)
        direction = Vector2.up;

      var velocity = new Vector3(direction.x, direction.y, 0f) * force;
      var position = new Vector3(worldPoint.x, worldPoint.y, 0f);
      impulseSource.GenerateImpulseAtPositionWithVelocity(position, velocity);
    }

    private float ReadOrthoSize()
    {
      if (virtualCamera == null)
        return closeOrthoSize;

      return virtualCamera.m_Lens.OrthographicSize;
    }

    private void ApplyOrthoSize(float size)
    {
      if (virtualCamera == null)
        return;

      var lens = virtualCamera.m_Lens;
      lens.OrthographicSize = size;
      virtualCamera.m_Lens = lens;
    }

    private void OnDestroy()
    {
      foreach (var subscription in _subscriptions)
        subscription.Dispose();

      _subscriptions.Clear();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
      if (virtualCamera != null && !Application.isPlaying)
        _currentOrthoSize = virtualCamera.m_Lens.OrthographicSize;
    }
#endif
  }
}
