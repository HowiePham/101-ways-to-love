using Mimi.VisualActions.Editor;
using UnityEditor;

namespace Mimi.VisualActions.Shaking.Editor
{
    public static class VisualShakingEditor
    {
        [MenuItem("GameObject/Visual Actions/Mechanics/Shake Device", false, -10000)]
        private static void CreateDragToWorld(MenuCommand menuCommand)
        {
            VisualActionBuilder.CreateActionGameObject<VisualShakeDevice>("ShakeDevice", menuCommand);
        }
    }
}