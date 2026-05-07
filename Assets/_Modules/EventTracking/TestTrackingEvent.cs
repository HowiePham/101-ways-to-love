using System;
using System.Collections.Generic;
using Firebase.Analytics;
using Mimi.Analytics.Tracking;

namespace Tracking
{
    internal class TestTrackingEvent : IEvent
    {
        private readonly string id;
        private readonly List<TestParameter> parameters;

        public TestTrackingEvent(string id)
        {
            this.id = id;
            this.parameters = new List<TestParameter>();
        }

        public List<TestParameter> Parameters => this.parameters;

        public IEvent AddStringParam(string name, string value)
        {
            this.Parameters.Add(new TestParameter(name, value));
            return this;
        }

        public IEvent AddIntParam(string name, int value)
        {
            this.Parameters.Add(new TestParameter(name, value));
            return this;
        }

        public IEvent AddFloatParam(string name, float value) 
        {
            this.Parameters.Add(new TestParameter(name, Math.Round(value, 2)));
            return this;
        }

        public void Track()
        {
        }
    }
}