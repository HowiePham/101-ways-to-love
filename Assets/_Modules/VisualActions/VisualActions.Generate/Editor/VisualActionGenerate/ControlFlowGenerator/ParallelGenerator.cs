using System.Collections.Generic;
using Mimi.VisualActions.ControlFlow;

namespace Mimi.VisualActions.Generate.Editor
{
    public class ParallelGenerator : BaseCompositeGenerateObject<VisualParallel, IVisualActionGenerator>, IVisualActionGenerator
    {
        public override string PrefabAddress => "/Prefabs/VisualActions/ControlFlow/VisualParallel.prefab";
        public VisualAction Generate()
        {
            var instance = GeneratePrefab();
            VisualAction[] actions = new VisualAction[generators.Count];
            for (int i = 0; i < generators.Count; i++)
            {
                var actionInstance = generators[i].Generate();
                actionInstance.transform.SetParent(instance.transform);
                actions[i] = actionInstance;
            }
            return instance;
        }
    }
}