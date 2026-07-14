using System;
using Futboloid.Core;
using Futboloid.Core.Bus;
using Futboloid.Core.Bus.Events;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Futboloid.UI.Views.Tournament
{
    /// <summary>
    /// Показывает FirstTimeGuide только при первом матче и только при первом запуске игры.
    /// При рестарте турнира (RestartTournament) не показывается повторно.
    /// Скрывается при первой подаче (BallServedEvent).
    /// </summary>
    public class FirstTimeGuideWidget : MonoBehaviour
    {
        [SerializeField] private GameObject guidePanel;

        private ITournamentRunService _tournamentRun;
        private IDisposable _serveSubscription;

        private void Awake()
        {
            if (guidePanel == null)
                guidePanel = gameObject;

            guidePanel.SetActive(false);
            DisableGuideRaycasts();
        }

        private void OnDestroy()
        {
            _serveSubscription?.Dispose();
        }

        [Inject]
        public void Construct(ITournamentRunService tournamentRun, IGameEventBus bus)
        {
            _tournamentRun = tournamentRun;
            _serveSubscription = bus.Subscribe<BallServedEvent>(_ => OnBallServed());
            Refresh();
        }

        private void OnBallServed()
        {
            if (guidePanel.activeSelf)
                guidePanel.SetActive(false);
        }

        /// <summary>
        /// Вызывать при смене навигации или обновлении состояния турнира.
        /// </summary>
        public void Refresh()
        {
            var isFirstMatch = _tournamentRun != null && _tournamentRun.CurrentMatchNumber == 1;
            var isFirstLaunch = _tournamentRun != null && !_tournamentRun.HasPlayedBefore;

            guidePanel.SetActive(isFirstMatch && isFirstLaunch);
            if (guidePanel.activeSelf)
                DisableGuideRaycasts();
        }

        private void DisableGuideRaycasts()
        {
            if (guidePanel == null)
                return;

            var group = guidePanel.GetComponent<CanvasGroup>();
            if (group == null)
                group = guidePanel.AddComponent<CanvasGroup>();

            group.blocksRaycasts = false;
            group.interactable = false;

            var graphics = guidePanel.GetComponentsInChildren<Graphic>(true);
            for (var i = 0; i < graphics.Length; i++)
                graphics[i].raycastTarget = false;
        }
    }
}
