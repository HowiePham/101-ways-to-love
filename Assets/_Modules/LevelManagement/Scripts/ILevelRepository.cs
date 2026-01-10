using System.Collections.Generic;

namespace Mimi.Prototypes.LevelManagement
{
    public interface ILevelRepository
    {
        int NumberOfLevels { get; }
        IEnumerable<LevelInfo> GetAll();
        bool TryGet(string id, out LevelInfo levelInfo);
    }
}