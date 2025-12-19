namespace Mimi.Actor.Graphic.Core
{
    public interface IGraphic
    {
        public int SortingOrder { get;}
        public string SortingLayerName { get;  }
        public IGraphicAsset CurrentGraphicAsset { get; }
        public void SetAssetGraphic(IGraphicAsset graphic);
        public void SetSortingOrder(int sortingOrder);
        public void SetSortingLayerName(string sortingLayerName);
        public void Show();
        public void Hide();
        public void SetAlpha(float alpha);
    }
}