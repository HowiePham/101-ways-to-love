using System.Linq;
using Games;

namespace Mimi.Prototypes.LevelManagement
{
    public class RemoteChapterParser
    {
        private readonly string chapterRemoteData;

        public RemoteChapterParser(string chapterRemoteData)
        {
            this.chapterRemoteData = chapterRemoteData;
        }

        public IChapterModel[] Parse()
        {
            return CSVSerializer.Deserialize<RemoteChapterModel>(this.chapterRemoteData);
        }
    }
}
