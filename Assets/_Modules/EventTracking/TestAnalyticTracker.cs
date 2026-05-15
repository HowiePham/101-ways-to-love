using System.Reflection;
using Firebase.Analytics;
using Mimi.Analytics.Tracking;
using Mimi.Analytics.Tracking.Trackers;
using UnityEngine;

namespace Tracking
{
    public class TestAnalyticTracker : IAnalyticTracker
    {
        private readonly TestTrackingProvider trackingProvider;

        public TestAnalyticTracker(TestTrackingProvider trackingProvider)
        {
            this.trackingProvider = trackingProvider;
        }

        public void LogEvent(IEventData e)
        {
            PropertyInfo[] properties = e.GetType().GetProperties();
            var newEvent = (TestTrackingEvent)this.trackingProvider.NewEvent(properties[0].GetValue(e).ToString());

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
        }

        public void LogMachineLearningEvent(IMachineLearningEventData e)
        {
        }

        public void SetUserProperties(IUserPropertyData property)
        {
        }
    }
}