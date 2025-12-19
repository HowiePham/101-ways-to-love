using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.Extensions;
using Mimi.Reflection.Extensions;
using UnityEngine;

namespace Mimi.VisualActions.Generate.Editor
{
    public class SnapToTargetExtensionGenerator : BaseGenerateObject<SnapToTargetExtensions>, IVisualDragExtensionGenerator
    {
        private Transform target;
        public override string PrefabAddress => "/Prefabs/VisualActions/VisualDrag/Extensions/SnapTarget.prefab";

        public SnapToTargetExtensionGenerator(Transform targetTf)
        {
            this.target = targetTf;
        }
        public MonoDragExtension Generate()
        {
            var instance = GeneratePrefab();
            instance.SetField("targetPosition", target, AccessModifier.Private);
            return instance;
        }
    }
}