using UnityEngine;
using UnityEngine.Serialization;

namespace Futboloid.Gameplay.Defenders
{
    [CreateAssetMenu(fileName = "DefenderGoalkeeper_Default", menuName = "Futboloid/Defenders/Goalkeeper Behavior")]
    public class DefenderGoalkeeperBehavior : ScriptableObject
    {
        [FormerlySerializedAs("paramSpeed")]
        [Tooltip("Скорость слежения по параметру t (−1…1) в секунду — насколько быстро GK едет по дуге к X мяча.")]
        [SerializeField] private float trackSpeed = 2.5f;

        public float TrackSpeed => trackSpeed;
    }
}
