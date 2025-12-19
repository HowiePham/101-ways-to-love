using Mimi.VisualActions.Deleting;

namespace Mimi.VisualActions.Generate.Editor.SmoothDeleteExtension
{
    public class PlaySoundDeleteExtensionEdit : BaseSmoothDeleteExtensionEdit<PlaySoundOnDeletingExtension>
    {
        protected override PlaySoundOnDeletingExtension CreateNewExtension()
        {
            return new PlaySoundSmoothDeleteExtensionGenerator().Generate(visualSmoothDelete) as PlaySoundOnDeletingExtension;
        }
    }
}