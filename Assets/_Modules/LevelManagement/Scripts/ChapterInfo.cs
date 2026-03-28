using System.Collections.Generic;

namespace Mimi.Prototypes.LevelManagement
{
    public class ChapterInfo
    {
        public int ChapterNumber { get; }
        public string ChapterIconAddress { get; }
        public string ChapterName { get; }
        public List<LevelInfo> Levels { get; }
        public int LevelCount => Levels.Count;

        public ChapterInfo(int chapterNumber, List<LevelInfo> levels, string chapterIconAddress, string chapterName)
        {
            ChapterNumber = chapterNumber;
            Levels = levels;
            ChapterIconAddress = chapterIconAddress;
            ChapterName = chapterName;
        }
    }
}
