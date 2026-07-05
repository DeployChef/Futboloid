using Futboloid.Core;
using Futboloid.Core.Audio;
using Futboloid.Core.Bus;
using Futboloid.Core.Run;
using Futboloid.Gameplay.Match;
using Futboloid.Main.Navigation;
using Futboloid.Main.Session;
using VContainer;
namespace Futboloid.Main.DI
{
    public static class AppScopeExtensions
    {
        public static IContainerBuilder RegisterAppScope(this IContainerBuilder builder)
        {
            var gameplaySettings = GameplaySettings.Load();
            builder.RegisterInstance(gameplaySettings);
            builder.RegisterInstance(gameplaySettings.DefenderGeneration);
            builder.Register<IGameEventBus, GameEventBus>(Lifetime.Singleton);
            builder.Register<TournamentRunService>(Lifetime.Singleton)
                .As<ITournamentRunService>()
                .As<ITournamentBracketReadModel>();
            builder.Register<OverlayStateController>(Lifetime.Singleton);
            builder.Register<MatchEndHandler>(Lifetime.Singleton);

            var audioCatalog = AudioCatalog.Load();
            builder.RegisterInstance(audioCatalog);
            builder.Register<AudioService>(Lifetime.Singleton);
            builder.RegisterBuildCallback(resolver => resolver.Resolve<AudioService>());

            var runProgressionSettings = RunProgressionSettings.Load();
            var perkCatalog = PerkCatalog.Load();
            builder.RegisterInstance(runProgressionSettings);
            builder.RegisterInstance(perkCatalog);
            builder.Register<RunStateService>(Lifetime.Singleton)
                .As<IRunProgressionService>();

            return builder;
        }
    }
}
