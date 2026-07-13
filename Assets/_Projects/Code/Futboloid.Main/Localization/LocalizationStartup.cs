using Cysharp.Threading.Tasks;
using Futboloid.Main.Localization;
using VContainer.Unity;

namespace Futboloid.Main.Localization
{
    public sealed class LocalizationStartup : IStartable
    {
        private readonly LocalizationService _localization;

        public LocalizationStartup(LocalizationService localization)
        {
            _localization = localization;
        }

        public void Start()
        {
            _localization.InitializeAsync().Forget();
        }
    }
}
