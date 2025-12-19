using Mimi.VisualActions.Deleting;

namespace Mimi.VisualActions.Generate.Editor.SmoothDeleteExtension
{
    public class UsingToolSmoothDeleteEdit : BaseSmoothDeleteExtensionEdit<UsingToolSmoothDeleteExtension>
    {
        protected override UsingToolSmoothDeleteExtension CreateNewExtension()
        {
            return new UsingToolSmoothDeleteExtensionGenerator(baseDraggable).Generate(visualSmoothDelete) as UsingToolSmoothDeleteExtension;
        }
    }
}