using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Dragging;
using UnityEngine;
using VisualActions.Areas;

namespace Mimi.VisualActions.Generate.Editor
{
    public class InsideArea2DConditionGenerator : BaseGenerateObject<InsideArea2D>,IVisualConditionGenerator
    {
        private BaseArea area;
        private Transform checkTransform;
        public override string PrefabAddress => "/Prefabs/VisualActions/Conditions/InsideArea2D.prefab";

        public InsideArea2DConditionGenerator(BaseArea area, Transform checkTransform)
        {
            this.area = area;
            this.checkTransform = checkTransform;
        }
        public VisualCondition Generate()
        {
            var instance = GeneratePrefab();
            instance.SetField("checkTransform", checkTransform, AccessModifier.Private);
            instance.SetField("targetArea", area, AccessModifier.Private);
            return instance;
        }
    }
}