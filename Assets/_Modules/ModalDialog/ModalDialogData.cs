using Mimi.Prototypes.Currencies;
using Sirenix.OdinInspector;
using TypeReferences;
using UnityEngine;

namespace Mimi.Prototypes.UI
{
    [CreateAssetMenu(fileName = "ModalDialogData", menuName = "UI/Modal Dialog Data")]
    public class ModalDialogData : ScriptableObject
    {
        [SerializeField, ClassExtends(typeof(BaseModalDialog))]
        private ClassTypeReference modelType;

        [SerializeField] private DialogId id;


        [SerializeField] private GameObject modalPrefab;

        public DialogId Id => this.id;

        public ClassTypeReference ModelType => this.modelType;

        public GameObject ModalPrefab => this.modalPrefab;
    }
}