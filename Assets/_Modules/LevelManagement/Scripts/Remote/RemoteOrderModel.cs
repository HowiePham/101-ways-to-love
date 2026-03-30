using System;

namespace Mimi.Prototypes.LevelManagement
{
    [Serializable]
    public class RemoteOrderModel
    {
        public int Order { private set; get; }
        public string Id { private set; get; }
        public string Chapter { private set; get; }
    }
}