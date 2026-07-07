namespace Futboloid.Gameplay.Defenders
{
    /// <summary>
    /// Визуальный и геймплейный профиль: уникальная пара Hit + Move.
    /// Несколько архетипов могут делить один Kind (Shield/Tank, Striker/Presser).
    /// </summary>
    public enum DefenderBehaviorKind
    {
        ReflectIdle = 0,
        ReflectWander = 1,
        ReflectChase = 2,
        ReflectPatrol = 3,
        ShootIdle = 4,
        ShootWander = 5,
        ShootChase = 6,
        ShootPatrol = 7
    }
}
