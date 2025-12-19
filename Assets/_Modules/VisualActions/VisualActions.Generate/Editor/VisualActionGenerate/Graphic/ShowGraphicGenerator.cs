using Mimi.Actor.Graphic.Core;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Graphic;

namespace Mimi.VisualActions.Generate.Editor.Graphic
{
    public class ShowGraphicGenerator : BaseGenerateObject<ShowGraphicAction>, IVisualActionGenerator
    {
        private BaseMonoGraphic targetGraphic; 
        private bool isShowGraphic;
        public override string PrefabAddress => "/Prefabs/VisualActions/Graphic/ShowGraphic.prefab";

        public ShowGraphicGenerator(BaseMonoGraphic targetGraphic, bool isShowGraphic)
        {
            this.targetGraphic = targetGraphic;
            this.isShowGraphic = isShowGraphic;
        }
        public VisualAction Generate()
        {
            var instance = GeneratePrefab();
            instance.SetField("targetGraphic", targetGraphic, AccessModifier.Private);
            instance.SetField("isShowGraphic", isShowGraphic, AccessModifier.Private);
            return instance;
        }
    }
}