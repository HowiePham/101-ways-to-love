using System;
using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberBasedLifeView : BaseView
{
    [SerializeField] private TMP_Text lifeCount;
    [SerializeField] private TMP_Text timeRemaining;
    [SerializeField] private GameObject addLifeIconObject;
    [SerializeField] private Button lifeButton;

    public Action OnLifeButtonClicked;

    public override void Initialize()
    {
        base.Initialize();
        if (this.lifeButton != null)
        {
            this.lifeButton.onClick.AddListener(() => OnLifeButtonClicked?.Invoke());
        }
    }

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