using System;
using BW.EventSystem;
using UnityEngine;
using UnityEngine.UI;

public class WinView : MonoBehaviour
{
    [SerializeField] private Button nextButton;
    public Action OnButtonClicked;

    private void Awake()
    {
        this.nextButton.onClick.AddListener(PlayNextLevel);
    }

    private void PlayNextLevel()
    {
        Messenger.Broadcast(EventKey.LevelComplete);
        this.gameObject.SetActive(false);
    }
}