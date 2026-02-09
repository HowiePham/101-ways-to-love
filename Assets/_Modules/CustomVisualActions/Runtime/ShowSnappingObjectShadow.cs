using Cysharp.Threading.Tasks;
using UnityEngine;

public class ShowSnappingObjectShadow : SnappingBoxCheckingEffect
{
    [SerializeField] private SpriteRenderer objectShadow;

    public override async UniTask ShowEffect(Transform target)
    {
        var targetRenderer = target.GetComponent<SpriteRenderer>();
        this.objectShadow.gameObject.SetActive(true);
        this.objectShadow.sprite = targetRenderer.sprite;
        this.objectShadow.sortingOrder = targetRenderer.sortingOrder - 1;
        this.objectShadow.transform.position = target.position;
    }

    public override async UniTask HideEffect(Transform target)
    {
        this.objectShadow.gameObject.SetActive(false);
    }
}