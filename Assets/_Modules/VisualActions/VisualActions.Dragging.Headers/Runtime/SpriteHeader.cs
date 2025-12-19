using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mimi.VisualActions.Dragging.Headers
{
    public class SpriteHeader : BaseGraphicHeader
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public override Bounds GraphicBounds => this.spriteRenderer.bounds;

        public override void SetActive(bool active)
        {
            this.spriteRenderer.enabled = active;
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/Visual Flow/Graphic Header/Sprite", false, -10000)]
        public static void CreateSpriteHeader(MenuCommand menuCommand)
        {
            var go = new GameObject("SpriteHeader");
            go.transform.position = Vector3.zero;
            var spriteHeader = go.AddComponent<SpriteHeader>();
            var graphicGo = new GameObject("Graphic");
            graphicGo.transform.SetParent(go.transform);
            graphicGo.transform.localPosition = Vector3.zero;
            var renderer = graphicGo.AddComponent<SpriteRenderer>();
            spriteHeader.GetType().GetField("spriteRenderer", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(spriteHeader, renderer);
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, go.GetInstanceID().ToString());
            Selection.activeObject = go;
        }
#endif
    }
}