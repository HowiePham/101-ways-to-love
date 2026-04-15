using System;
using DG.Tweening;
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

    public void PlayLifeGainedEffect()
    {
        this.lifeCount.transform.DOKill();
        this.lifeCount.DOKill();

        this.lifeCount.transform.DOPunchScale(Vector3.one * 0.5f, 0.7f, vibrato: 6, elasticity: 0.6f);

        var colorSeq = DOTween.Sequence();
        colorSeq.Append(this.lifeCount.DOColor(Color.green, 0.15f));
        colorSeq.Append(this.lifeCount.DOColor(Color.white, 0.55f));
        colorSeq.Play();
    }

    public void PlayLifeLostEffect()
    {
        this.lifeCount.transform.DOKill();
        this.lifeCount.DOKill();

        this.lifeCount.transform.DOPunchScale(Vector3.one * 0.5f, 0.7f, vibrato: 6, elasticity: 0.6f);

        var colorSeq = DOTween.Sequence();
        colorSeq.Append(this.lifeCount.DOColor(Color.red, 0.15f));
        colorSeq.Append(this.lifeCount.DOColor(Color.white, 0.55f));
        colorSeq.Play();
    }
}