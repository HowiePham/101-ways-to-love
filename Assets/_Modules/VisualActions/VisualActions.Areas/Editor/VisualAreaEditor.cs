using System.Reflection;
using Mimi.VisualActions.Editor;
using UnityEditor;
using UnityEngine;
using VisualActions.Areas;

namespace Mimi.VisualActions.Areas.Editor
{
    public static class VisualAreaEditor
    {
        [MenuItem("GameObject/Visual Actions/Areas/Box", false, -10000)]
        public static void CreateBoxArea(MenuCommand menuCommand)
        {
            var area = VisualActionBuilder.CreateGameObject("BoxArea", menuCommand).AddComponent<BoxArea>();
            var collider = area.gameObject.GetComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;
            area.GetType().GetField("boxCollider", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(area, collider);
        }

        public static BoxArea NewBoxArea(MenuCommand menuCommand)
        {
            var area = VisualActionBuilder.CreateGameObject("BoxArea", menuCommand).AddComponent<BoxArea>();
            var collider = area.GetComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;
            area.GetType().GetField("boxCollider", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(area, collider);
            return area;
        }

        [MenuItem("GameObject/Visual Actions/Areas/Composite Area", false, -10000)]
        private static void CreateCompositeArea(MenuCommand menuCommand)
        {
            VisualActionBuilder.CreateGameObject("CompositeArea", menuCommand).AddComponent<CompositeArea>();
        }

        [MenuItem("GameObject/Visual Actions/Areas/Polygon Area", false, -10000)]
        private static void CreatePolygonArea(MenuCommand menuCommand)
        {
            var polygonArea = VisualActionBuilder.CreateGameObject("PolygonArea", menuCommand)
                .AddComponent<PolygonArea>();
            var collider = polygonArea.GetComponent<PolygonCollider2D>();
            collider.isTrigger = true;
            polygonArea.GetType().GetField("polygonCollider", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(polygonArea, collider);
        }

        [MenuItem("GameObject/Visual Actions/Areas/Check Correct Area", false, -10000)]
        private static void CreateCheckCorrectArea(MenuCommand menuCommand)
        {
            VisualActionBuilder.CreateActionGameObject<CheckCorrectArea>("CheckCorrectArea", menuCommand);
        }
    }
}