using UnityEngine;
using UnityEngine.UI;

public class CheatView : MonoBehaviour
{
    [SerializeField] private Button nextButton;
    private bool isToggle;

    private void Awake()
    {
        this.nextButton.onClick.AddListener(ToggleFps);
    }

    private void ToggleFps()
    {
        if (!this.isToggle)
        {
            this.isToggle = true;
            Application.targetFrameRate = 60;
            return;
        }

        Application.targetFrameRate = 30;
        this.isToggle = false;
    }
}