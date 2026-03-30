using System;
using Mimi.Prototypes.LevelManagement;

namespace Games
{
    [Serializable]
    public class RemoteChapterModel : IChapterModel
    {
        public string Id;
        public string ChapterIconAddress;
        public string ChapterName;

        string IChapterModel.Id => Id;
        string IChapterModel.ChapterIconAddress => ChapterIconAddress;
        string IChapterModel.ChapterName => ChapterName;
    }
}