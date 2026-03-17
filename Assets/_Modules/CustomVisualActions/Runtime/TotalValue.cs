using TMPro;
using UnityEngine;

public class TotalValue : MonoBehaviour
{
    [SerializeField] private float totalValue;
    [SerializeField] private TMP_Text totalValueText;
    public float Total => this.totalValue;

    public void SetTotalValue(float totalValue)
    {
        this.totalValue = totalValue;

        if (this.totalValueText != null)
        {
            this.totalValueText.text = totalValue.ToString();
        }
    }
}