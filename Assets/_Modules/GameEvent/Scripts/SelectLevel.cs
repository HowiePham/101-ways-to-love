using Mimi.Events.AsyncBus;

namespace _Modules.Gameflow_Events_.Scripts
{
    public class SelectLevel : IMessage
    {
        public int LevelOrder { get; }

        public SelectLevel(int levelOrder)
        {
            this.LevelOrder = levelOrder;
        }
    }
}