using System.Collections.Generic;

namespace Mimi.Prototypes.LevelManagement
{
    public class ChapterInfo
    {
        public int ChapterNumber { get; }
        public List<LevelInfo> Levels { get; }
        public int LevelCount => Levels.Count;

        public ChapterInfo(int chapterNumber, List<LevelInfo> levels)
        {
            ChapterNumber = chapterNumber;
            Levels = levels;
        }
    }
}
