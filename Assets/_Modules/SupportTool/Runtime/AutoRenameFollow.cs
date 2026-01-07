using UnityEngine;

public class AutoRenameFollow : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private string prefix;
    [SerializeField] private string removeString;
    
    private void OnValidate()
    {
        if (this.target == null)
        {
            return;
        }

        string targetName = this.target.name;
        string suffix = targetName;

        if (!string.IsNullOrEmpty(this.removeString))
        {
            suffix = targetName.Replace(this.removeString, "");
        }

        this.gameObject.name = $"{this.prefix}{suffix}";
    }
}