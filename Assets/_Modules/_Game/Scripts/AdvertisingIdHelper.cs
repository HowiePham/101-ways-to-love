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
        // Get activity reference on main thread (required by Unity 6 GameActivity)
        AndroidJavaObject activity = AndroidApplication.currentActivity;
        if (activity == null)
        {
            Debug.LogWarning("Failed to get GAID: currentActivity is null");
            return null;
        }

        return await UniTask.RunOnThreadPool(() =>
        {
            try
            {
                using var client = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
                using var adInfo = client.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", activity);
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