using Cysharp.Threading.Tasks;
using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.VisualActions.Attribute;
using Mimi.VisualActions.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.VisualActions.Interactions.Draggable.Extensions
{
    [TypeInfoBox("Return Draggable to start position when release finger")]
    public class ReturnOnDeselect : MonoDraggableExtension
    {
        [HideInBehaviourEditor]
        [SerializeField] private Transform startPosition;
        [HideInBehaviourEditor]
        [SerializeField] private Vector3Field offsetField;
        [SerializeField] private float delaySeconds;

        public override void Init(BaseDraggable draggable)
        {
            base.Init(draggable);
            if (offsetField == null)
            {
                draggable.TryGetComponent<Vector3Field>(out offsetField);
                if (offsetField == null)
                {
                    offsetField = gameObject.AddComponent<Vector3Field>();
                }
            }
        }

        public override void StartDrag()
        {
            
        }

        public override void Drag()
        {
            
        }

        public override void EndDrag()
        {
            ReturnPos(this.BaseDraggable);
        }

        async UniTask ReturnPos(BaseDraggable draggable)
        {
            await UniTask.Delay(Mathf.RoundToInt(delaySeconds * 1000));
            draggable.SetPosition(startPosition.position-offsetField.GetValue());
        }
    }
}