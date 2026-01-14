using UnityEngine;

public class LoopBackground : MonoBehaviour
{
    [SerializeField] private GameObject[] bgPanels;
    [SerializeField] private Transform startPos;
    [SerializeField] private Transform target;
    [SerializeField] private Canvas parentCanvas; 
    
    private void FixedUpdate()
    {
        if(!this.parentCanvas.enabled) return;
        
        foreach (var item in this.bgPanels)
        {
            item.transform.position = Vector3.MoveTowards(item.transform.position, this.target.position, 1 * Time.deltaTime);
            var distance = Vector2.Distance(item.transform.position, target.position);
            if (distance < 0.1f)
            {
                item.transform.position = this.startPos.position;
            }
        }
    }
}
