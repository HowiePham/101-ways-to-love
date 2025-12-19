using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using Mimi.VisualActions.Data;
using UnityEngine;

namespace VisualActions.VisualActions.GameObjects.Runtime
{
    public class SetActiveMultipleFieldGameObjectsAction : VisualAction
    {
        [SerializeField] private GameObjectField[] fieldGameObjects;
        [SerializeField] private bool status;
        protected override UniTask OnExecuting(CancellationToken cancellationToken)
        {
            foreach (var fieldGameObject in fieldGameObjects)
            {
                fieldGameObject.GetValue().SetActive(status);
            }
            return UniTask.CompletedTask;
        }
    }
}