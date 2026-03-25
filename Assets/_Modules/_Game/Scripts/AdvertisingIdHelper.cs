using Cysharp.Threading.Tasks;
using UnityEngine;

public static class AdvertisingIdHelper
{
    public static async UniTask<string> GetGoogleAdvertisingId()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return await UniTask.RunOnThreadPool(() =>
        {
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
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
