using System;
using Lean.Common;
using Lean.Touch;
using UnityEngine;

namespace Mimi.Actor.ObjectInputReceiver.Core
{
    public class LeanTouchObjectInputReceiver : IObjectInputReceiver
    {
        private LeanSelectable _selectable;
        private Camera camera;
        private bool isSelected;

        public event Action<Vector3> OnSelect;
        public event Action<Vector3> OnDrag;
        public event Action<Vector3> OnRelease;
        public event Action<Vector3> OnTap;

        private float timeTap = 0.5f;
        private float selectTime;
        public LeanTouchObjectInputReceiver(LeanSelectable selectable, Camera camera)
        {
            _selectable = selectable;
            this.camera = camera;
        }
        
        public void Initialize()
        {
            this._selectable.OnSelected.AddListener(OnLeanSelect);
            LeanTouch.OnFingerUp += OnLeanRelease;
            LeanTouch.OnFingerUpdate += OnLeanDrag;
        }
  
        public void Dispose()
        {
            this._selectable.OnSelected.RemoveListener(OnLeanSelect);
            LeanTouch.OnFingerUp += OnLeanRelease;
            LeanTouch.OnFingerUpdate -= OnLeanDrag;
        }
        private void OnLeanSelect(LeanSelect finger)
        {
            isSelected = true;
            selectTime = Time.time;
            OnSelect?.Invoke(camera.ScreenToWorldPoint(Input.mousePosition));
        }

        private void OnLeanDrag(LeanFinger finger)
        {
            if (!isSelected) return;
            OnDrag?.Invoke(camera.ScreenToWorldPoint(finger.ScreenPosition));
        }

        private void OnLeanRelease(LeanFinger finger)
        {
            if(!isSelected) return;
            this._selectable.Deselect();
            isSelected = false;
            var worldPos = camera.ScreenToWorldPoint(finger.ScreenPosition);
            if (Mathf.Abs(Time.time - selectTime) < timeTap)
            {
                OnTap?.Invoke(worldPos);
            }
            OnRelease?.Invoke(worldPos);
        }
        

       
    }
}