using _Modules.GameEvent.Scripts;
using Mimi.Events.AsyncBus;
using Mimi.Prototypes.UI;
using UnityEngine;

namespace _Modules._UI.LoseView.Scripts
{
    public class LoseViewPresenter : BaseViewPresenter
    {
        private LoseView loseView;
        private IAsyncPublisher eventPublisher;

        public LoseViewPresenter(BaseScenePresenter scenePresenter, Transform transform, IAsyncPublisher eventPublisher) : base(scenePresenter, transform)
        {
            this.eventPublisher = eventPublisher;
        }

        protected override void AddViews()
        {
            this.loseView = AddView<LoseView>();
        }

        protected override void AddChildren()
        {
        }

        protected override void OnShow()
        {
            base.OnShow();
            this.eventPublisher.PublishAsync(new ScreenShown("lose_view"));

            this.loseView.OnTryAgainClicked += TryAgainClickedHandler;
        }

        protected override void OnHide()
        {
            base.OnHide();

            this.loseView.OnTryAgainClicked -= TryAgainClickedHandler;
        }

        private void TryAgainClickedHandler()
        {
            Debug.Log($"--- (Lose) Try Again Clicked");
            this.eventPublisher.PublishAsync(new LevelTryAgain());
            Hide();
        }
    }
}