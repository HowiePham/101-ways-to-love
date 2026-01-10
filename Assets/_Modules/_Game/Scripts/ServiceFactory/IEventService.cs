using Mimi.Events.AsyncBus;
using Mimi.ServiceLocators;

namespace Mimi.Prototypes
{
    public interface IEventService : IAsyncPublisher, IAsyncSubscriber, IService
    {
        
    }
}