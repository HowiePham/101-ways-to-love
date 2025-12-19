using System.Collections.Generic;
using Mimi.Reflection.Extensions;

namespace Mimi.VisualActions.Generate.Editor
{
    public class CompleteAnyConditionGenerator : BaseCompositeGenerateObject<CompleteAnyCondition, IVisualConditionGenerator>, IVisualConditionGenerator
    {
        public override string PrefabAddress => "/Prefabs/VisualActions/Conditions/CompleteAnyCondition.prefab";
        public VisualCondition Generate()
        {
            var instance = GeneratePrefab();
            var conditions = new VisualCondition[generators.Count];
            for (int i = 0; i < generators.Count; i++)
            {
                var conditionInstance = generators[i].Generate();
                conditionInstance.transform.SetParent(instance.transform);
                conditions[i] = conditionInstance;
            }
            instance.SetField("conditions", conditions, AccessModifier.Private);
            return instance;
        }
        
    }
}