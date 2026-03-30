using Mimi.Events.AsyncBus;

namespace _Modules.GameEvent.Scripts
{
    public class IapClick : IMessage
    {
        public string Placement { get; }
        public string ShowType { get; }
        public string TriggerType { get; }
        public string PackName { get; }

        public IapClick(string placement, string showType, string triggerType, string packName)
        {
            this.Placement = placement;
            this.ShowType = showType;
            this.TriggerType = triggerType;
            this.PackName = packName;
        }
    }
}