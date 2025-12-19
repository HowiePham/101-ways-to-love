using Lean.Touch;
using UnityEngine;

public class DraggableSliderItem : MonoBehaviour
{
    private ItemSlider parentSlider;
    private Vector3 offset;
    private bool isDragging = false;
    private Vector3 originalPosition;
    private LeanFinger currentFinger;
    
    [SerializeField] private bool returnToSlider = true;
    [SerializeField] private float returnSpeed = 5f;
    [SerializeField] private float dragThreshold = 0.1f;

    private Vector3 fingerDownPosition;
    private bool hasDraggedEnough = false;

    void Start()
    {
        this.parentSlider = GetComponentInParent<ItemSlider>();
        this.originalPosition = this.transform.position;
    }

    void OnEnable()
    {
        LeanTouch.OnFingerDown += OnFingerDown;
        LeanTouch.OnFingerUpdate += OnFingerUpdate;
        LeanTouch.OnFingerUp += OnFingerUp;
    }

    void OnDisable()
    {
        LeanTouch.OnFingerDown -= OnFingerDown;
        LeanTouch.OnFingerUpdate -= OnFingerUpdate;
        LeanTouch.OnFingerUp -= OnFingerUp;
    }

    void Update()
    {
        if (!this.isDragging && this.returnToSlider)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, this.originalPosition, Time.deltaTime * this.returnSpeed);
        }
        else if (!this.isDragging)
        {
            this.originalPosition = this.transform.position;
        }
    }

    private void OnFingerDown(LeanFinger finger)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(finger.ScreenPosition);
        worldPos.z = 0f;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col.OverlapPoint(worldPos))
        {
            this.currentFinger = finger;
            this.fingerDownPosition = worldPos;
            this.hasDraggedEnough = false;
            
            this.offset = this.transform.position - worldPos;
        }
    }

    private void OnFingerUpdate(LeanFinger finger)
    {
        if (finger != this.currentFinger) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(finger.ScreenPosition);
        worldPos.z = 0f;

        // Kiểm tra xem đã kéo đủ xa chưa
        if (!this.hasDraggedEnough)
        {
            float dragDistance = Vector3.Distance(worldPos, this.fingerDownPosition);
            if (dragDistance > this.dragThreshold)
            {
                this.hasDraggedEnough = true;
                this.isDragging = true;
                
                // Thông báo cho slider biết item đang được drag
                if (this.parentSlider != null)
                {
                    this.parentSlider.NotifyItemDragStart(finger);
                }
            }
            else
            {
                return; // Chưa kéo đủ xa, chưa bắt đầu drag
            }
        }

        if (this.isDragging)
        {
            // Drag item
            this.transform.position = worldPos + this.offset;
        }
    }

    private void OnFingerUp(LeanFinger finger)
    {
        if (finger != this.currentFinger) return;

        if (this.isDragging)
        {
            this.isDragging = false;
            
            // Thông báo cho slider biết đã dừng drag
            if (this.parentSlider != null)
            {
                this.parentSlider.NotifyItemDragEnd();
            }

            // Có thể thêm logic thả item ở đây
            OnItemDropped();
        }

        this.currentFinger = null;
        this.hasDraggedEnough = false;
    }

    private void OnItemDropped()
    {
        // Xử lý khi item được thả
        // Ví dụ: kiểm tra có drop vào vùng hợp lệ không
        Debug.Log($"Item {this.gameObject.name} dropped at {this.transform.position}");
    }

    // Public methods để script khác gọi
    public bool IsDragging()
    {
        return this.isDragging;
    }

    public void SetReturnToSlider(bool value)
    {
        this.returnToSlider = value;
    }
}