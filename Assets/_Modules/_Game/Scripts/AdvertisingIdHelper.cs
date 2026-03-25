using Cysharp.Threading.Tasks;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public static class AdvertisingIdHelper
{
    public static async UniTask<string> GetGoogleAdvertisingId()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Get application context on main thread - works with Unity 6 GameActivity
        // Use context instead of activity since AdvertisingIdClient only needs a Context
        AndroidJavaObject appContext = AndroidApplication.currentActivity?
            .Call<AndroidJavaObject>("getApplicationContext");

        if (appContext == null)
        {
            Debug.LogWarning("Failed to get GAID: Application context is null");
            return null;
        }

        return await UniTask.RunOnThreadPool(() =>
        {
            try
            {
                using var client = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
                using var adInfo = client.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", appContext);
                return adInfo.Call<string>("getId");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to get GAID: {e.Message}");
                return null;
            }
        });
#else
        return null;
#endif
    }
}
