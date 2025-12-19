using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Areas.Editor;
using Mimi.VisualActions.Editor;
using UnityEditor;
using VisualActions.Areas;

namespace Mimi.VisualActions.Swiping.Editor
{
    public static class VisualSwipeEditor
    {
        [MenuItem("GameObject/Visual Actions/Mechanics/Swipe", false, -10000)]
        private static void CreateSwipe(MenuCommand command)
        {
            var swipe = VisualActionBuilder.CreateActionGameObject<VisualSwipe>("Swipe", command);
            BoxArea boxArea = VisualAreaEditor.NewBoxArea(new MenuCommand(swipe.transform.parent.gameObject));
            boxArea.transform.SetParent(swipe.transform);
            swipe.SetField("startingArea", boxArea, AccessModifier.Private);
        }
    }
}