using Mimi.Events.AsyncBus;

namespace _Modules.GameEvent.Scripts
{
    public class IapPurchase : IMessage
    {
        public string Placement { get; }
        public string ShowType { get; }
        public string TriggerType { get; }
        public string PackName { get; }
        public string Price { get; }
        public string Currency { get; }

        public IapPurchase(string placement, string showType, string triggerType, string packName, string price, string currency)
        {
            this.Placement = placement;
            this.ShowType = showType;
            this.TriggerType = triggerType;
            this.PackName = packName;
            this.Price = price;
            this.Currency = currency;
        }
    }
}