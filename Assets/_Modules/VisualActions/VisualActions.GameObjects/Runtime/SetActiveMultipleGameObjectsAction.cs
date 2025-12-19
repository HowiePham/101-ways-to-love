using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using Mimi.VisualActions.Attribute;
using UnityEngine;

namespace VisualActions.VisualActions.GameObjects.Runtime
{
    public class SetActiveMultipleGameObjectsAction : VisualAction
    {
        [MainInput]
        [SerializeField] private GameObject[] gameObjects;
        [SerializeField] private bool status;
        protected override UniTask OnExecuting(CancellationToken cancellationToken)
        {
            for (int i = 0; i < gameObjects.Length; i++)
            {
                gameObjects[i].SetActive(status);
            }
            return UniTask.CompletedTask;
        }
    }
}