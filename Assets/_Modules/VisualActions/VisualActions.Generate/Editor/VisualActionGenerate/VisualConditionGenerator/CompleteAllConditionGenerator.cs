using System.Collections.Generic;
using Mimi.Reflection.Extensions;

namespace Mimi.VisualActions.Generate.Editor
{
    public class CompleteAllConditionGenerator : BaseCompositeGenerateObject<CompleteAllCondition,IVisualConditionGenerator>, IVisualConditionGenerator
    {
        public override string PrefabAddress => "/Prefabs/VisualActions/Conditions/CompleteAllCondition.prefab";
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