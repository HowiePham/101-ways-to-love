using _Modules._UI.LoseView.Scripts;
using Mimi.Events;
using Mimi.Events.AsyncBus;
using Mimi.Prototypes;
using Mimi.Prototypes.UI;
using UnityEngine;

namespace _Modules._UI.WinView.Scripts
{
    public class WinViewPresenter : BaseViewPresenter
    {
        private WinView winView;
        private IAsyncPublisher eventPublisher;

        public WinViewPresenter(BaseScenePresenter scenePresenter, Transform transform, IAsyncPublisher eventPublisher, RuntimeState runtimeState) : base(scenePresenter, transform)
        {
            this.eventPublisher = eventPublisher;
        }

        protected override void AddViews()
        {
            this.winView = AddView<WinView>();
        }

        protected override void AddChildren()
        {
        }

        protected override void OnShow()
        {
            base.OnShow();

            this.winView.OnContinueClicked += ContinueClickedHandler;
            this.winView.OnReplayClicked += ReplayClickedHandler;
        }

        protected override void OnHide()
        {
            base.OnHide();

            this.winView.OnContinueClicked -= ContinueClickedHandler;
            this.winView.OnReplayClicked -= ReplayClickedHandler;
        }

        private void ContinueClickedHandler()
        {
            Debug.Log($"--- (WIN) Continue Clicked");
            this.eventPublisher.PublishAsync(new NextLevelClicked());
            Hide();
        }
        
        private void ReplayClickedHandler()
        {
            Debug.Log($"--- (WIN) Replay Clicked");
            this.eventPublisher.PublishAsync(new LevelTryAgain());
            Hide();
        }
    }
}