using Cysharp.Threading.Tasks;
using UnityEngine;

public class SwapObjectEffect : SnappingBoxCheckingEffect
{
    [SerializeField] private GameObject originalObject;
    [SerializeField] private GameObject swappedObject;

    public override async UniTask ShowEffect(Transform target)
    {
        this.originalObject.gameObject.SetActive(false);
        this.swappedObject.gameObject.SetActive(true);
    }

    public override async UniTask HideEffect(Transform target)
    {
        this.originalObject.gameObject.SetActive(true);
        this.swappedObject.gameObject.SetActive(false);
    }
}