using UnityEngine;
using VisualActions.Areas;

[ExecuteInEditMode]
public class InteractingBox : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private bool followTarget;
    private BoxArea BoxArea => GetComponent<BoxArea>();

    public bool FollowTarget
    {
        get => this.followTarget;
        set => this.followTarget = value;
    }

    public Transform Target
    {
        get => this.target;
        set => this.target = value;
    }

    private void LateUpdate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        UpdatePosition();
    }

    private void Start()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (this.Target == null || !this.FollowTarget)
        {
            return;
        }

        this.transform.position = this.Target.position;

        var targetBoxArea = this.Target.GetComponent<BoxArea>();
        if (BoxArea != null && targetBoxArea != null)
        {
            BoxArea.SetSize(targetBoxArea.Size);
        }
    }
}