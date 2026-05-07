using Firebase.Analytics;
using Mimi.Analytics.Tracking;

namespace Tracking
{
    public class TestTrackingProvider : ITrackingProvider
    {
        public bool IsReady { get; } = true;

        public TestTrackingProvider()
        {
        }

        public ITrackingProvider SetUserId(string id)
        {
            return this;
        }

        public IEvent NewEvent(string id)
        {
            var firebaseEvent = new TestTrackingEvent(id);
            return firebaseEvent;
        }

        public ITrackingProvider SetUserProperty(string id, string value)
        {
            return this;
        }
    }
}