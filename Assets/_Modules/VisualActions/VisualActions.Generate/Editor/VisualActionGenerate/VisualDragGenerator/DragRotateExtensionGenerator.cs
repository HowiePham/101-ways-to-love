using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.Extensions;
using Mimi.Reflection.Extensions;
using UnityEngine;
using VisualActions.Areas;

namespace Mimi.VisualActions.Generate.Editor
{
    public class DragRotateExtensionGenerator : BaseGenerateObject<DragRotateExtenstion>, IVisualDragExtensionGenerator
    {
        private Transform rotateTransform;
        private BaseArea baseArea;
        public override string PrefabAddress => "/Prefabs/VisualActions/VisualDrag/Extensions/DragRotate.prefab";

        public DragRotateExtensionGenerator(Transform rotateTransform, BaseArea baseArea)
        {
            this.rotateTransform = rotateTransform;
            this.baseArea = baseArea;
        }
        public MonoDragExtension Generate()
        {
            var instance = GeneratePrefab();
            instance.SetField("rotatedTransform", rotateTransform, AccessModifier.Private);
            instance.SetField("selectArea", baseArea, AccessModifier.Private);
            return instance;
        }
    }
}