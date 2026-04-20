using System;
using UnityEngine;

public class AutoFollowTarget : MonoBehaviour
{
    [SerializeField] private Transform target;

    private void Update()
    {
        if (this.target == null)
        {
            return;
        }

        this.transform.position = this.target.position;
    }
}