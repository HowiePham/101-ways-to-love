using System;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Deleting;

namespace Mimi.VisualActions.Generate.Editor
{
    public class CompositeSmoothDeleteExtensionGenerator : BaseCompositeGenerateObject<CompositeSmoothDeleteExtension, ISmoothDeleteExtensionGenerator>, ISmoothDeleteExtensionGenerator
    {
        
        public override string PrefabAddress => "/Prefabs/VisualActions/Delete/CompositeSmoothDeleteExtension.prefab";
        public MonoSmoothDeleteExtension Generate(VisualSmoothDelete smoothDelete)
        {
            var instanceComposition = smoothDelete.GetComponentInChildren<CompositeSmoothDeleteExtension>();
            CompositeSmoothDeleteExtension instance = null;
            if (instanceComposition == null)
            {
                instance = GeneratePrefab();
            }
            else
            {
                instance = instanceComposition;
            }

            var currentExtension =
                instance.GetFieldValue<MonoSmoothDeleteExtension[]>("extensions", AccessModifier.Private);
            if (currentExtension == null)
            {
                currentExtension = Array.Empty<MonoSmoothDeleteExtension>();
            }
            MonoSmoothDeleteExtension[] extensions = new MonoSmoothDeleteExtension[currentExtension.Length + generators.Count];
            for (int i = 0; i < currentExtension.Length; i++)
            {
                extensions[i] = currentExtension[i];
            }
            for (int i = 0; i < generators.Count; i++)
            {
                extensions[i+currentExtension.Length] = generators[i].Generate(smoothDelete);
                extensions[i+currentExtension.Length].transform.SetParent(instance.transform);
            }
            instance.SetField("extensions", extensions, AccessModifier.Private);
            return instance;
        }

        
    }
}