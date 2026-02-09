using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class BoxSnapping : MonoBehaviour
{
    [SerializeField] private GameObject[] snappingObjects;
    [SerializeField] private bool startingObjectState;
    [SerializeField] private SnappingEffect[] snappingEffects;
    private int count;

    private void Start()
    {
        foreach (GameObject snappingObj in this.snappingObjects)
        {
            snappingObj.SetActive(this.startingObjectState);
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
    }

    [Button]
    private void GetAllSnappingEffects()
    {
        this.snappingEffects = GetComponentsInChildren<SnappingEffect>();
    }
}