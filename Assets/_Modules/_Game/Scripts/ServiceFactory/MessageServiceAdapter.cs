using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Events.AsyncBus;

namespace Mimi.Prototypes
{
    public class MessageServiceAdapter : IEventService
    {
        private readonly IAsyncPublisher publisher;
        private readonly IAsyncSubscriber subscriber;

        public MessageServiceAdapter(IAsyncPublisher publisher, IAsyncSubscriber subscriber)
        {
            this.publisher = publisher;
            this.subscriber = subscriber;
        }

        public async UniTask PublishAsync<T>(T msg, CancellationToken cancellation = new CancellationToken())
            where T : IMessage
        {
            await this.publisher.PublishAsync(msg, cancellation);
        }

        public IDisposable Subscribe<T>(Func<T, CancellationToken, UniTask> action,
            CancellationToken cancellation = new CancellationToken())
        {
            return this.subscriber.Subscribe(action, cancellation);
        }
    }
}