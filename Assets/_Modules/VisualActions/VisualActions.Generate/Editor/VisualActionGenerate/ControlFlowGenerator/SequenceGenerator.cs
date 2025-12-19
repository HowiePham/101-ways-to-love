using System.Collections.Generic;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.ControlFlow;

namespace Mimi.VisualActions.Generate.Editor
{
    public class SequenceGenerator : BaseCompositeGenerateObject<VisualSequence, IVisualActionGenerator>, IVisualActionGenerator
    {
        public override string PrefabAddress => "/Prefabs/VisualActions/ControlFlow/VisualSequence.prefab";
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