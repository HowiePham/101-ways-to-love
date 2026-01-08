using UnityEngine;

[ExecuteInEditMode]
public class MovingWay : MonoBehaviour
{
    [SerializeField] private Transform[] points;
    public Transform[] Points => this.points;

    private void LateUpdate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (this.points.Length != this.transform.childCount)
        {
            UpdatePath();
        }
    }

    private void UpdatePath()
    {
        this.points = new Transform[this.transform.childCount];
        for (int i = 0; i < this.transform.childCount; i++)
        {
            Transform point = this.transform.GetChild(i);
            this.points[i] = point;
        }
    }
}