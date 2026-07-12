using System;
using UnityEngine;

namespace Futboloid.Gameplay.Ball
{
    [Serializable]
    public class BallSettings
    {
        [SerializeField] private float radius = 0.25f;
        [SerializeField] private float baseSpeed = 8f;
        [SerializeField] private float serveSpeed = 10f;
        [SerializeField] private float maxSpeed = 20f;
        [SerializeField] private float deceleration = 3f;
        [SerializeField] private float keeperBoost = 2f;
        [SerializeField] private float defenderHitBoost = 3f;
        [SerializeField] private float wallSpeedPenalty = 1.5f;
        [SerializeField] private float skin = 0.02f;
        [SerializeField] private float minVerticalComponent = 0.15f;

        [Header("Fire stage")]
        [SerializeField] private float fireSpeedThreshold = 14f;
        [SerializeField] private int fireExtraDamage = 1;
        [SerializeField] private float fireVfxFadeSpeed = 3.5f;

        public float Radius => radius;
        public float BaseSpeed => baseSpeed;
        public float ServeSpeed => serveSpeed;
        public float MaxSpeed => maxSpeed;
        public float Deceleration => deceleration;
        public float KeeperBoost => keeperBoost;
        public float DefenderHitBoost => Mathf.Max(0f, defenderHitBoost);
        public float WallSpeedPenalty => Mathf.Max(0f, wallSpeedPenalty);
        public float Skin => skin;
        public float MinVerticalComponent => minVerticalComponent;
        public float FireSpeedThreshold => fireSpeedThreshold;
        public int FireExtraDamage => Mathf.Max(0, fireExtraDamage);
        public float FireVfxFadeSpeed => Mathf.Max(0.1f, fireVfxFadeSpeed);
    }
}
