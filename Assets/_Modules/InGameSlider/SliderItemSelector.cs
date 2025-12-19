using System;
using Lean.Touch;
using Mimi.VisualActions;
using UnityEngine;
using UnityEngine.Events;

public class SliderItemSelector : MonoBehaviour
{
    [Header("Events")] 
    public UnityEvent OnItemReadyToDrag;
    public UnityEvent OnItemEndDragging;

    [Header("Settings")] 
    [SerializeField] private float dragThreshold = 0.1f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private LeanSelectableByFinger draggableItem;
    [SerializeField] private LeanSelectByFinger leanSelectByFinger;
    [SerializeField] private VisualCondition visualCondition;
    
    private ItemSlider parentSlider;
    private LeanFinger currentFinger;
    private Vector3 fingerDownPosition;
    private bool hasDraggedEnough = false;
    
    public Bounds Bounds => this.spriteRenderer.bounds;

    void Start()
    {
        this.parentSlider = GetComponentInParent<ItemSlider>();
        this.leanSelectByFinger = FindAnyObjectByType<LeanSelectByFinger>();
    }

    void OnEnable()
    {
        // LeanTouch.OnFingerDown += OnFingerDown;
        LeanTouch.OnFingerUpdate += OnFingerUpdate;
        LeanTouch.OnFingerUp += OnFingerUp;
    }

    void OnDisable()
    {
        // LeanTouch.OnFingerDown -= OnFingerDown;
        LeanTouch.OnFingerUpdate -= OnFingerUpdate;
        LeanTouch.OnFingerUp -= OnFingerUp;
    }

    private void OnFingerDown(LeanFinger finger)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(finger.ScreenPosition);
        worldPos.z = 0f;

        if (Bounds.Contains(worldPos))
        {
            this.currentFinger = finger;
            this.fingerDownPosition = worldPos;
            this.hasDraggedEnough = false;
        }
    }

    public void StartDragging(LeanFinger finger)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(finger.ScreenPosition);
        worldPos.z = 0f;

        if (Bounds.Contains(worldPos))
        {
            this.currentFinger = finger;
            this.fingerDownPosition = worldPos;
            this.hasDraggedEnough = false;
        }
    }

    private void OnFingerUpdate(LeanFinger finger)
    {
        if (finger != this.currentFinger) return;
        if (this.parentSlider == null) return;

        // Nếu slider đã quyết định SLIDE -> không cho drag item
        if (this.parentSlider.HasDecidedToSlide())
        {
            this.currentFinger = null;
            this.hasDraggedEnough = false;
            return;
        }

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(finger.ScreenPosition);
        worldPos.z = 0f;

        // Kiểm tra xem đã kéo đủ xa chưa
        if (!this.hasDraggedEnough)
        {
            float dragDistance = Vector3.Distance(worldPos, this.fingerDownPosition);
            if (dragDistance > this.dragThreshold)
            {
                this.hasDraggedEnough = true;

                // Thông báo cho slider biết item đang được chọn để drag
                this.parentSlider.NotifyItemDragStart(finger);

                // Trigger Unity Event - script khác sẽ xử lý drag
                this.OnItemReadyToDrag?.Invoke();
                this.draggableItem.SelectSelf(finger);
                this.leanSelectByFinger.Select(this.draggableItem);
            }
        }
    }

    private void OnFingerUp(LeanFinger finger)
    {
        if (finger != this.currentFinger)
        {
            return;
        }

        if (!this.hasDraggedEnough)
        {
            this.currentFinger = null;
            this.hasDraggedEnough = false;
            return;
        }

        if (this.parentSlider == null)
        {
            this.currentFinger = null;
            this.hasDraggedEnough = false;
            return;
        }

        this.parentSlider.NotifyItemDragEnd();
        this.draggableItem.Deselect();
        this.leanSelectByFinger.Deselect(this.draggableItem);

        if (!this.visualCondition.Validate())
        {
            this.OnItemEndDragging?.Invoke();
        }
        else
        {
            this.parentSlider.RemoveItemFromList(this);
            this.gameObject.SetActive(false);
        }

        this.currentFinger = null;
        this.hasDraggedEnough = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(this.Bounds.center, this.Bounds.size);
    }
#endif
}