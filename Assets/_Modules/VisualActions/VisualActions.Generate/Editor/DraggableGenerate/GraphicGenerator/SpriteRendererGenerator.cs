using Mimi.Actor.Graphic.Core;
using Mimi.Actor.Graphic.SpriteRenderer;

namespace Mimi.VisualActions.Generate.Editor.GraphicGenerator
{
    public class SpriteRendererGenerator : BaseGenerateObject<MonoSpriteRendererGraphic>, IGraphicGenerator
    {
        public override string PrefabAddress => "/Prefabs/Draggable/Graphics/SpriteGraphic.prefab";
        public BaseMonoGraphic Generate()
        {
            return GeneratePrefab();
        }
    }
}