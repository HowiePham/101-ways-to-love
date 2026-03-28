using UnityEngine;
using UnityEngine.UI;

public class ChapterProgressBlock : MonoBehaviour
{
    [SerializeField] private Image blockImage;
    [SerializeField] private Image borderImage;
    [SerializeField] private Sprite firstBlockSprite;
    [SerializeField] private Sprite midBlockSprite;
    [SerializeField] private Sprite lastBlockSprite;
    [SerializeField] private Color completedColor = new Color(0.3f, 0.85f, 0.4f, 1f);
    [SerializeField] private Color uncompletedColor = new Color(0.75f, 0.75f, 0.75f, 0.5f);

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
}

public enum BlockType
{
    First,
    Mid,
    Last
}
