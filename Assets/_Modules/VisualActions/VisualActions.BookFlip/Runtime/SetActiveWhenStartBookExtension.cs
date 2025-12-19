using System.Collections.Generic;
using UnityEngine;

namespace Mimi.VisualActions.BookFlip
{
    public class SetActiveWhenStartBookExtension : MonoBookFlipExtension
    {
        [SerializeField] private List<GameObject> activeObjects;
        [SerializeField] private List<GameObject> inactiveObjects;
        public override void Start()
        {
            
        }

        public override void FlipStart()
        {
            foreach (var activeObject in activeObjects)
            {
                activeObject.SetActive(true);
            }

            foreach (var inactiveObject in inactiveObjects)
            {
                inactiveObject.SetActive(false);
            }
        }

        public override void FlipCompleted()
        {
            
        }

        public override void PageRelease()
        {
            foreach (var activeObject in activeObjects)
            {
                activeObject.SetActive(false);
            }

            foreach (var inactiveObject in inactiveObjects)
            {
                inactiveObject.SetActive(true);
            }
        }
    }
}