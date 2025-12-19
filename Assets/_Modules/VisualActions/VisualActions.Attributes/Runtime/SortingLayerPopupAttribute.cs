using System;

namespace Mimi.VisualActions.Attribute
{
    // Attribute để đánh dấu field nào cần hiển thị SortingLayer popup
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SortingLayerPopupAttribute : System.Attribute
    {
        public SortingLayerPopupAttribute() { }
    }

}