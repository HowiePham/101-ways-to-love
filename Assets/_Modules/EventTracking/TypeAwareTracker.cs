using System.Reflection;
using Mimi.Analytics.Tracking;
using Mimi.Analytics.Tracking.Trackers;

namespace Tracking
{
    public class TypeAwareTracker : IAnalyticTracker
    {
        private readonly ITrackingProvider trackingProvider;

        public TypeAwareTracker(ITrackingProvider trackingProvider)
        {
            this.trackingProvider = trackingProvider;
        }

        public void LogEvent(IEventData e)
        {
            if (!this.trackingProvider.IsReady) return;
            PropertyInfo[] properties = e.GetType().GetProperties();
            IEvent newEvent = this.trackingProvider.NewEvent(properties[0].GetValue(e).ToString());

            foreach (PropertyInfo property in properties)
            {
                object valueObj = property.GetValue(e);
                if (valueObj == null) continue;

                if (property.PropertyType == typeof(int))
                {
                    newEvent.AddIntParam(property.Name, (int)valueObj);
                }
                else if (property.PropertyType == typeof(float))
                {
                    newEvent.AddFloatParam(property.Name, (float)valueObj);
                }
                else
                {
                    string value = valueObj.ToString();
                    if (!string.IsNullOrEmpty(value))
                        newEvent.AddStringParam(property.Name, value);
                }
            }

            newEvent.Track();
        }

        public void LogMachineLearningEvent(IMachineLearningEventData e)
        {
            if (!this.trackingProvider.IsReady) return;
            PropertyInfo[] properties = e.GetType().GetProperties();
            IEvent tracker = this.trackingProvider.NewEvent(properties[0].GetValue(e).ToString());
            tracker.Track();
        }

        public void SetUserProperties(IUserPropertyData property)
        {
            if (!this.trackingProvider.IsReady) return;
            PropertyInfo[] properties = property.GetType().GetProperties();
            this.trackingProvider.SetUserProperty(
                properties[0].GetValue(property).ToString(),
                properties[1].GetValue(property).ToString());
        }
    }
}
