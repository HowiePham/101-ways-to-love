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

        public IEnumerable<string> Parse()
        {
            RemoteOrderModel[] items = CSVSerializer.Deserialize<RemoteOrderModel>(this.levelRemoteData);
            return items.Select(x => x.Id).Distinct();
        }
    }
}