using Mimi.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.Graphic.LineRenderers
{
    [RequireComponent(typeof(LineRenderer))]
    public class UnityLineRendererMonoWrapper : MonoLineRenderer
    {
        [SerializeField] private LineRenderer unityLineRenderer;
        [SerializeField, MinValue(0.1f)] private float width = 1f;
        [SerializeField] private Color color = Color.black;
        [SerializeField, SortingLayerName] private string sortingLayerName;
        [SerializeField] private int sortingOrder = 1;

        protected override ILineRenderer CreateRenderer()
        {
            var lineRenderer = new UnityLineRenderer(this.unityLineRenderer);
            lineRenderer.SetMaterial(new Material(Shader.Find("Sprites/Default")));
            lineRenderer.SetColor(this.color);
            lineRenderer.SetWidth(this.width);
            lineRenderer.SetWorldSpace(true);
            lineRenderer.SetSortingLayer(this.sortingLayerName);
            lineRenderer.SetSortingOrder(this.sortingOrder);
            return lineRenderer;
        }
    }
}