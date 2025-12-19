using Mimi.VisualActions.Deleting;

namespace Mimi.VisualActions.Generate.Editor
{
    public class PlaySoundSmoothDeleteExtensionGenerator : BaseGenerateObject<PlaySoundOnDeletingExtension>, ISmoothDeleteExtensionGenerator
    {
        public override string PrefabAddress => "/Prefabs/VisualActions/Delete/PlaySoundExtension.prefab";
        public MonoSmoothDeleteExtension Generate(VisualSmoothDelete smoothDelete)
        {
            return GeneratePrefab();
        }
    }
}