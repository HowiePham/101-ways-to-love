using Lean.Touch;
using ScratchCardAsset;
using UnityEngine;

public class EnableSelectedScratchCard : MonoBehaviour
{
    [SerializeField] private GameObject itemHolder;
    [SerializeField] private ScratchCard currentDelete;

    private ScratchCard[] scratchCards;
    private bool isFingerDown;
    
    private void Start()
    {
        this.scratchCards = this.itemHolder.GetComponentsInChildren<ScratchCard>();
        foreach (var item in this.scratchCards)
        {
            item.enabled = false;
        }

        LeanTouch.OnFingerDown += OnFingerDownHandler;
        LeanTouch.OnFingerUpdate += OnFingerUpdateHandler;
        LeanTouch.OnFingerUp += OnFingerUpHandler;
    }

    private void OnDisable()
    {
        LeanTouch.OnFingerDown -= OnFingerDownHandler;
        LeanTouch.OnFingerUpdate -= OnFingerUpdateHandler;
        LeanTouch.OnFingerUp -= OnFingerUpHandler;
    }
    
    private void OnFingerUpHandler(LeanFinger finger)
    {
        this.isFingerDown = false;
        if (this.currentDelete != null)
        {
            this.currentDelete.enabled = false;
            this.currentDelete = null;
        }
    }
    
    private void OnFingerUpdateHandler(LeanFinger finger)
    {
        if (!this.isFingerDown) return;
        var worldFingerPos = Camera.main.ScreenToWorldPoint(finger.ScreenPosition);
        if (this.currentDelete == null)
        {
            var hit = Physics2D.Raycast(worldFingerPos, transform.TransformDirection(Vector3.forward), 5f);
            if (hit.collider == null) return;
            hit.collider.gameObject.GetComponent<ScratchCard>().enabled = true;
            this.currentDelete = hit.collider.gameObject.GetComponent<ScratchCard>();
        }
    }

    private void OnFingerDownHandler(LeanFinger finger)
    {
        this.isFingerDown = true;
    }
}
