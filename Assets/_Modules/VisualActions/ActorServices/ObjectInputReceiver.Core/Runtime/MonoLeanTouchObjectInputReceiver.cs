using Lean.Common;
using Lean.Touch;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.Actor.ObjectInputReceiver.Core
{
    [RequireComponent(typeof(LeanSelectableByFinger))]
    public class MonoLeanTouchObjectInputReceiver : BaseMonoObjectInputReceiver
    {
        [Required]
        [SerializeField] private LeanSelectable leanSelectable;
        [SerializeField] private Camera renderCamera;

       

        protected override IObjectInputReceiver CreateObjectInputReceiver()
        {
            if (renderCamera == null)
            {
                renderCamera = Camera.main;
            }
            return new LeanTouchObjectInputReceiver(leanSelectable, renderCamera);
        }
    }
}