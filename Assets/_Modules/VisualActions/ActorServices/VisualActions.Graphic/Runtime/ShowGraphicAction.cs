using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Actor.Graphic.Core;
using UnityEngine;

namespace Mimi.VisualActions.Graphic
{
    public class ShowGraphicAction : VisualAction
    {
        [SerializeField] private BaseMonoGraphic targetGraphic;
        [SerializeField] private bool isShowGraphic;
        protected override UniTask OnExecuting(CancellationToken cancellationToken)
        {
            if (isShowGraphic)
            {
                targetGraphic.Show();
            }
            else
            {
                targetGraphic.Hide();
            }
            return UniTask.CompletedTask;
        }
    }
}