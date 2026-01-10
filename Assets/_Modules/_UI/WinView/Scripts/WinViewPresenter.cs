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
        private RuntimeState runtimeState;

        public WinViewPresenter(BaseScenePresenter scenePresenter, Transform transform, IAsyncPublisher eventPublisher, RuntimeState runtimeState) : base(scenePresenter, transform)
        {
            this.eventPublisher = eventPublisher;
            this.runtimeState = runtimeState;
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
            this.winView.SetLevelText(this.runtimeState.CurrentLevelOrder.Value + 1);
        }

        protected override void OnHide()
        {
            base.OnHide();

            this.winView.OnContinueClicked -= ContinueClickedHandler;
        }

        private void ContinueClickedHandler()
        {
            Debug.Log($"--- (WIN) Continue Clicked");
            this.eventPublisher.PublishAsync(new NextLevelClicked());
            Hide();
        }
    }
}