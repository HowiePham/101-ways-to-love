using System.Collections.Generic;
using Mimi.Loots;
using UnityEngine;

public class SkinLootProcessor : ILootProcessor
{
    private readonly GameData gameData;

    public SkinLootProcessor(GameData gameData)
    {
        this.gameData = gameData;
    }

    public bool Process(ILoot loot, LootContext lootContext)
    {
        var skinLoot = (SkinLoot)loot;
        string skinId = skinLoot.SkinId;

        if (!this.gameData.AngelSkins.ContainsKey(skinId))
        {
            Debug.LogError($"[SkinLootProcessor] Unknown skin: {skinId}");
            return false;
        }

        if (this.gameData.AngelSkins[skinId])
        {
            Debug.Log($"[SkinLootProcessor] Skin already owned: {skinId}");
            return true;
        }

        this.gameData.AngelSkins[skinId] = true;
        Debug.Log($"[SkinLootProcessor] Skin granted: {skinId}");
        return true;
    }

    public void Process(IReadOnlyList<ILoot> loots, LootContext lootContext)
    {
        foreach (ILoot loot in loots)
            Process(loot, lootContext);
    }
}