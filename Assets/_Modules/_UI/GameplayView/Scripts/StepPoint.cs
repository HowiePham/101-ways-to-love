using UnityEngine;

public class StepPoint : MonoBehaviour
{
    [SerializeField] private GameObject doneImageObject;
    [SerializeField] private bool isChecked;

    public bool IsChecked => this.isChecked;

    public void SetDoneUI(bool isDone)
    {
        this.doneImageObject.SetActive(isDone);
        this.isChecked = isDone;
    }
}