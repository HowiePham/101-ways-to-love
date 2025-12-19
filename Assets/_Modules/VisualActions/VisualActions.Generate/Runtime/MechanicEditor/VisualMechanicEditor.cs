using Mimi.VisualActions.ControlFlow;
using Sirenix.OdinInspector;
using UnityEngine;

namespace  Mimi.VisualActions.Generate
{
    public class VisualMechanicEditor : MonoBehaviour
    {
        [SerializeField,HideInInspector] private GameObject actionBefore;
        [SerializeField, HideInInspector] private GameObject actionAfter;

        [SerializeReference, OnValueChanged("OnActionBeforeChanged")] private ActionEditor actionBeforeEditor;
        [SerializeReference, OnValueChanged("OnActionAfterChanged")] private ActionEditor actionAfterEditor;
       
        #if UNITY_EDITOR
        private void OnActionBeforeChanged(ActionEditor actionEditor)
        {
            if (actionBefore != null)
            {
                DestroyImmediate(actionBefore.gameObject);
            }
            actionEditor.OnInit(gameObject);
            actionEditor.actionObject.transform.SetAsFirstSibling();
            actionBefore = actionEditor.actionObject.gameObject;
        }
        
        private void OnActionAfterChanged(ActionEditor actionEditor)
        {
            if (actionAfter != null)
            {
                DestroyImmediate(actionAfter.gameObject);
            }
            actionEditor.OnInit(gameObject);
            actionEditor.actionObject.transform.SetAsLastSibling();
            actionAfter = actionEditor.actionObject.gameObject;
        }
        #endif
    }
}