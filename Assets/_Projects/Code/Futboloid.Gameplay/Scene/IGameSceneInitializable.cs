using Futboloid.Gameplay.Bus;

namespace Futboloid.Gameplay.Scene
{
  public interface IGameSceneInitializable
  {
    void Initialize(IGameEventBus bus);
  }
}
