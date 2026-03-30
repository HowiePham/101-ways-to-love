using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ChapterProgressBlock : MonoBehaviour
{
    [SerializeField] private Image blockImage;
    [SerializeField] private Image borderImage;
    [SerializeField] private RectTransform maskRect;
    [SerializeField] private Sprite firstBlockSprite;
    [SerializeField] private Sprite midBlockSprite;
    [SerializeField] private Sprite lastBlockSprite;
    [SerializeField] private Color completedColor = new Color(0.3f, 0.85f, 0.4f, 1f);
    [SerializeField] private Color uncompletedColor = new Color(0.75f, 0.75f, 0.75f, 0.5f);

    private float fullWidth;

    public void SetBlockType(BlockType blockType)
    {
        this.blockImage.sprite = blockType switch
        {
            BlockType.First => this.firstBlockSprite,
            BlockType.Last => this.lastBlockSprite,
            _ => this.midBlockSprite
        };
        this.borderImage.gameObject.SetActive(blockType != BlockType.Last);
    }

    public void SetCompleted(bool completed)
    {
        this.blockImage.color = completed ? this.completedColor : this.uncompletedColor;
    }

    public void ResetFill()
    {
        CacheFullWidth();
        this.maskRect.sizeDelta = new Vector2(this.fullWidth, this.maskRect.sizeDelta.y);
    }

    public void HideFill()
    {
        CacheFullWidth();
        this.maskRect.sizeDelta = new Vector2(0f, this.maskRect.sizeDelta.y);
    }

    public Sequence PlayCurrentBlockAnimation()
    {
        CacheFullWidth();
        this.maskRect.sizeDelta = new Vector2(0f, this.maskRect.sizeDelta.y);

        return DOTween.Sequence()
            .Append(DOTween.To(
                () => this.maskRect.sizeDelta.x,
                x => this.maskRect.sizeDelta = new Vector2(x, this.maskRect.sizeDelta.y),
                this.fullWidth,
                0.9f
            ).SetEase(Ease.OutCubic));
    }

    private void CacheFullWidth()
    {
        if (this.fullWidth <= 0f)
        {
            this.fullWidth = ((RectTransform)transform).rect.width;
        }
    }
}