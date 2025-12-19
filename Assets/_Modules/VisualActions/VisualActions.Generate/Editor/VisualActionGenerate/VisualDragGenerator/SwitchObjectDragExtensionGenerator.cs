using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.Extensions;
using Mimi.Reflection.Extensions;
using UnityEngine;

namespace Mimi.VisualActions.Generate.Editor
{
    public class SwitchObjectDragExtensionGenerator : BaseGenerateObject<SwitchObjectExtension>, IVisualDragExtensionGenerator
    {
        private GameObject switchObject;

        public SwitchObjectDragExtensionGenerator(GameObject switchObject)
        {
            this.switchObject = switchObject;
        }
        public override string PrefabAddress =>
            "/Prefabs/VisualActions/VisualDrag/Extensions/SwitchObject.prefab";
        public MonoDragExtension Generate()
        {
            var instance = GeneratePrefab();
            instance.SetField("switchObject", switchObject, AccessModifier.Private);
            return instance;
        }
    }
}