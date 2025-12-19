using Mimi.VisualActions.Editor;
using UnityEditor;
using UnityEngine;

namespace Mimi.VisualActions.ControlFlow.Editor
{
    public static class VisualControlFlowEditor
    {
        private static readonly GUIStyle NameStyle = new()
        {
            normal = new GUIStyleState() {textColor = Color.yellow},
            fontStyle = FontStyle.Normal,
        };

        [MenuItem("GameObject/Visual Actions/Control Flow/Parallel", false, -10000)]
        private static void Parallel(MenuCommand menuCommand)
        {
            VisualActionBuilder.CreateActionGameObject<VisualParallel>("Parallel", menuCommand, NameStyle);
        }

        [MenuItem("GameObject/Visual Actions/Control Flow/Sequence", false, -10000)]
        private static void Sequence(MenuCommand menuCommand)
        {
            VisualActionBuilder.CreateActionGameObject<VisualSequence>("Sequence", menuCommand, NameStyle);
        }

        [MenuItem("GameObject/Visual Actions/Control Flow/Wait Any", false, -10000)]
        private static void WaitAny(MenuCommand menuCommand)
        {
            VisualActionBuilder.CreateActionGameObject<VisualWaitAny>("WaitAny", menuCommand, NameStyle);
        }

        [MenuItem("GameObject/Visual Actions/Control Flow/Wait Seconds", false, -10000)]
        private static void WaitSeconds(MenuCommand menuCommand)
        {
            VisualActionBuilder.CreateActionGameObject<VisualWaitSeconds>("WaitSeconds", menuCommand, NameStyle);
        }
    }
}