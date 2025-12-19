using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.Interactions.Dragging;
using Mimi.VisualActions;
using UnityEngine;

namespace VisualActions.VisualTransform
{
     public class MoveDraggableFromTo : VisualAction
       {
           [SerializeField] private BaseDraggable target;
           [SerializeField] private bool isDeActiveInitial;
           [SerializeField] private Vector3 offsetFrom;
           protected override UniTask OnInitializing()
           {
               if (isDeActiveInitial)
               {
                   target.gameObject.SetActive(false);
               }
           
               return base.OnInitializing();
           }
   
           protected override UniTask OnExecuting(CancellationToken cancellationToken)
           {
               target.transform.position += offsetFrom;
               target.gameObject.SetActive(true);
               target.SetPosition(target.transform.position - offsetFrom);
               return UniTask.CompletedTask;
           }
       }
}