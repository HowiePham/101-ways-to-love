using System;
using System.Collections.Generic;
using Mimi.Prototypes.Currencies;
using Mimi.Prototypes.Pooling;
using Mimi.ServiceLocators;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditors.Utils;
#endif
using UnityEngine;

namespace Mimi.Prototypes.UI
{
    public class DialogManager : MonoBehaviour, IService
    {
        [SerializeField] private ModalDialogData[] modalDialogData;

        private Dictionary<Type, List<ModalDialogData>> modalLookup;

        private List<BaseModalDialog> currentShowDialogs;

        public void Initialize()
        {
            this.modalLookup = new Dictionary<Type, List<ModalDialogData>>();
            this.currentShowDialogs = new List<BaseModalDialog>();

            foreach (var data in modalDialogData)
            {
                if (!this.modalLookup.ContainsKey(data.ModelType))
                {
                    this.modalLookup.Add(data.ModelType, new List<ModalDialogData>());
                }

                this.modalLookup[data.ModelType].Add(data);
            }
        }

        private bool IsShowingDialogType<T>(DialogId dialogId)
        {
            foreach (BaseModalDialog dialog in this.currentShowDialogs)
            {
                if (dialog.DialogId != dialogId || typeof(T) != dialog.GetType()) continue;
                return true;
            }

            return false;
        }

        public bool TryShowModalDialogOnce<T>(DialogId dialogId, out T modalDialog) where T : BaseModalDialog
        {
            modalDialog = null;
            if (IsShowingDialogType<T>(dialogId)) return false;
            TryShowModalDialogDelayShow(dialogId, out modalDialog);
            modalDialog.Show();
            return true;
        }

        /// <summary>
        /// Try to show a dialog of type with id
        /// </summary>
        /// <param name="dialogId"></param>
        /// <param name="modalDialog"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryShowModalDialog<T>(DialogId dialogId, out T modalDialog) where T : BaseModalDialog
        {
            TryShowModalDialogDelayShow(dialogId, out modalDialog);
            modalDialog.Show();
            return true;
        }

        public bool TryShowModalDialogDelayShow<T>(DialogId dialogId, out T modalDialog) where T : BaseModalDialog
        {
            modalDialog = null;
            ModalDialogData dialogData = FindDialogData<T>(dialogId);

            if (dialogData == null)
            {
                Debug.LogError("No Modal Dialog Data with Id" + dialogId);
                return false;
            }

            modalDialog = ServiceLocator.Global.Get<IPoolService>()
                .Spawn<T>(dialogData.ModalPrefab, Vector3.zero, Quaternion.identity);
            modalDialog.DialogId = dialogId;
            this.currentShowDialogs.Add(modalDialog);
            modalDialog.OnHiding += DialogHidingHandler;
            return true;
        }

        private void DialogHidingHandler(BaseModalDialog dialog)
        {
            this.currentShowDialogs.Remove(dialog);
        }

        public void PoolDialog<T>(DialogId dialogId, int amount) where T : BaseModalDialog
        {
            ModalDialogData dialogData = FindDialogData<T>(dialogId);

            if (dialogData == null)
            {
                Debug.LogError("No Modal Dialog Data with Id" + dialogId);
                return;
            }

            ServiceLocator.Global.Get<IPoolService>().Preload(dialogData.ModalPrefab, amount);
        }

        private ModalDialogData FindDialogData<T>(DialogId dialogId) where T : BaseModalDialog
        {
            var type = typeof(T);

            if (!this.modalLookup.ContainsKey(type))
            {
                Debug.LogError("Modal Dialog Type does not exist " + type);
                return default;
            }

            List<ModalDialogData> dialogDatas = this.modalLookup[type];
            ModalDialogData dialogData = null;

            foreach (var data in dialogDatas)
            {
                if (data.Id != dialogId) continue;
                dialogData = data;
                break;
            }

            return dialogData;
        }

#if UNITY_EDITOR


        // private void Start()
        // {
        //     var foundAll = AssetDatabaseUtils.GetAssetsOfType<ModalDialogData>();
        //
        //     if (foundAll.Length != modalDialogData.Length)
        //     {
        //         var missing = foundAll.Except(modalDialogData).ToList();
        //         Debug.LogWarning("Missing Dialog in game scene manager: \n" + string.Join("\n", missing));
        //     }
        // }

        [Button]
        private void FindAllModalDialogDataInAssetDatabase()
        {
            modalDialogData = AssetDatabaseUtils.GetAssetsOfType<ModalDialogData>();
        }
#endif
    }
}