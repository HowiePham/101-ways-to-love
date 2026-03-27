using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;

public class NumberBasedLifeView : BaseView
{
    [SerializeField] private TMP_Text lifeCount;
    [SerializeField] private TMP_Text timeRemaining;
    [SerializeField] private GameObject addLifeIconObject;

    public void SetLifeCount(int count)
    {
        this.lifeCount.text = count.ToString();
    }

    public void SetTimeRemaining(string timeRemaining)
    {
        this.timeRemaining.text = timeRemaining;
    }

    public void SetAddLifeIconActive(bool isActive)
    {
        this.addLifeIconObject.SetActive(isActive);
    }
}