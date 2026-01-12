using System;
using DarkTonic.MasterAudio;
using Mimi.Prototypes.Currencies;
using Mimi.ServiceLocators;
using UnityEngine;

namespace Mimi.Prototypes.UI
{
    public abstract class BaseModalDialog : MonoBehaviour
    {
        [SerializeField, SoundGroup] private string showSound;

        public event Action<BaseModalDialog> OnHiding;

        // Multiple dialog can show. So next dialog need to increase sort order.
        protected static int AdditionalSortOrder;

        public DialogId DialogId;

        public virtual void Show()
        {
            AdditionalSortOrder++;
            IncreaseSortOrder();
            ServiceLocator.Global.Get<IAudioService>().PlaySound(this.showSound);
        }

        private void IncreaseSortOrder()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder += AdditionalSortOrder;
            }
        }

        public virtual void Hide()
        {
            AdditionalSortOrder--;
            OnHiding?.Invoke(this);
        }
    }
}