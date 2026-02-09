using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class SnappingEffect : MonoBehaviour
{
    [SerializeField] protected bool waitEffect = true;
    public abstract UniTask RunEffect(Transform target);
}