using System.Collections.Generic;
using System.Linq;
using Lean.Touch;
using Sirenix.OdinInspector;
using UnityEngine;

public class ItemSlider : MonoBehaviour
{
    [Header("Slider Settings")] [SerializeField]
    private float itemSpacing = 2f;

    [SerializeField] private float slideSpeed = 5f;
    [SerializeField] private float snapSpeed = 10f;
    [SerializeField] private bool enableSnapping = true;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Interaction Settings")] [Tooltip("Khoảng cách tối thiểu để xác định hướng kéo")] [SerializeField]
    private float directionDetectionThreshold = 0.1f;

    [Tooltip("Góc tối thiểu để coi là kéo ngang (độ). 0° = ngang hoàn toàn, 45° = chéo 45°")] [Range(0f, 45f)] [SerializeField]
    private float horizontalAngleThreshold = 30f;

    [Header("Item Settings")] [SerializeField]
    private Transform itemContainer;

    [SerializeField] private List<SliderItemSelector> sliderItemSelectors;

    private Vector3 dragStartPosition;
    private float currentOffset = 0f;
    private float targetOffset = 0f;
    [SerializeField] private bool isDragging = false;
    private int centerItemIndex = 0;

    private LeanFinger currentFinger;
    private bool isItemBeingDragged = false;

    // Cache các khoảng cách thực tế của từng item
    private List<float> itemPositions = new List<float>();

    // Tracking cho interaction
    private Vector3 initialTouchPosition;
    private SliderItemSelector touchedItem;
    private bool hasDecidedInteraction = false; // Đã quyết định là slide hay drag item

    void Start()
    {
        if (this.itemContainer == null)
        {
            this.itemContainer = this.transform;
        }

        CalculateItemPositions();
        UpdateItemPositions();
    }

    void Update()
    {
        if (!isDragging && this.enableSnapping)
        {
            this.currentOffset = Mathf.Lerp(this.currentOffset, this.targetOffset, Time.deltaTime * this.snapSpeed);
        }

        UpdateItemPositions();
        UpdateItemSelection();
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

    private void CalculateItemPositions()
    {
        itemPositions.Clear();

        if (this.sliderItemSelectors.Count == 0) return;

        float currentPos = 0f;

        for (int i = 0; i < this.sliderItemSelectors.Count; i++)
        {
            itemPositions.Add(currentPos);

            var item = this.sliderItemSelectors[i];
            if (item != null)
            {
                float halfWidth = item.Bounds.size.x / 2f;

                float nextHalfWidth = 0f;
                if (i + 1 < this.sliderItemSelectors.Count)
                {
                    var nextItem = this.sliderItemSelectors[i + 1];
                    if (nextItem != null)
                    {
                        nextHalfWidth = nextItem.Bounds.size.x / 2f;
                    }
                }

                currentPos += halfWidth + this.itemSpacing + nextHalfWidth;
            }
        }
    }

    private void OnFingerDown(LeanFinger finger)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(finger.ScreenPosition);
        worldPos.z = 0f;

        // Reset tracking
        initialTouchPosition = worldPos;
        hasDecidedInteraction = false;

        this.touchedItem = GetTouchedItem(worldPos);

        if (IsInSliderArea(worldPos))
        {
            currentFinger = finger;
            dragStartPosition = worldPos;
        }
    }

    private void OnFingerUpdate(LeanFinger finger)
    {
        if (finger != currentFinger) return;

        // Nếu item đã được xác nhận đang drag, không làm gì
        if (isItemBeingDragged && hasDecidedInteraction) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(finger.ScreenPosition);
        worldPos.z = 0f;

        Vector3 dragDelta = worldPos - initialTouchPosition;
        float dragDistance = dragDelta.magnitude;

        // Chưa quyết định là slide hay drag item
        if (!hasDecidedInteraction)
        {
            // Chỉ quyết định khi đã di chuyển đủ xa
            if (dragDistance > directionDetectionThreshold)
            {
                // Tính góc kéo (so với trục X - ngang)
                float angle = Mathf.Abs(Mathf.Atan2(Mathf.Abs(dragDelta.y), Mathf.Abs(dragDelta.x)) * Mathf.Rad2Deg);

                // Góc < threshold -> Kéo NGANG -> SLIDE
                if (angle < horizontalAngleThreshold)
                {
                    hasDecidedInteraction = true;
                    isDragging = true;
                    isItemBeingDragged = false;
                    touchedItem = null; // Hủy khả năng drag item

                    Debug.Log($"Decided: SLIDE (angle: {angle}°)");
                }
                // Góc >= threshold -> Kéo DỌC -> DRAG ITEM
                else if (touchedItem != null)
                {
                    hasDecidedInteraction = true;
                    isItemBeingDragged = true;
                    isDragging = false;
                    this.touchedItem.StartDragging(finger);

                    Debug.Log($"Decided: DRAG ITEM (angle: {angle}°)");
                    return;
                }
                else
                {
                    // Kéo dọc nhưng không có item -> không làm gì
                    hasDecidedInteraction = true;
                    return;
                }
            }
            else
            {
                // Chưa đủ điều kiện để quyết định, chờ thêm
                return;
            }
        }

        // Đã quyết định là SLIDE -> thực hiện slide
        if (isDragging)
        {
            float delta = worldPos.x - dragStartPosition.x;
            this.currentOffset += delta * slideSpeed;

            if (itemPositions.Count > 0)
            {
                float maxOffset = itemPositions[itemPositions.Count - 1];
                this.currentOffset = Mathf.Clamp(this.currentOffset, -maxOffset, 0f);
            }

            dragStartPosition = worldPos;
        }
    }

    private void OnFingerUp(LeanFinger finger)
    {
        if (finger != currentFinger) return;

        // Nếu chưa quyết định gì (tap nhanh) -> snap đến item gần nhất
        if (!hasDecidedInteraction && touchedItem != null)
        {
            SnapToItem(touchedItem);
        }

        if (isDragging)
        {
            isDragging = false;

            if (this.enableSnapping)
            {
                SnapToNearestItem();
            }
        }

        // Reset
        isItemBeingDragged = false;
        currentFinger = null;
        touchedItem = null;
        hasDecidedInteraction = false;
    }

    private SliderItemSelector GetTouchedItem(Vector3 worldPos)
    {
        foreach (var item in this.sliderItemSelectors)
        {
            if (item == null) continue;

            if (item.Bounds.Contains(worldPos))
            {
                return item;
            }
        }

        return null;
    }

    private void UpdateItemPositions()
    {
        if (itemPositions.Count != this.sliderItemSelectors.Count)
        {
            CalculateItemPositions();
        }

        for (int i = 0; i < this.sliderItemSelectors.Count; i++)
        {
            if (this.sliderItemSelectors[i] == null) continue;

            float targetX = itemPositions[i] + this.currentOffset;
            Vector3 pos = this.sliderItemSelectors[i].transform.localPosition;
            pos.x = targetX;
            this.sliderItemSelectors[i].transform.localPosition = pos;
        }
    }

    private void UpdateItemSelection()
    {
        if (this.sliderItemSelectors.Count == 0) return;

        float closestDist = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < this.sliderItemSelectors.Count; i++)
        {
            if (this.sliderItemSelectors[i] == null) continue;

            float dist = Mathf.Abs(this.sliderItemSelectors[i].transform.localPosition.x);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }

        centerItemIndex = closestIndex;
    }

    private void SnapToNearestItem()
    {
        if (this.sliderItemSelectors.Count == 0 || itemPositions.Count == 0) return;

        this.targetOffset = -itemPositions[centerItemIndex];
        float maxOffset = itemPositions[itemPositions.Count - 1];
        this.targetOffset = Mathf.Clamp(this.targetOffset, -maxOffset, 0f);
    }

    private void SnapToItem(SliderItemSelector item)
    {
        int index = this.sliderItemSelectors.IndexOf(item);
        if (index >= 0 && index < itemPositions.Count)
        {
            this.targetOffset = -itemPositions[index];
            float maxOffset = itemPositions[itemPositions.Count - 1];
            this.targetOffset = Mathf.Clamp(this.targetOffset, -maxOffset, 0f);
        }
    }

    private bool IsInSliderArea(Vector3 worldPos)
    {
        if (spriteRenderer == null) return false;
        return spriteRenderer.bounds.Contains(worldPos);
    }

    public bool IsSliderDragging()
    {
        return isDragging;
    }

    public bool HasDecidedToSlide()
    {
        return hasDecidedInteraction && isDragging;
    }

    public void NotifyItemDragStart(LeanFinger finger)
    {
        isItemBeingDragged = true;
        isDragging = false;
        currentFinger = finger;
    }

    public void NotifyItemDragEnd()
    {
        isItemBeingDragged = false;
        currentFinger = null;
    }

    public void RemoveItemFromList(SliderItemSelector sliderItemSelector)
    {
        this.sliderItemSelectors.Remove(sliderItemSelector);
        CalculateItemPositions();
    }

#if UNITY_EDITOR
    [Button]
    private void GetAllItemSlider()
    {
        this.sliderItemSelectors = GetComponentsInChildren<SliderItemSelector>().ToList();
        CalculateItemPositions();
    }

    [Button]
    private void RecalculatePositions()
    {
        CalculateItemPositions();
        Debug.Log($"Recalculated {itemPositions.Count} item positions");
        for (int i = 0; i < itemPositions.Count; i++)
        {
            Debug.Log($"Item {i}: Position = {itemPositions[i]}");
            this.sliderItemSelectors[i].transform.localPosition = new Vector3(itemPositions[i], 0, 0);
        }
    }
#endif
}