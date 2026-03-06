using UnityEngine;

namespace Mimi.NativeEnvironment
{
    public static class NativeEnv
    {
#if UNITY_IOS && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void iOSSet(string key, string value);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string iOSGet(string key, string defaultValue);

        private const string IOS_PREFIX = "automation_";
#endif

        private static string FormatKey(string key)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return $"debug.{Application.identifier}.{key}";
#else
            return key;
#endif
        }

        public static void Set(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                return;

#if UNITY_EDITOR
            PlayerPrefs.SetString(key, value);

#elif UNITY_ANDROID
            try
            {
                string k = FormatKey(key);
                using var sysProp = new AndroidJavaClass("android.os.SystemProperties");
                sysProp.CallStatic("set", k, value);
                Debug.Log($"[NativeEnv] set {k} = {value}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[NativeEnv] SystemProperties.set failed: {e.Message}");
            }

#elif UNITY_IOS
            iOSSet(IOS_PREFIX + key, value);

#endif
        }

        public static string Get(string key, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;

#if UNITY_EDITOR
            return PlayerPrefs.GetString(key, defaultValue);

#elif UNITY_ANDROID
            try
            {
                string k = FormatKey(key);
                using var sysProp = new AndroidJavaClass("android.os.SystemProperties");
                return sysProp.CallStatic<string>("get", k, defaultValue);
            }
            catch(System.Exception e)
            {
                Debug.LogError($"[NativeEnv] SystemProperties.get failed: {e.Message}");
                return defaultValue;
            }

#elif UNITY_IOS
            return iOSGet(IOS_PREFIX + key, defaultValue);

#else
            return defaultValue;
#endif
        }
    }
}