using Mimi.Actor.Movement.Transform;
using Sirenix.OdinInspector;
using UnityEngine;

public class TestMovementDemo : MonoBehaviour
{
    [SerializeField] private MonoTransformMovement testMovement;

    [SerializeField] private Vector3 position;

    [Button]
    public void Test()
    {
        Debug.Log(testMovement.GetPosition());
        testMovement.SetPosition(position);
        Debug.Log(testMovement.GetPosition());
    }
}
