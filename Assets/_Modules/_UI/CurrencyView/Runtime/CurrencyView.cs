using System;
using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyView : BaseView
{
    [SerializeField] private TMP_Text currencyQuantityText;
    [SerializeField] private Button addCurrencyButton;

    public Action OnAddCurrencyClicked;

    public override void Initialize()
    {
        base.Initialize();

        this.addCurrencyButton.onClick.AddListener(() => OnAddCurrencyClicked.Invoke());
    }

    public void SetCurrencyQuantityText(string quantity)
    {
        this.currencyQuantityText.text = quantity;
    }
}