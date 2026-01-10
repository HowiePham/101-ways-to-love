using _Modules.Gameflow_Events_.Scripts;
using Mimi.Events.AsyncBus;
using Mimi.Prototypes.UI;
using UnityEngine;

namespace _Modules._UI.CheatView.Scripts
{
    public class CheatViewPresenter : BaseViewPresenter<CheatViewPresenter>
    {
        private CheatView cheatView;
        private readonly IAsyncPublisher eventPublisher;

        public CheatViewPresenter(BaseScenePresenter scenePresenter, Transform transform, IAsyncPublisher eventPublisher) : base(scenePresenter, transform)
        {
            this.eventPublisher = eventPublisher;
        }

        protected override void AddViews()
        {
            this.cheatView = AddView<CheatView>();
        }

        protected override void AddChildren()
        {
        }

        protected override void OnShow()
        {
            base.OnShow();

            this.cheatView.SelectLevelClicked += CheatSelectingLevel;
        }

        private void CheatSelectingLevel(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            int currentSelectedLevel = int.Parse(text);
            int levelOrder = currentSelectedLevel - 1;
            Debug.Log($"--- (CHEAT) Selected Level: {levelOrder}");

            this.eventPublisher.PublishAsync(new SelectLevel(levelOrder));
        }
    }
}