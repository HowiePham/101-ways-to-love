using Mimi.Reflection.Extensions;
using Mimi.VisualActions.BookFlip;

namespace Mimi.VisualActions.Generate.Editor
{
    public class BookFlipGenerator : BaseGenerateObject<BookFlipAction>, IVisualActionGenerator
    {
        private IBookFlipExtensionGenerator extensionGenerator;
        public override string PrefabAddress => "/Prefabs/VisualActions/BookFlip/BookFlip.prefab";

        public BookFlipGenerator(IBookFlipExtensionGenerator extensionGenerator)
        {
            this.extensionGenerator = extensionGenerator;
        }
        public VisualAction Generate()
        {
            var instance = GeneratePrefab();
            if (extensionGenerator != null)
            {
                var extension = extensionGenerator.Generate();
                extension.transform.parent = instance.transform;
                instance.SetField("extension", extension, AccessModifier.Private);
            }
            return instance;
        }
    }
}