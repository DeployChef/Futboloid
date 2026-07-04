namespace Futboloid.Gameplay.Input
{
    public interface IGameplayInput
    {
        float MoveX { get; }
        bool WasServePressedThisFrame { get; }
    }
}
