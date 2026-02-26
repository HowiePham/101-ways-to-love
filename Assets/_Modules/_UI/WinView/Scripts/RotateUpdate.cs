using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateUpdate : MonoBehaviour
{
    [SerializeField] private bool isRotate;
    [SerializeField] private float speedRotate;

    [SerializeField] private Transform target;

    // Update is called once per frame
    void Update()
    {
        if (isRotate)
        {
            target.Rotate(Vector3.forward, speedRotate * Time.deltaTime);
        }
    }

    public void SetIsRotate(bool isRotate)
    {
        this.isRotate = isRotate;
    }
}