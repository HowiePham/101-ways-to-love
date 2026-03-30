using System;

namespace Mimi.Prototypes.LevelManagement
{
    [Serializable]
    public class LevelInfo
    {
        public string Id { get; }
        public string PrefabAddress { get; }
        public int StageNumber { get; }
        public string IconName { get; }
        public int Chapter { get; }

        public LevelInfo(string id, string prefabAddress, int stageNumber, int chapter = 1, string iconName = "")
        {
            Id = id;
            PrefabAddress = prefabAddress;
            StageNumber = stageNumber;
            Chapter = chapter;
            IconName = iconName;
        }
    }
}