using Mimi.Prototypes.Pooling;
using Mimi.Prototypes.UI;
using Mimi.ServiceLocators;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RatingDialog : SimpleAnimModalDialog
{
    [SerializeField] private Button submitBut;
    [SerializeField] private Button laterBut;
    [SerializeField] private TextMeshProUGUI tmTittle;

    [SerializeField] private UnityEvent onClickYesEvent;
    [SerializeField] private UnityEvent onClickNoEvent;

    private void Awake()
    {
        this.submitBut.onClick.AddListener(AcceptRating);
        this.laterBut.onClick.AddListener(OnClickNo);
    }

    private void AcceptRating()
    {
        RateMarket();
        this.onClickYesEvent.Invoke();
        Hide();
    }

    public void SetYesCallback(UnityAction action)
    {
        this.onClickYesEvent.AddListener(action);
    }

    public void SetNoCallback(UnityAction action)
    {
        this.onClickNoEvent.AddListener(action);
    }

    private void OnClickNo()
    {
        PlayerPrefs.SetInt("isAppRated", 1);
        this.onClickNoEvent.Invoke();
        Hide();
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
        this.onClickYesEvent.RemoveAllListeners();
        this.onClickNoEvent.RemoveAllListeners();
        base.Hide();
    }

    protected override void OnHideComplete()
    {
        ServiceLocator.Global.Get<IPoolService>().Despawn(gameObject);
    }
}