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
        private CurrencyView currencyView;
        private IAsyncPublisher eventPublisher;

        public WinViewPresenter(BaseScenePresenter scenePresenter, Transform transform, IAsyncPublisher eventPublisher, RuntimeState runtimeState) : base(scenePresenter, transform)
        {
            this.eventPublisher = eventPublisher;
        }

        protected override void AddViews()
        {
            this.winView = AddView<WinView>();
            this.currencyView = AddView<CurrencyView>();
        }

        protected override void AddChildren()
        {
        }

        protected override void OnShow()
        {
            base.OnShow();

            this.winView.OnContinueClicked += ContinueClickedHandler;
            this.winView.OnReplayClicked += ReplayClickedHandler;
            this.currencyView.OnAddCurrencyClicked += AddCurrencyClickedHandler;
        }

        protected override void OnHide()
        {
            base.OnHide();

            this.winView.OnContinueClicked -= ContinueClickedHandler;
            this.winView.OnReplayClicked -= ReplayClickedHandler;
            this.currencyView.OnAddCurrencyClicked -= AddCurrencyClickedHandler;
        }

        private void AddCurrencyClickedHandler()
        {
            Debug.Log($"--- (Currency) Add currency clicked");
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