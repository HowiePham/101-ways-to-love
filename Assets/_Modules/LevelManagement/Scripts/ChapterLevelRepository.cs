using System.Collections.Generic;
using Games;

namespace Mimi.Prototypes.LevelManagement
{
    public class ChapterLevelRepository
    {
        private readonly SortedDictionary<int, ChapterInfo> chapters;

        public int ChapterCount => chapters.Count;

        public ChapterLevelRepository(ILevelRepository repository, List<SheetChapterModel> chapterModels)
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

            var chapterDataMap = new Dictionary<int, SheetChapterModel>();
            foreach (var model in chapterModels)
            {
                if (int.TryParse(model.Id, out int chapterId))
                {
                    chapterDataMap[chapterId] = model;
                }
            }

            chapters = new SortedDictionary<int, ChapterInfo>();

            foreach (var kvp in groups)
            {
                string iconAddress = string.Empty;
                string chapterName = string.Empty;

                if (chapterDataMap.TryGetValue(kvp.Key, out var chapterModel))
                {
                    iconAddress = chapterModel.ChapterIconAddress;
                    chapterName = chapterModel.ChapterName;
                }

                chapters[kvp.Key] = new ChapterInfo(kvp.Key, kvp.Value, iconAddress, chapterName);
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
