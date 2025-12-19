using System;
using System.Reflection;
using Mimi.Actor.Graphic.Core;
using Mimi.Actor.Movement.Core;
using Mimi.Actor.Movement.Transform;
using Mimi.Interactions.Dragging.DraggableExtensions;
using UnityEngine;

namespace Mimi.Interactions.Dragging
{
    public abstract class BaseDraggable : MonoBehaviour
    {
        [SerializeField] private float smoothTime = 0.2f;
        [SerializeField] private float speedScalar = 2f;
        [SerializeField] private BasePositionProcessor positionProcessor;
        [SerializeField] private MonoDraggableExtension draggableExtension;
        [SerializeField] private BaseMonoGraphic graphic;
        [SerializeField] private MonoMovement movement;
        public bool IsSelected { private set; get; }
        public bool IsInteractable { private set; get; }
        public Vector3 Position => movement.GetPosition();
        public Transform Transform { private set; get; }
        
        public IGraphic Graphic => graphic;
        public IMovement Movement => movement;
        
        
        private Vector3 dragVelocity;
        private Vector3 dest;

        private void Awake()
        {
            if (movement == null)
            {
                var transformMovement = gameObject.AddComponent<MonoTransformMovement>();
                FieldInfo field = typeof(MonoTransformMovement)
                    .GetField("tf", BindingFlags.NonPublic | BindingFlags.Instance);
                field.SetValue(transformMovement, transform);
                movement = transformMovement;
                Debug.LogError($"Missing movement component at {gameObject.name}");
            }

            if (graphic == null)
            {
                Debug.LogError($"Missing graphic component at {gameObject.name}");
            }
            if (this.positionProcessor == null)
            {
                this.positionProcessor = gameObject.AddComponent<NullPositionProcessor>();
            }
            
            Transform = transform;
            this.dest = Transform.position;
            this.IsInteractable = true;
            this.positionProcessor.Initialize(Transform);
            if (this.draggableExtension == null)
            {
                this.draggableExtension = gameObject.GetComponent<MonoDraggableExtension>();
                if (this.draggableExtension == null)
                {
                    this.draggableExtension = gameObject.AddComponent<NullMonoDraggableExtension>();
                }
            }
            draggableExtension.Init(this);
            OnInit();
        }

        public T GetExtension<T>() where T : MonoDraggableExtension
        {
            var targetType = typeof(T);
            if (this.draggableExtension is T)
            {
                return (T)this.draggableExtension;
            }
            if (this.draggableExtension is IParentDraggableExtension parentDraggable)
            {
                return (T)parentDraggable.GetExtension<T>();
            }
            return null;
        }
        private void OnEnable()
        {
            OnActivated();
        }

        private void OnDisable()
        {
            OnDeactivated();
        }

        public void SetInteractable(bool isInteractable)
        {
            var previousValue = IsInteractable;
            IsInteractable = isInteractable;
            if (isInteractable == previousValue) return;
            if (isInteractable)
            {
                OnActivated();
            }
            else
            {
                OnDeactivated();
            }
        }
        protected abstract void OnInit();
        protected abstract void OnActivated();
        protected abstract void OnDeactivated();
        public void SetPosition(Vector3 targetPos)
        {
            this.dest = this.positionProcessor.Process(targetPos);
        }

        protected void OnStartDrag()
        {
            IsSelected = true;
            this.draggableExtension.StartDrag();
        }

        protected void OnDrag()
        {
            this.draggableExtension.Drag();
        }

        protected void OnEndDrag()
        {
            IsSelected = false;
            this.draggableExtension.EndDrag();
        }
        

    private void LateUpdate()
    {
        if (!IsInteractable) return;
        movement.SetPosition(Vector3.SmoothDamp(movement.GetPosition(), this.dest, ref this.dragVelocity,
            this.smoothTime,
            Mathf.Infinity, Time.deltaTime * this.speedScalar));
            ;
        }
    }
}