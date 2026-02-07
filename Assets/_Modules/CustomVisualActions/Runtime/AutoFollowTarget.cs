using System;
using UnityEngine;

public class AutoFollowTarget : MonoBehaviour
{
    [SerializeField] private Transform target;

    private void Update()
    {
        this.transform.position = this.target.position;
    }
}