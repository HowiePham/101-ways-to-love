using System;
using BW.EventSystem;
using UnityEngine;

public class GameScene : MonoBehaviour
{
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private GameObject levelRoot;
    [SerializeField] private GameObject[] levelPrefab;
    [SerializeField] private GameObject winView;
    [SerializeField] private GameObject loseView;

    private void Awake()
    {
        Messenger.AddListener(EventKey.LevelComplete, NextLevel);
        Messenger.AddListener(EventKey.LevelWin, LevelWinHandler);
        Messenger.AddListener(EventKey.LevelLose, LevelLoseHandler);
        Messenger.AddListener(EventKey.PlayAgain, PlayAgainHandler);
    }

    private void NextLevel()
    {
        this.currentLevel++;
        if (this.currentLevel >= this.levelPrefab.Length)
        {
            this.currentLevel = this.levelPrefab.Length - 1;
        }

        StartNewLevel(this.currentLevel);
    }

    private void PlayAgainHandler()
    {
        StartNewLevel(this.currentLevel);
    }

    private void LevelWinHandler()
    {
        this.winView.SetActive(true);
    }

    private void LevelLoseHandler()
    {
        this.loseView.SetActive(true);
    }

    private void StartNewLevel(int level)
    {
        Destroy(this.levelRoot);
        this.levelRoot = null;

        this.levelRoot = Instantiate(this.levelPrefab[level]);
    }
}