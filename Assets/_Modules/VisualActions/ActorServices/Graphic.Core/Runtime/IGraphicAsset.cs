using UnityEngine;

namespace Mimi.Actor.Graphic.Core
{ 
    public interface IGraphicAsset
    {
    }

    public interface IGraphicAsset<T> : IGraphicAsset
    {
        public void SetAssetGraphic(T sprite);
        public T GetAssetGraphic();
    }
}