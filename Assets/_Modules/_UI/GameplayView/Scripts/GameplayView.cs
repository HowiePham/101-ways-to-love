using System;
using Mimi.Prototypes.UI;
using UnityEngine;
using UnityEngine.UI;

public class GameplayView : BaseView
{
    [SerializeField] private Button pauseButton;

    public Action OnPauseClicked;

    public override void Initialize()
    {
        base.Initialize();
        this.pauseButton.onClick.AddListener(() => this.OnPauseClicked?.Invoke());
    }
}