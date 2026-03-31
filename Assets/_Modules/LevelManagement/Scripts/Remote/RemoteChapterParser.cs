using System.Text.RegularExpressions;
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
            string normalized = this.chapterRemoteData.Replace("\\n", "\n").Replace("\\r", "\r");
            normalized = Regex.Replace(normalized, @" (\d)", "\n$1");
            return CSVSerializer.Deserialize<RemoteChapterModel>(normalized);
        }
    }
}
