using Mimi.VisualActions.Editor;
using UnityEditor;

namespace Mimi.VisualActions.Spines.Editor
{
    public static class VisualSpineEditor
    {
        [MenuItem("GameObject/Visual Actions/Spine/Mix Skin Additive", false, -10000)]
        private static void CreateMixSkinAdditive(MenuCommand menuCommand)
        {
            VisualActionBuilder.CreateGameObject("MixSpineSkin", menuCommand).AddComponent<MixSpineSkinAdditive>();
        }

        [MenuItem("GameObject/Visual Actions/Spine/Wait Spine Animation", false, -10000)]
        private static void CreateWaitSpineAnim(MenuCommand menuCommand)
        {
            VisualActionBuilder.CreateActionGameObject<WaitSpineAnim>(nameof(WaitSpineAnim), menuCommand);
        }

        [MenuItem("GameObject/Visual Actions/Spine/Play Spine Animation", false, -10000)]
        private static void CreatePlaySpineAnim(MenuCommand menuCommand)
        {
            VisualActionBuilder.CreateActionGameObject<PlaySpineAnim>(nameof(PlaySpineAnim), menuCommand);
        }

        [MenuItem("GameObject/Visual Actions/Spine/Set Time Scale", false, -10000)]
        private static void CreateSetTimeScale(MenuCommand menuCommand)
        {
            VisualActionBuilder.CreateActionGameObject<SetSpineTimeScale>("SetTimeScale", menuCommand);
        }

        [MenuItem("GameObject/Visual Actions/Spine/Remove Skin", false, -10000)]
        private static void CreateRemoveSkin(MenuCommand menuCommand)
        {
            VisualActionBuilder.CreateActionGameObject<RemoveSpineSkin>("RemoveSkin", menuCommand);
        }
    }
}