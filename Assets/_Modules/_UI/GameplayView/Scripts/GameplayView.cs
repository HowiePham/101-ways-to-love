using System;
using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayView : BaseView
{
    [Header("Text")] [SerializeField] private TMP_Text levelTextCurrent;

    [Header("Button")] [SerializeField] private Button settingBtn;
    [SerializeField] private Button skipBtn;

    public Action OnSettingClicked;
    public Action OnSkipClicked;

    public override void Initialize()
    {
        base.Initialize();
        this.settingBtn.onClick.AddListener(() => this.OnSettingClicked?.Invoke());
        this.skipBtn.onClick.AddListener(() => this.OnSkipClicked?.Invoke());
    }
    
    public void SetLevelCurrent(string level)
    {
        this.levelTextCurrent.text = "Level " + level;
    }
}