using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Deleting;
using Mimi.ScratchCardAsset;
using UnityEngine;

namespace Mimi.VisualActions.Generate.Editor
{
    public class VisualSmoothDeleteGenerator : BaseGenerateObject<Deleting.VisualSmoothDelete>, IVisualActionGenerator
    {
        private ISmoothDeleteExtensionGenerator extensionGenerator;
        private SpriteRenderer targetRenderer;
        private ScratchCard.ScratchMode modeDelete;
        public override string PrefabAddress => "/Prefabs/VisualActions/Delete/VisualSmoothDelete.prefab";

        public VisualSmoothDeleteGenerator(ISmoothDeleteExtensionGenerator extensionGenerator,
            SpriteRenderer targetRenderer, ScratchCard.ScratchMode mode = ScratchCard.ScratchMode.Erase)
        {
            this.extensionGenerator = extensionGenerator;
            this.targetRenderer = targetRenderer;
            this.modeDelete = mode;
        }
        public VisualAction Generate()
        {
            var instance = GeneratePrefab();
            if (extensionGenerator != null)
            {
                var extensionInstance = extensionGenerator.Generate(instance);
                extensionInstance.transform.SetParent(instance.transform);
                instance.SetField("smoothDeleteExtension", extensionInstance, AccessModifier.Private);
            }

            SpriteRenderer graphicGameObject = targetRenderer;
            if (modeDelete == ScratchCard.ScratchMode.Restore)
            {
                graphicGameObject = GameObject.Instantiate(targetRenderer.gameObject,instance.transform).GetComponent<SpriteRenderer>();
                graphicGameObject.gameObject.SetActive(false);
            }

            float valuePercentage = modeDelete == ScratchCard.ScratchMode.Erase ? 0.85f : 0.2f;
            instance.targetDeletePercentage = valuePercentage;
            if (graphicGameObject != null)
            {
                instance.SetField("maskSpriteRenderer", graphicGameObject, AccessModifier.Private);
            }
            instance.SetField("scratchMode", modeDelete, AccessModifier.Private);
            return instance;
        }
    }
}