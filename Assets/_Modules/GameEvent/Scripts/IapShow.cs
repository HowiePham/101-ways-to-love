using Mimi.Events.AsyncBus;

namespace _Modules.GameEvent.Scripts
{
    public class IapShow : IMessage
    {
        public string Placement { get; }
        public string ShowType { get; }
        public string TriggerType { get; }
        public string PackName { get; }

        public IapShow(string placement, string showType, string triggerType, string packName)
        {
            this.Placement = placement;
            this.ShowType = showType;
            this.TriggerType = triggerType;
            this.PackName = packName;
        }
    }
}