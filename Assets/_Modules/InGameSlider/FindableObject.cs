using UnityEngine;

// Script đơn giản để đánh dấu object có thể tìm được
public class FindableObject : MonoBehaviour
{
    [Header("Object Info")] public string objectName = "Mystery Object";
    public Color highlightColor = Color.yellow;

    [Header("Custom Reveal")] public bool useCustomRevealRadius = false;
    public float customRevealRadius = 3f;

    [Header("Events")] public UnityEngine.Events.UnityEvent onFoundEvent;

    private bool isFound = false;
    private SpriteRenderer spriteRenderer;

    public void OnFound()
    {
        if (isFound) return;

        isFound = true;
        Debug.Log($"Found: {objectName}");

        // Gọi event nếu có
        onFoundEvent?.Invoke();
    }


    void OnDrawGizmos()
    {
        // Vẽ outline trong Scene view để dễ nhìn
        Gizmos.color = isFound ? Color.green : highlightColor;

        if (useCustomRevealRadius)
        {
            // Vẽ vòng tròn với custom radius
            Gizmos.DrawWireSphere(transform.position, customRevealRadius);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}