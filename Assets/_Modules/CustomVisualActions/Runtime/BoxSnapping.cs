using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class BoxSnapping : MonoBehaviour
{
    [SerializeField] private GameObject[] snappingObjects;
    [SerializeField] private bool startingObjectState;
    [SerializeField] private SnappingEffect[] snappingEffects;
    [SerializeField] private SnappingBoxCheckingEffect[] checkingEffects;
    private int count;

    private void Awake()
    {
        foreach (GameObject snappingObj in this.snappingObjects)
        {
            snappingObj.SetActive(this.startingObjectState);
        }
    }

    public async UniTask ShowCheckingEffect()
    {
        if (this.count >= this.snappingEffects.Length)
        {
            return;
        }

        GameObject snappingObject = this.snappingObjects[this.count];

        for (int i = 0; i < this.checkingEffects.Length; i++)
        {
            SnappingBoxCheckingEffect effect = this.checkingEffects[i];
            await effect.ShowEffect(snappingObject.transform);
        }
    }

    public async UniTask HideCheckingEffect()
    {
        if (this.count >= this.snappingEffects.Length)
        {
            return;
        }

        GameObject snappingObject = this.snappingObjects[this.count];

        for (int i = 0; i < this.checkingEffects.Length; i++)
        {
            SnappingBoxCheckingEffect effect = this.checkingEffects[i];
            await effect.HideEffect(snappingObject.transform);
        }
    }

    public async UniTask SnapNextObject()
    {
        if (this.snappingObjects.Length <= 0 || this.count >= this.snappingEffects.Length)
        {
            return;
        }

        GameObject snappingObject = this.snappingObjects[this.count];
        this.count++;
        snappingObject.SetActive(true);

        for (int i = 0; i < this.snappingEffects.Length; i++)
        {
            SnappingEffect effect = this.snappingEffects[i];
            await effect.RunEffect(snappingObject.transform);
        }

        foreach (SnappingBoxCheckingEffect checkingEffect in this.checkingEffects)
        {
            await checkingEffect.HideEffect(snappingObject.transform);
        }
    }

    [Button]
    private void GetAllSnappingEffects()
    {
        this.snappingEffects = GetComponentsInChildren<SnappingEffect>();
    }

    [Button]
    private void GetAllCheckingEffects()
    {
        this.checkingEffects = GetComponentsInChildren<SnappingBoxCheckingEffect>();
    }
}