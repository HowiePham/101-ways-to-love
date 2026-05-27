using System.Collections.Generic;
using Mimi.Loots;
using Mimi.Prototypes.LevelManagement;
using UnityEngine;

public class SkinLootProcessor : ILootProcessor
{
    private readonly GameData gameData;
    private readonly SheetAngelSkinRepo sheetAngelSkinRepo;

    public SkinLootProcessor(GameData gameData, SheetAngelSkinRepo sheetAngelSkinRepo)
    {
        this.gameData = gameData;
        this.sheetAngelSkinRepo = sheetAngelSkinRepo;
    }

    public bool Process(ILoot loot, LootContext lootContext)
    {
        var skinLoot = (SkinLoot)loot;
        string skinId = skinLoot.SkinId;
        bool containSkin = this.sheetAngelSkinRepo.IsContainSkin(skinId);

        if (!containSkin)
        {
            return false;
        }

        string skinType = this.sheetAngelSkinRepo.GetSkinType(skinId);

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
        this.gameData.EquippedAngelSkins[skinType] = skinId;

        Debug.Log($"[SkinLootProcessor] Skin granted: {skinId}");
        return true;
    }

    public void Process(IReadOnlyList<ILoot> loots, LootContext lootContext)
    {
        foreach (ILoot loot in loots)
            Process(loot, lootContext);
    }
}