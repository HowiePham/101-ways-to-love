using System.Collections.Generic;
using System.Linq;

namespace Mimi.Prototypes.LevelManagement
{
    public class RemoteLinearOrderParser
    {
        private readonly string levelRemoteData;

        public RemoteLinearOrderParser(string levelRemoteData)
        {
            this.levelRemoteData = levelRemoteData;
        }

        public IEnumerable<LevelOrderEntry> Parse()
        {
            RemoteOrderModel[] items = CSVSerializer.Deserialize<RemoteOrderModel>(this.levelRemoteData);
            return items
                .GroupBy(x => x.Id)
                .Select(g => g.First())
                .Select(x => new LevelOrderEntry(x.Id, int.TryParse(x.Chapter, out int ch) ? ch : 1));
        }
    }
}