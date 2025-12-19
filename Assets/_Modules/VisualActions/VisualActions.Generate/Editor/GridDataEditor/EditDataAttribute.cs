using System;
using NUnit.Framework;

namespace Mimi.VisualActions.Generate.Editor
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EditDataAttribute : PropertyAttribute
    {
        public Type InterfaceType { get; }
        public EditDataAttribute(Type interfaceType) => InterfaceType = interfaceType;
    }
}