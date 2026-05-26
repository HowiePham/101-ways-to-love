using Mimi.Loots;

public class SkinLootFactory : BaseLootFactory
{
    public override ILoot Create(LootParams lootParam)
    {
        string skinId = lootParam.Params[0];
        return new SkinLoot(skinId);
    }
}