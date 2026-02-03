using DG.Tweening;
using TMPro;
using UnityEngine;

public class StepPoint : MonoBehaviour
{
    [SerializeField] private GameObject doneImageObject;
    [SerializeField] private GameObject bridgeImageObject;
    [SerializeField] private TMP_Text stepText;
    [SerializeField] private bool isChecked;
    [SerializeField] private Vector3 maxScale;
    public bool IsChecked => this.isChecked;

    public void SetStepText(string stepText)
    {
        this.stepText.text = stepText;
    }

    public void SetActiveBridgeImage(bool active)
    {
        this.bridgeImageObject.SetActive(active);
    }
    
    public void SetDoneUI(bool isDone)
    {
        this.doneImageObject.SetActive(isDone);
        this.isChecked = isDone;

        if (isDone)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(this.doneImageObject.transform.DOScale(this.maxScale, 0.4f));
            sequence.Join(this.stepText.transform.DOScale(this.maxScale, 0.4f));
            sequence.Append(this.doneImageObject.transform.DOScale(Vector3.one, 0.4f));
            sequence.Join(this.stepText.transform.DOScale(Vector3.one, 0.4f));
            sequence.Play();
        }
    }
}