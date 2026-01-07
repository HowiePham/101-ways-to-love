using UnityEngine;

public class AutoRenameFollow : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private string prefix;

    private void OnValidate()
    {
        this.gameObject.name = $"{this.prefix}_{this.target.name}";
    }
}