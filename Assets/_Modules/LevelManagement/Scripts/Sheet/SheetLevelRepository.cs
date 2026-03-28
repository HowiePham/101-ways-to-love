using System.Collections.Generic;
using Games;
using Mimi.Logging;

namespace Mimi.Prototypes.LevelManagement
{
    public class SheetLevelRepository : ILevelRepository
    {
        public int NumberOfLevels => this.lookup.Count;

        private readonly Dictionary<string, LevelInfo> lookup;

        public SheetLevelRepository(List<SheetLevelModel> levels)
        {
            this.lookup = new Dictionary<string, LevelInfo>(levels.Count);
            var stageNumber = 1;

            foreach (SheetLevelModel levelData in levels)
            {
                var levelInfo = new LevelInfo(levelData.Id, levelData.PrefabAddress, stageNumber, levelData.Chapter);
                this.lookup.Add(levelData.Id, levelInfo);
                stageNumber++;
            }
        }

        public IEnumerable<LevelInfo> GetAll()
        {
            return this.lookup.Values;
        }

        public bool TryGet(string id, out LevelInfo levelInfo)
        {
            levelInfo = null;
            if (!this.lookup.ContainsKey(id))
            {
                MiLogger.LogError($"Level prefab does not exist: {id}");
                return false;
            }

            levelInfo = this.lookup[id];
            return true;
        }
    }
}