using System.Collections.Generic;
using _Modules.LevelManagement.Scripts.Sheet;
using Mimi.DataSources.GoogleSheet;
using Mimi.Loots;

namespace Mimi.Prototypes.LevelManagement
{
    public class SheetAngelUpgradeRepository
    {
        private readonly ILootFactory skinLootFactory;
        private readonly Dictionary<int, List<ILoot>> angelUpgrades;

        public SheetAngelUpgradeRepository(IEnumerable<SheetAngelUpgradeModel> sheetAngelUpgradeModels, ILootFactory skinLootFactory)
        {
            this.skinLootFactory = skinLootFactory;
            this.angelUpgrades = new Dictionary<int, List<ILoot>>();

            foreach (SheetAngelUpgradeModel model in sheetAngelUpgradeModels)
            {
                if (this.angelUpgrades.ContainsKey(model.Order))
                {
                    continue;
                }

                IReadOnlyList<LootParams> lootParams = ParseToLootParams(model.Params);
                IList<ILoot> skinLoots = this.skinLootFactory.Create(lootParams);
                this.angelUpgrades[model.Order] = (List<ILoot>)skinLoots;
            }
        }

        public IList<ILoot> GetAngelUpgrades(int order)
        {
            if (!this.angelUpgrades.ContainsKey(order))
            {
                return null;
            }

            return this.angelUpgrades[order];
        }

        private IReadOnlyList<LootParams> ParseToLootParams(IReadOnlyList<MultiParam> multiParams)
        {
            if (IsInvalidMultiParam(multiParams))
            {
                return null;
            }

            var lootParams = new List<LootParams>();

            foreach (MultiParam param in multiParams)
            {
                string type = param.Type;
                string[] paramList = param.Params;

                var newLootParam = LootParams.New(type, paramList);
                lootParams.Add(newLootParam);
            }

            return lootParams;
        }

        private bool IsInvalidMultiParam(IReadOnlyList<MultiParam> multiParams)
        {
            return multiParams == null || multiParams.Count == 0;
        }
    }
}