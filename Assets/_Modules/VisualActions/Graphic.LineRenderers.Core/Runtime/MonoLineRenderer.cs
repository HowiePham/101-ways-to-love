using UnityEngine;

namespace Mimi.Graphic.LineRenderers
{
    public abstract class MonoLineRenderer : MonoBehaviour, ILineRenderer
    {
        private ILineRenderer lineRenderer;

        public void Init()
        {
            this.lineRenderer = CreateRenderer();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        protected abstract ILineRenderer CreateRenderer();

        public void AddPoint(Vector2 point)
        {
            this.lineRenderer.AddPoint(point);
        }

        public void ClearPoints()
        {
            this.lineRenderer.ClearPoints();
        }

        public ILineRenderer SetWidth(float width)
        {
            this.lineRenderer.SetWidth(width);
            return this;
        }

        public ILineRenderer SetWorldSpace(bool useWorldSpace)
        {
            this.lineRenderer.SetWorldSpace(useWorldSpace);
            return this;
        }

        public ILineRenderer SetColor(Color color)
        {
            this.lineRenderer.SetColor(color);
            return this;
        }

        public ILineRenderer SetMaterial(Material material)
        {
            this.lineRenderer.SetMaterial(material);
            return this;
        }

        public ILineRenderer SetSortingLayer(string layerName)
        {
            this.lineRenderer.SetSortingLayer(layerName);
            return this;
        }

        public ILineRenderer SetSortingOrder(int sortingOrder)
        {
            this.lineRenderer.SetSortingOrder(sortingOrder);
            return this;
        }

        protected virtual void OnDispose()
        {
        }

        public void Dispose()
        {
            this.lineRenderer?.Dispose();
            OnDispose();
        }
    }
}