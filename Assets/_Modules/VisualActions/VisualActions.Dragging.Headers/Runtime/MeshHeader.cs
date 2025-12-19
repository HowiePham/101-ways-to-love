using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mimi.VisualActions.Dragging.Headers
{
    public class MeshHeader : BaseGraphicHeader
    {
        [SerializeField] private MeshRenderer meshRenderer;

        public override Bounds GraphicBounds => this.meshRenderer.bounds;

        public override void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/Visual Flow/Graphic Header/Mesh", false, -10000)]
        private static void CreateSpriteHeader(MenuCommand menuCommand)
        {
            var go = new GameObject("MeshHeader");
            go.transform.position = Vector3.zero;
            var header = go.AddComponent<MeshHeader>();
            var graphicGo = new GameObject("Graphic");
            graphicGo.transform.SetParent(go.transform);
            graphicGo.transform.localPosition = Vector3.zero;
            var renderer = graphicGo.AddComponent<MeshRenderer>();
            header.GetType().GetField("meshRenderer", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(header, renderer);
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, go.GetInstanceID().ToString());
            Selection.activeObject = go;
        }
#endif
    }
}