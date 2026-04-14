using System;
using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RatingDialog : BaseModalDialog
{
    [SerializeField] private Button submitBut;
    [SerializeField] private Button laterBut;
    [SerializeField] private TextMeshProUGUI tmTittle;

    public override void Show()
    {
        base.Show();
        this.submitBut.onClick.AddListener(AcceptRating);
        this.laterBut.onClick.AddListener(Hide);
    }

    private void AcceptRating()
    {
        Hide();
        RateMarket();
    }

    private void RateMarket()
    {
#if UNITY_ANDROID
        Application.OpenURL("market://details?id=" + Application.identifier);

#elif UNITY_IOS
            UnityEngine.iOS.Device.RequestStoreReview();
#endif
        PlayerPrefs.SetInt("isAppRated", 1);
    }

    public override void Hide()
    {
        PlayerPrefs.SetInt("isAppRated", 1);
        base.Hide();
    }
}