using System;
using Mimi.Actor.Core;
using UnityEngine;

namespace Mimi.Actor.Graphic.Core
{
    public abstract class BaseMonoGraphic : MonoActorAspect, IGraphic
    {
        public int SortingOrder => WrapperGraphic.SortingOrder;
        public string SortingLayerName => WrapperGraphic.SortingLayerName;
        public IGraphicAsset CurrentGraphicAsset => WrapperGraphic.CurrentGraphicAsset;


        private IGraphic wrapperGraphic;

        protected virtual IGraphic WrapperGraphic
        {
            get
            {
                if (wrapperGraphic == null)
                {
                    wrapperGraphic = CreateGraphic();
                }
                return wrapperGraphic;
            }
        }
        
        protected abstract IGraphic CreateGraphic();
        public void SetAssetGraphic(IGraphicAsset graphic)
        {
            WrapperGraphic.SetAssetGraphic(graphic);
        }

        public void SetSortingOrder(int sortingOrder)
        {
            WrapperGraphic.SetSortingOrder(sortingOrder);
        }

        public void SetSortingLayerName(string sortingLayerName)
        {
            WrapperGraphic.SetSortingLayerName(sortingLayerName);
        }

        public void Show()
        {
            WrapperGraphic.Show();
        }

        public void Hide()
        {
            WrapperGraphic.Hide();
        }

        public void SetAlpha(float alpha)
        {
            WrapperGraphic.SetAlpha(alpha);
        }

        public override void Initialize()
        {
            wrapperGraphic =  CreateGraphic();
        }

        public override void Dispose()
        {
            
        }
    }
}