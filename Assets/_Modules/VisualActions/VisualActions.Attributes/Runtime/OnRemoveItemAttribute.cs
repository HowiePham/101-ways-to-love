using System;

namespace Mimi.VisualActions.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class OnRemoveItemAttribute : System.Attribute
    {
        public string MethodName;
        public OnRemoveItemAttribute(string methodName) => MethodName = methodName;
    }
}