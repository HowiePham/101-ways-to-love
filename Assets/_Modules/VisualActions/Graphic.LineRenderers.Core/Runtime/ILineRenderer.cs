using System;
using UnityEngine;

namespace Mimi.Graphic.LineRenderers
{
    public interface ILineRenderer : IDisposable
    {
        void AddPoint(Vector2 point);
        void ClearPoints();
        ILineRenderer SetWidth(float width);
        ILineRenderer SetWorldSpace(bool useWorldSpace);
        ILineRenderer SetColor(Color color);
        ILineRenderer SetMaterial(Material material);
        ILineRenderer SetSortingLayer(string layerName);
        ILineRenderer SetSortingOrder(int sortingOrder);
    }
}