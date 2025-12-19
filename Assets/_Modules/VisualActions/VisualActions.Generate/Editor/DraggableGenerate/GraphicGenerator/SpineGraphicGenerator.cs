using Mimi.Actor.Graphic.Core;
using Mimi.Actor.Graphic.Spine;

namespace Mimi.VisualActions.Generate.Editor.GraphicGenerator
{
    public class SpineGraphicGenerator : BaseGenerateObject<MonoSpineGraphic>, IGraphicGenerator
    {
        public override string PrefabAddress =>"/Prefabs/Draggable/Graphics/SpineGraphic.prefab"; 
        public BaseMonoGraphic Generate()
        {
            return GeneratePrefab();
        }
    }
}