using System.Collections.Generic;

namespace Tracking
{
    public class TestParameter
    {
        internal string Name { get; set; }

        internal object Value { get; set; }

        public TestParameter(string parameterName, string parameterValue)
        {
            this.Name = parameterName;
            this.Value = (object) parameterValue;
        }

        public TestParameter(string parameterName, long parameterValue)
        {
            this.Name = parameterName;
            this.Value = (object) parameterValue;
        }

        public TestParameter(string parameterName, double parameterValue)
        {
            this.Name = parameterName;
            this.Value = (object) parameterValue;
        }

        public TestParameter(string parameterName, IDictionary<string, object> parameterValue)
        {
            this.Name = parameterName;
            this.Value = (object) parameterValue;
        }

        public TestParameter(
            string parameterName,
            IEnumerable<IDictionary<string, object>> parameterValue)
        {
            this.Name = parameterName;
            this.Value = (object) parameterValue;
        }
    }
}