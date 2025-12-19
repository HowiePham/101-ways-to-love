using System;

namespace Mimi.VisualActions.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class OnAddItemAttribute : System.Attribute
    {
        public string MethodName;
        public OnAddItemAttribute(string methodName) => MethodName = methodName;
    }
}