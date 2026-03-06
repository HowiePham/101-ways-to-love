using System;
using System.Linq;
using Mimi.Reflection.Util;
using Mimi.Watermarks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mimi.DevOps.Testing.Poco
{
    public static class Automator
    {
        public static bool IsTestMode { private set; get; }
        private static bool IsRunning;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CheckAutomation()
        {
            if(IsRunning) return;
            IsRunning = true;
            
            if (AutomationGuard.ShouldEnableAutomation())
            {
                IsTestMode = true;
                AutomationGuard.Consume();
                Watermark.Add("Automated");
                var automatorGo = new GameObject("Automator");
                Object.DontDestroyOnLoad(automatorGo);
                var pocoManager = automatorGo.AddComponent<PocoManager>();
                var pocoListener = Object.FindFirstObjectByType<PocoListenersBase>(FindObjectsInactive.Include);

                if (pocoListener != null && pocoListener.GetType() != typeof(PocoListenersBase))
                {
                    pocoManager.pocoListenersBase = pocoListener;
                    Debug.Log(
                        $"[{nameof(Automator)}] PocoListenersBase subclass found:  {pocoListener.GetType().FullName}");
                }
                else
                {
                    Type listenerType = ReflectionUtils.FindTypesWithBase<PocoListenersBase>().FirstOrDefault();
                    if (listenerType != null && listenerType != typeof(PocoListenersBase))
                    {
                        Component listener = automatorGo.AddComponent(listenerType);
                        pocoManager.pocoListenersBase = (PocoListenersBase)listener;
                        Debug.Log(
                            $"[{nameof(Automator)}] PocoListenersBase subclass found:  {listenerType.FullName}");
                    }
                    else
                    {
                        Debug.LogError($"[{nameof(Automator)}] PocoListenersBase subclass not found");
                    }
                }

                pocoManager.Initialize();
            }
            else
            {
                Debug.LogError($"[{nameof(Automator)}] Automator is not enabled");
            }
        }
    }
}