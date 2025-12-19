using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Data;
using Mimi.VisualActions.Generate.Editor.DraggableExtensions;
using Mimi.VisualActions.Interactions.Draggable.Extensions;
using UnityEngine;

namespace Mimi.VisualActions.Generate.Editor.DraggableExtension
{
    public class ReturnOnDeselectExtensionEdit : BaseDraggableExtensionEdit<ReturnOnDeselect>
    {
        protected override ReturnOnDeselect CreateNewExtension()
        {
            return new ReturnOnDeselectExtensionGenerator().Generate() as ReturnOnDeselect;
        }

        public override void TurnOnHandle()
        {
            base.TurnOnHandle();
            this.Extension.SetField("offsetField", Draggable.GetComponent<Vector3Field>(), AccessModifier.Private);
            if (this.Extension.GetFieldValue<Transform>("startPosition", AccessModifier.Private) == null)
            {
                var returnPosGameObject = new GameObject("ReturnPos");
                returnPosGameObject.transform.SetParent( Draggable.transform.parent);
                returnPosGameObject.transform.position = Draggable.transform.position;
                this.Extension.SetField("startPosition", returnPosGameObject.transform, AccessModifier.Private);
            }
        }
    }
}