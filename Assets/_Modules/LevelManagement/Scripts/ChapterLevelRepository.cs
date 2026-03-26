using System.Collections.Generic;

namespace Mimi.Prototypes.LevelManagement
{
    public class ChapterLevelRepository
    {
        private readonly SortedDictionary<int, ChapterInfo> chapters;

        public int ChapterCount => chapters.Count;

        public ChapterLevelRepository(ILevelRepository repository)
        {
            var groups = new SortedDictionary<int, List<LevelInfo>>();

            foreach (var level in repository.GetAll())
            {
                if (!groups.ContainsKey(level.Chapter))
                {
                    groups[level.Chapter] = new List<LevelInfo>();
                }

                groups[level.Chapter].Add(level);
            }

            chapters = new SortedDictionary<int, ChapterInfo>();

            foreach (var kvp in groups)
            {
                chapters[kvp.Key] = new ChapterInfo(kvp.Key, kvp.Value);
            }
        }

        public IEnumerable<ChapterInfo> GetChapters()
        {
            return chapters.Values;
        }

        public ChapterInfo GetChapter(int chapterNumber)
        {
            if (chapters.TryGetValue(chapterNumber, out var chapter))
            {
                return chapter;
            }

            return null;
        }

        public List<LevelInfo> GetLevelsByChapter(int chapterNumber)
        {
            if (chapters.TryGetValue(chapterNumber, out var chapter))
            {
                return chapter.Levels;
            }

            return new List<LevelInfo>();
        }

        public int GetChapterOfLevel(LevelInfo levelInfo)
        {
            return levelInfo.Chapter;
        }
    }
}
