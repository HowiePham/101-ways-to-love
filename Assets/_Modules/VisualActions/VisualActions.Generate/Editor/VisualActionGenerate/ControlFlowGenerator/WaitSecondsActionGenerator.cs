using Mimi.VisualActions.ControlFlow;
using UnityEngine;

namespace Mimi.VisualActions.Generate.Editor
{
    public class WaitSecondsActionGenerator : BaseGenerateObject<VisualWaitSeconds>, IVisualActionGenerator
    {
        public override string PrefabAddress => "/Prefabs/VisualActions/ControlFlow/WaitSecondsAction.prefab";
        public VisualAction Generate()
        {
            return GeneratePrefab();
        }
    }
}