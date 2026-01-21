using System.Collections.Generic;
using Mimi.Prototypes.UI;
using TMPro;
using UnityEngine;

public class IconBasedLifeView : BaseView
{
    [SerializeField] private TMP_Text timeRemaining;
    [SerializeField] private GameObject lifeIconPrefab;
    [SerializeField] private List<GameObject> lifeIcons = new List<GameObject>();
    private int currentLifeCount;

    public void SetLifeCount(int lifeCount)
    {
        if (lifeCount == this.currentLifeCount)
        {
            return;
        }

        this.currentLifeCount = lifeCount;
        var count = 1;
        foreach (GameObject lifeIcon in this.lifeIcons)
        {
            lifeIcon.SetActive(count <= lifeCount);

            count++;
        }
    }

    public void SetTimeRemaining(string timeRemaining)
    {
        this.timeRemaining.text = timeRemaining;
    }
}