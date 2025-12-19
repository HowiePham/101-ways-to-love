using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Actions;
using Mimi.Logging;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mimi.VisualActions
{
    public abstract class VisualAction : MonoBehaviour, IAsyncAction
    {
        [SerializeField] private UnityEvent onEnter;
        [SerializeField] private UnityEvent onExit;

        [ShowInInspector, ReadOnly] public bool IsInitialized { private set; get; }
        [ShowInInspector, ReadOnly] public bool Completed { private set; get; }
        [ShowInInspector, ReadOnly] public bool IsExecuting { private set; get; }
        [ShowInInspector, ReadOnly] public bool IsPaused { private set; get; }

#if UNITY_EDITOR
        public bool HasError { private set; get; }
        public string ErrorMessage { private set; get; }
#endif

        public async UniTask Initialize()
        {
            if (IsInitialized) return;
            Completed = false;
            IsExecuting = false;
#if UNITY_EDITOR
            HasError = false;
            ErrorMessage = string.Empty;
#endif
            await OnInitializing();
            IsInitialized = true;
        }

        protected virtual async UniTask OnInitializing()
        {
            await UniTask.CompletedTask;
        }

        public async UniTask Execute(CancellationToken cancellationToken)
        {
#if UNITY_EDITOR
            HasError = false;
            EditorGUIUtility.PingObject(gameObject);
#endif

            try
            {
                await OnEnter(cancellationToken);
                await OnExecuting(cancellationToken);

                if (IsPaused)
                {
                    await UniTask.WaitUntil(() => !IsPaused, cancellationToken: cancellationToken);
                }

                await OnExit(cancellationToken);
            }
            catch (OperationCanceledException e)
            {
                IsExecuting = false;
                Completed = false;

#if UNITY_EDITOR
                HasError = false;
                ErrorMessage = string.Empty;
#endif
                MiLogger.Log(
                    $"[Visual Action] Action {GetType().Name} is canceled at game object \"{gameObject.name}\"");
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                HasError = true;
                ErrorMessage = e.Message;
#endif
                MiLogger.LogException(e);
            }
        }

        public void Pause()
        {
            IsPaused = true;
            OnPaused();
        }

        public void Resume()
        {
            IsPaused = false;
            OnResumed();
        }

        protected virtual async UniTask OnEnter(CancellationToken cancellationToken)
        {
            Completed = false;
            IsExecuting = true;
            this.onEnter?.Invoke();
            await UniTask.CompletedTask;
        }

        protected virtual async UniTask OnExit(CancellationToken cancellationToken)
        {
            Completed = true;
            IsExecuting = false;
            this.onExit?.Invoke();
            await UniTask.CompletedTask;
        }

        protected abstract UniTask OnExecuting(CancellationToken cancellationToken);

        public virtual void Dispose()
        {
            this.onEnter.RemoveAllListeners();
            this.onExit.RemoveAllListeners();
        }

        protected virtual void OnPaused()
        {
        }

        protected virtual void OnResumed()
        {
        }

        protected IEnumerable<VisualAction> GetAllChildActions()
        {
            var allActions = new List<VisualAction>();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);

                if (!child.gameObject.activeSelf) continue;
                var action = child.GetComponent<VisualAction>();

                if (action != null)
                {
                    allActions.Add(action);
                }
            }

            return allActions;
        }
    }
}