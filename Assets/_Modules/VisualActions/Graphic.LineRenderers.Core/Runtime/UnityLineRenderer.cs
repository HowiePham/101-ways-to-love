using System.Collections.Generic;
using UnityEngine;

namespace Mimi.Graphic.LineRenderers
{
    public class UnityLineRenderer : ILineRenderer
    {
        private readonly List<Vector2> points;
        private readonly LineRenderer lineRenderer;

        public UnityLineRenderer(LineRenderer lineRenderer)
        {
            this.points = new List<Vector2>(20);
            this.lineRenderer = lineRenderer;
            lineRenderer.positionCount = 0;

            if (this.lineRenderer.material == null)
            {
                this.lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }
        }

        public void AddPoint(Vector2 point)
        {
            this.points.Add(point);
            this.lineRenderer.positionCount = this.points.Count;
            this.lineRenderer.SetPosition(this.lineRenderer.positionCount - 1, point);
        }

        public void ClearPoints()
        {
            this.points.Clear();
            this.lineRenderer.positionCount = 0;
        }

        public ILineRenderer SetWidth(float width)
        {
            this.lineRenderer.startWidth = width;
            this.lineRenderer.endWidth = width;
            return this;
        }

        public ILineRenderer SetWorldSpace(bool useWorldSpace)
        {
            this.lineRenderer.useWorldSpace = true;
            return this;
        }

        public ILineRenderer SetColor(Color color)
        {
            this.lineRenderer.startColor = color;
            this.lineRenderer.endColor = color;
            return this;
        }

        public ILineRenderer SetMaterial(Material material)
        {
            this.lineRenderer.material = material;
            return this;
        }

        public ILineRenderer SetSortingLayer(string layerName)
        {
            this.lineRenderer.sortingLayerID = SortingLayer.NameToID(layerName);
            return this;
        }

        public ILineRenderer SetSortingOrder(int sortingOrder)
        {
            this.lineRenderer.sortingOrder = sortingOrder;
            return this;
        }

        public void Dispose()
        {
            ClearPoints();
        }
    }
}