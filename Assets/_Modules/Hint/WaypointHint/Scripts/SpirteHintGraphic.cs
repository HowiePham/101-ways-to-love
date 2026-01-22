
using UnityEngine;

public class SpirteHintGraphic : MonoBehaviour
{
    public void SetActive(bool active)
    {
        if (gameObject != null)
        {
            this.gameObject.SetActive(active);
        }
    }

    public void SetColor(Color color)
    {
        this.gameObject.SetActive(false);
    }
}
