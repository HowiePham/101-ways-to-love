using Mimi.Events.AsyncBus;

namespace _Modules.GameEvent.Scripts
{
    public class ScreenShown : IMessage
    {
        public string ScreenName { get; }

        public ScreenShown(string screenName)
        {
            ScreenName = screenName;
        }
    }
}
