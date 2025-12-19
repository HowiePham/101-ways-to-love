using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mimi.VisualActions.DragHold
{
    public class FixedWorldRotation : MonoBehaviour
    {
        private Vector3 target;

        private void OnEnable()
        {
            target = transform.eulerAngles;
            
        }

        private void Update()
        {
            transform.eulerAngles = target;
        }
    }
}
