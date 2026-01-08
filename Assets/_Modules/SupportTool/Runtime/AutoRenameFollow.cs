using UnityEngine;

[ExecuteInEditMode]
public class AutoRenameFollow : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private string prefix;
    [SerializeField] private string removeString;
    private string cachedTargetName;

    private void LateUpdate()
    {
        if (Application.isPlaying || this.target == null)
        {
            return;
        }

        if (this.cachedTargetName != this.target.name)
        {
            UpdateName();
        }
    }

    private void UpdateName()
    {
        string targetName = this.target.name;
        this.cachedTargetName = targetName;
        string suffix = targetName;

        if (!string.IsNullOrEmpty(this.removeString))
        {
            suffix = targetName.Replace(this.removeString, "");
        }

        this.gameObject.name = $"{this.prefix}{suffix}";
    }
}