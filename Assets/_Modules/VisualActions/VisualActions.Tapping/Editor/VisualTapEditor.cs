using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Areas.Editor;
using Mimi.VisualActions.Editor;
using UnityEditor;
using UnityEngine;
using VisualActions.Areas;

namespace Mimi.VisualActions.Tapping.Editor
{
    public static class VisualTapEditor
    {
        [MenuItem("GameObject/Visual Actions/Mechanics/Tapping/Tap Area", false, -10000)]
        private static void CreateTapArea(MenuCommand menuCommand)
        {
            var tapArea = VisualActionBuilder.CreateActionGameObject<TapArea>("TapArea", menuCommand);
            BaseArea boxArea = VisualAreaEditor.NewBoxArea(menuCommand);
            boxArea.transform.SetParent(tapArea.transform);
            boxArea.transform.localPosition = Vector3.zero;
            tapArea.SetField("target", boxArea, AccessModifier.Private);
        }

        [MenuItem("GameObject/Visual Actions/Mechanics/Tapping/Tap Screen", false, -10000)]
        private static void CreateTapScreen(MenuCommand menuCommand)
        {
            VisualActionBuilder.CreateActionGameObject<TapScreen>("TapScreen", menuCommand);
        }
    }
}