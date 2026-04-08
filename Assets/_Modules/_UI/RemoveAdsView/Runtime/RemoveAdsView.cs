using System;
using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RemoveAdsView : BaseView
{
    [SerializeField] private Button removeAdsButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text priceText;

    public Action OnCloseClicked;
    public Action OnBuyRemoveAdsClicked;

    public override void Initialize()
    {
        base.Initialize();

        this.removeAdsButton.onClick.AddListener(() => this.OnBuyRemoveAdsClicked?.Invoke());
        this.closeButton.onClick.AddListener(() => this.OnCloseClicked?.Invoke());
    }

    public void SetPriceText(string text)
    {
        this.priceText.text = text;
    }
}