using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;

public class LifeView : BaseView
{
    [SerializeField] private TMP_Text lifeCount;
    [SerializeField] private TMP_Text timeRemaining;

    public void SetLifeCount(string count)
    {
        this.lifeCount.text = count;
    }

    public void SetTimeRemaining(string timeRemaining)
    {
        this.timeRemaining.text = timeRemaining;
    }
}