using Mimi.Graphic.LineRenderers;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Editor;
using UnityEditor;
using UnityEngine;
using VisualActions.Drawing;

namespace Mimi.VisualActions.Drawing.Editor
{
    public static class VisualDrawEditor
    {
        [MenuItem("GameObject/Visual Actions/Mechanics/Drawing/Draw Line", false, -10000)]
        private static void CreateDrawLineRenderer(MenuCommand menuCommand)
        {
            var drawPattern =
                VisualActionBuilder.CreateActionGameObject<VisualDrawLine>("DrawLine", menuCommand);
            var unityLineRenderer = drawPattern.gameObject.AddComponent<UnityLineRendererMonoWrapper>();
            unityLineRenderer.SetField("unityLineRenderer", drawPattern.GetComponent<LineRenderer>(),
                AccessModifier.Private);
            drawPattern.SetField("lineRenderer", unityLineRenderer, AccessModifier.Private);
            var pointRoot = new GameObject("DrawPoints");
            var p1 = new GameObject("p1");
            var p2 = new GameObject("p2");
            pointRoot.transform.SetParent(drawPattern.transform);
            p1.transform.SetParent(pointRoot.transform);
            p2.transform.SetParent(pointRoot.transform);
            drawPattern.SetField("pointRoot", pointRoot.transform, AccessModifier.Private);
        }
    }
}