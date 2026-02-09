using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class SnappingBoxCheckingEffect : MonoBehaviour
{
    [SerializeField] protected bool waitEffect = true;
    public abstract UniTask ShowEffect(Transform target);
    public abstract UniTask HideEffect(Transform target);
}