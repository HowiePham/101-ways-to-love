using System.Collections.Generic;
using System.Linq;
using Mimi.Logging;
using UnityEngine;

namespace Mimi.Prototypes.LevelManagement
{
    public class LinearLevelOrder : ILevelOrder
    {
        private readonly ILevelRepository levelRepository;
        private readonly List<LevelInfo> orderedLevels;

        public LinearLevelOrder(ILevelRepository levelRepository, IEnumerable<LevelOrderEntry> levelOrderEntries)
        {
            this.levelRepository = levelRepository;
            this.orderedLevels = new List<LevelInfo>(levelRepository.NumberOfLevels);
            var stageNumber = 1;
            foreach (LevelOrderEntry entry in levelOrderEntries)
            {
                if (levelRepository.TryGet(entry.Id, out LevelInfo levelInfo))
                {
                    var withChapter = new LevelInfo(
                        levelInfo.Id,
                        levelInfo.PrefabAddress,
                        stageNumber,
                        entry.Chapter,
                        levelInfo.IconName);
                    this.orderedLevels.Add(withChapter);
                    stageNumber++;
                }
                else
                {
                    MiLogger.LogError(
                        $"Miss match level order and level repository! Level id not found: {entry.Id}");
                }
            }
        }

        private bool ValidateOrder(int order)
        {
            return order >= 0 && order < this.orderedLevels.Count;
        }

        public LevelInfo GetByOrder(int order)
        {
            if (ValidateOrder(order))
            {
                return this.orderedLevels[order];
            }

            MiLogger.LogError($"Order out of bounds: {order}");
            return null;
        }

        public LevelInfo First()
        {
            return this.orderedLevels.FirstOrDefault();
        }

        public LevelInfo Last()
        {
            return this.orderedLevels.LastOrDefault();
        }

        public bool IsFirst(string id)
        {
            return this.orderedLevels.Count > 0 && First().Id == id;
        }

        public bool IsLast(string id)
        {
            return this.orderedLevels.Count > 0 && Last().Id == id;
        }

        public LevelInfo GetNextLevel(string id)
        {
            if (this.levelRepository.TryGet(id, out LevelInfo currentLevel))
            {
                int currentLevelOrder = this.orderedLevels.FindIndex(x => x.Id == currentLevel.Id);
                int nextOrder = Mathf.Clamp(currentLevelOrder + 1, 0, this.orderedLevels.Count - 1);
                return this.orderedLevels[nextOrder];
            }

            MiLogger.LogError($"Level id not found: {id}");
            return null;
        }

        public LevelInfo GetPreviousLevel(string id)
        {
            if (this.levelRepository.TryGet(id, out LevelInfo currentLevel))
            {
                int currentLevelOrder = this.orderedLevels.FindIndex(x => x.Id == currentLevel.Id);
                int prevOrder = Mathf.Clamp(currentLevelOrder - 1, 0, this.orderedLevels.Count - 1);
                return this.orderedLevels[prevOrder];
            }

            MiLogger.LogError($"Level id not found: {id}");
            return null;
        }

        public LevelInfo GetNextLevel(int order)
        {
            int nextOrder = Mathf.Clamp(order + 1, 0, this.orderedLevels.Count - 1);
            return this.orderedLevels[nextOrder];
        }

        public LevelInfo GetPreviousLevel(int order)
        {
            int prevOrder = Mathf.Clamp(order - 1, 0, this.orderedLevels.Count - 1);
            return this.orderedLevels[prevOrder];
        }

        public IEnumerable<LevelInfo> GetAllOrdered()
        {
            return this.orderedLevels;
        }
    }
}