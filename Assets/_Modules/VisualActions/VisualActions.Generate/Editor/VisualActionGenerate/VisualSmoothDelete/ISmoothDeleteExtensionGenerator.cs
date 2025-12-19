using Mimi.VisualActions.Deleting;

namespace Mimi.VisualActions.Generate.Editor
{
    public interface ISmoothDeleteExtensionGenerator
    {
        public MonoSmoothDeleteExtension Generate(VisualSmoothDelete smoothDelete);
    }
}