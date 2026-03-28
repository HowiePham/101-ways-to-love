using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChapterProgressBar : MonoBehaviour
{
    [SerializeField] private ChapterProgressBlock blockPrefab;
    [SerializeField] private Transform blockContainer;

    private List<ChapterProgressBlock> blocks;

    public void Initialize()
    {
        this.blocks = new List<ChapterProgressBlock>();
    }

    public void SetProgress(int completedCount, int totalLevels)
    {
        EnsureBlockCount(totalLevels);

        for (int i = 0; i < this.blocks.Count; i++)
        {
            if (i < totalLevels)
            {
                this.blocks[i].gameObject.SetActive(true);

                BlockType blockType;
                if (i == 0)
                    blockType = BlockType.First;
                else if (i == totalLevels - 1)
                    blockType = BlockType.Last;
                else
                    blockType = BlockType.Mid;

                this.blocks[i].SetBlockType(blockType);
                this.blocks[i].SetCompleted(i < completedCount);
            }
            else
            {
                this.blocks[i].gameObject.SetActive(false);
            }
        }
    }

    private void EnsureBlockCount(int requiredCount)
    {
        if (this.blocks.Count < requiredCount)
        {
            int diff = requiredCount - this.blocks.Count;

            for (int i = 0; i < diff; i++)
            {
                ChapterProgressBlock block = Instantiate(this.blockPrefab, this.blockContainer);
                this.blocks.Add(block);
            }
        }
    }
}
