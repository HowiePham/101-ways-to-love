using System;
using UnityEngine;

[Serializable]
public class TutorialStepData
{
    public string stepId;
    public TutorialTargetElement targetElement;
    public float spotlightPadding = 20f;
    public string title;
    [TextArea(2, 4)]
    public string description;
    public bool isLastStep;
}

public enum TutorialTargetElement
{
    None = 0,
    HintButton,
    SkipButton,
    LifeView,
    SettingButton,
    StepPanel,
    ChapterProgressBar,
    StartLevelButton,
    LevelTitleText,
}
