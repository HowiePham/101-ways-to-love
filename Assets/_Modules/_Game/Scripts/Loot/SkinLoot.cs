using System;
using Mimi.Loots;
using UnityEngine;

[Serializable]
public class SkinLoot : ILoot
{
    [SerializeField] private string skinId;

    public string LootType => "Skin";
    public string SkinId => this.skinId;
    public int Amount => 1;

    public SkinLoot(string skinId)
    {
        this.skinId = skinId;
    }
}