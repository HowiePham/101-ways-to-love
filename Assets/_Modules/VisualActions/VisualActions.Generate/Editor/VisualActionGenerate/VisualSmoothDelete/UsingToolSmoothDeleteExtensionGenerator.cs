using Mimi.Interactions.Dragging;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Deleting;

namespace Mimi.VisualActions.Generate.Editor
{
    public class UsingToolSmoothDeleteExtensionGenerator : BaseGenerateObject<UsingToolSmoothDeleteExtension>, ISmoothDeleteExtensionGenerator
    {
        private BaseDraggable tool;
        public override string PrefabAddress => "/Prefabs/VisualActions/Delete/UsingToolExtension.prefab";

        public UsingToolSmoothDeleteExtensionGenerator(BaseDraggable tool)
        {
            this.tool = tool;
        }
        public MonoSmoothDeleteExtension Generate(VisualSmoothDelete smoothDelete)
        {
            var instance = GeneratePrefab();
            instance.SetField("tool", tool, AccessModifier.Private);
            instance.SetField("visualSmoothDelete", smoothDelete, AccessModifier.Private);
            return instance;
        }
    }
}