using DG.Tweening;
using TMPro;
using UnityEngine;

public class StepPoint : MonoBehaviour
{
    [SerializeField] private GameObject doneImageObject;
    [SerializeField] private TMP_Text stepText;
    [SerializeField] private bool isChecked;
    public bool IsChecked => this.isChecked;

    public void SetStepText(string stepText)
    {
        this.stepText.text = stepText;
    }

    public void SetDoneUI(bool isDone)
    {
        this.doneImageObject.SetActive(isDone);
        this.isChecked = isDone;

        if (isDone)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(this.doneImageObject.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.4f));
            sequence.Append(this.doneImageObject.transform.DOScale(new Vector3(1f, 1f, 1f), 0.4f));
            sequence.Play();
        }
    }
}