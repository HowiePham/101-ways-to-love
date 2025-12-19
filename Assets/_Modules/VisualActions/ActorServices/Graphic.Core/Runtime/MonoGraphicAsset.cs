using UnityEngine;

namespace Mimi.Actor.Graphic.Core
{
    public abstract class MonoGraphicAsset : MonoBehaviour, IGraphicAsset
    {
        
    }
    public abstract class MonoGraphicAsset<T> : MonoGraphicAsset,IGraphicAsset<T>
    {
        private IGraphicAsset<T> wrapper;

        private IGraphicAsset<T> Wrapper
        {
            get
            {
                if (wrapper == null)
                {
                    wrapper = CreateAssetGraphic();
                }
                return wrapper;
            }
        }
        
        protected abstract IGraphicAsset<T> CreateAssetGraphic();

        public void SetAssetGraphic(T asset)
        {
            Wrapper.SetAssetGraphic(asset);
        }

        public T GetAssetGraphic()
        {
            return Wrapper.GetAssetGraphic();
        }

     
    }
}