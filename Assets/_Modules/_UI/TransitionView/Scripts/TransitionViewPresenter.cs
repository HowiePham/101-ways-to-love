using System;
using Cysharp.Threading.Tasks;
using Mimi.Prototypes.UI;
using UnityEngine;

namespace _Modules._UI.TransitionView.Scripts
{
    public class TransitionViewPresenter : BaseViewPresenter
    {
        private TransitionView transitionView;
        public TransitionViewPresenter(BaseScenePresenter scenePresenter, Transform transform) : base(scenePresenter, transform)
        {
        }

        protected override void AddViews()
        {
            this.transitionView = AddView<TransitionView>();
        }

        protected override void OnShow()
        {
            base.OnShow();
            this.transitionView.OnTransitionEnd += Hide;
            
        }

        public void ShowEffect(Func<UniTask> task,float delay=0)
        {
            this.Show();
            this.transitionView.ShowTransition(task,delay);
        }

        public void ShowEffect()
        {
            this.Show();
            this.transitionView.ShowTransition();
        }

       
        protected override void OnHide()
        {
            base.OnHide();
            this.transitionView.OnTransitionEnd -= Hide;
        }

        protected override void AddChildren()
        {
            
        }
    }
}