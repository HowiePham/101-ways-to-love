using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi;
using Mimi.Prototypes.Currencies;
using Mimi.Prototypes.UI;
using Mimi.ServiceLocators;
using UnityEngine;

namespace VisualFlow
{
    public class ShowTextHint : BaseHint
    {
        [SerializeField] private string hintTextId;

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            if (ServiceLocator.Global.Get<DialogManager>()
                .TryShowModalDialogOnce(DialogId.Hint, out NotificationOkDialog dialog))
            {
                dialog.SetContentText(ServiceLocator.Global.Get<ILocalizationService>()
                    .Localize(this.hintTextId));
                await UniTask.CompletedTask;
            }
        }
    }
}