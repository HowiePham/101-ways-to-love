using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.Audio;
using Mimi.Services.ScriptableObject.Audio;
using UnityEngine;

public class ChapterProgressBar : MonoBehaviour
{
    [SerializeField] private ChapterProgressBlock blockPrefab;
    [SerializeField] private Transform blockContainer;
    [SerializeField] private float hideDelay = 1f;
    [SerializeField] private float hideDuration = 0.3f;

    [Header("Reward SFX")] [SerializeField]
    private BaseAudioServiceSO audioPlayer;

    [SerializeField, SoundKey] private string progressBarIncreasingSFX;

    private List<ChapterProgressBlock> blocks;
    private Sequence currentBlockSequence;
    private int currentBlockIndex = -1;

    public void Initialize()
    {
        this.blocks = new List<ChapterProgressBlock>();
        transform.localScale = Vector3.zero;
    }

    public void SetProgressData(int completedCount, int totalLevels)
    {
        this.currentBlockIndex = completedCount - 1;
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

                if (i == this.currentBlockIndex)
                {
                    this.blocks[i].HideFill();
                    Debug.Log($"--- (CHAPTER) Block {i}: {blockType} --- Completed: {i < completedCount} --- Hide Fill");
                }
                else
                {
                    this.blocks[i].ResetFill();
                    Debug.Log($"--- (CHAPTER) Block {i}: {blockType} --- Completed: {i < completedCount} --- Reset Fill");
                }
            }
            else
            {
                this.blocks[i].gameObject.SetActive(false);
            }
        }
    }

    public async UniTask PlayShowAnimation(bool autoHide = true)
    {
        this.currentBlockSequence?.Kill();
        PlayProgressBarSFX();

        transform.localScale = Vector3.zero;
        this.blocks[this.currentBlockIndex].HideFill();

        await transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).AsyncWaitForCompletion();

        if (this.currentBlockIndex >= 0 && this.currentBlockIndex < this.blocks.Count)
        {
            this.currentBlockSequence = this.blocks[this.currentBlockIndex].PlayCurrentBlockAnimation();
            await this.currentBlockSequence.AsyncWaitForCompletion();
        }

        if (!autoHide) return;

        await UniTask.WaitForSeconds(this.hideDelay);
    }

    public async UniTask PlayHideAnimation()
    {
        await transform.DOScale(0f, this.hideDuration).SetEase(Ease.InBack).AsyncWaitForCompletion();
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

    public void PlayProgressBarSFX()
    {
        this.audioPlayer.StopSound(this.progressBarIncreasingSFX);
        this.audioPlayer.PlaySound(this.progressBarIncreasingSFX);
    }
}