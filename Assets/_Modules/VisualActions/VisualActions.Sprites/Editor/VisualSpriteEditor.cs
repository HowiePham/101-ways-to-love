using Mimi.VisualActions.Editor;
using UnityEditor;

namespace Mimi.VisualActions.Sprites.Editor
{
    public static class VisualSpriteEditor
    {
        [MenuItem("GameObject/Visual Actions/Sprites/Fade", false, -10000)]
        private static void CreateFadeSprite(MenuCommand command)
        {
            VisualActionBuilder.CreateActionGameObject<FadeSprite>("FadeSprite", command);
        }
    }
}