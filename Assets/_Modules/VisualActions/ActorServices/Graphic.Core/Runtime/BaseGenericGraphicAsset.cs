namespace Mimi.Actor.Graphic.Core
{
    public abstract class BaseGenericGraphicAsset<T> : IGraphicAsset<T>
    {
        private T assetGraphic;

        public void SetAssetGraphic(T assetGraphic)
        {
            this.assetGraphic = assetGraphic;
        }

        public T GetAssetGraphic()
        {
            return this.assetGraphic;
        }
    }
}