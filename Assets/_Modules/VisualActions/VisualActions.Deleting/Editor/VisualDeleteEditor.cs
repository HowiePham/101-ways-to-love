using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Editor;
using Mimi.ScratchCardAsset;
using UnityEditor;
using UnityEngine;
using Mimi.VisualActions.Deleting;

namespace Mimi.VisualActions.Deleting.Editor
{
    public static class VisualDeleteEditor
    {
        [MenuItem("GameObject/Visual Actions/Mechanics/Deleting/Delete Smooth", false, -10000)]
        private static void DeletePattern(MenuCommand menuCommand)
        {
            GameObject go = VisualActionBuilder.CreateGameObject(nameof(VisualSmoothDelete), menuCommand);
            var delete = go.AddComponent<VisualSmoothDelete>();
            var scratchCard = go.AddComponent<ScratchCard>();
            var eraseProgress = go.AddComponent<EraseProgress>();
            eraseProgress.Card = scratchCard;

            delete.SetField("scratchCard", scratchCard, AccessModifier.Private);
            delete.SetField("eraseProgress", eraseProgress, AccessModifier.Private);

            GameObject deletedGo = VisualActionBuilder.CreateGameObject("DeletedGraphic", menuCommand);
            deletedGo.transform.SetParent(go.transform);

            var maskedRenderer = deletedGo.AddComponent<SpriteRenderer>();
            delete.SetField("maskSpriteRenderer", maskedRenderer, AccessModifier.Private);
            delete.SetField("maskShader", Shader.Find("ScratchCard/Mask"), AccessModifier.Private);
            delete.SetField("brushShader", Shader.Find("ScratchCard/Brush"), AccessModifier.Private);
            delete.SetField("maskProgressShader", Shader.Find("ScratchCard/MaskProgress"), AccessModifier.Private);
            delete.SetField("maskProgressCutOffShader", Shader.Find("ScratchCard/MaskProgressCutOff"),
                AccessModifier.Private);

            scratchCard.Surface = maskedRenderer.transform;

            string[] guids = AssetDatabase.FindAssets("Eraser_Mask t:texture2D");

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!assetPath.Contains("Deleting")) continue;
                var brushTexture = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                delete.SetField("brushMaskTexture", brushTexture, AccessModifier.Private);
            }

            guids = AssetDatabase.FindAssets("ScratchSurface t:material");

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!assetPath.Contains("Deleting")) continue;
                var scratchSurfaceMat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                delete.SetField("material", scratchSurfaceMat, AccessModifier.Both);
            }
        }
    }
}