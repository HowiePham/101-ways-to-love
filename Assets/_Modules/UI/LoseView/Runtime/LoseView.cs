using System;
using BW.EventSystem;
using UnityEngine;
using UnityEngine.UI;

public class LoseView : MonoBehaviour
{
    [SerializeField] private Button playAgainButton;
    public Action OnButtonClicked;

    private void Awake()
    {
        this.playAgainButton.onClick.AddListener(PlayAgain);
    }

    private void PlayAgain()
    {
        Messenger.Broadcast(EventKey.PlayAgain);
        this.gameObject.SetActive(false);
    }
}