using Mimi.NativeEnvironment;
using UnityEngine;

namespace Mimi.DevOps.Testing.Poco
{
    public static class AutomationGuard
    {
        private const string Key = "poco.enable";

        // Inject this value from CI before building for strict mode validation.
        // Run: curl -H "X-API-Key: $KEY" $FARM_URL/api/admin/automation-token
        // Leave empty to accept any valid 64-char hex token (format-only mode).
        private const string FarmToken = "";

        public static bool ShouldEnableAutomation()
        {
#if UNITY_EDITOR
            Debug.Log("[AutomationGuard] Editor mode -> Automation Enabled");
            return true;
#else
            try
            {
                var flag = NativeEnv.Get(Key, "0");
                bool enabled = IsValidToken(flag);
                Debug.Log($"[AutomationGuard] {Key}={flag}, enabled={enabled}");
                return enabled;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[AutomationGuard] Failed: {e.Message}");
                return false;
            }
#endif
        }

        private static bool IsValidToken(string value)
        {
            if (string.IsNullOrEmpty(value) || value == "0" || value == "1")
                return false;

            // Strict mode: only accept this farm's exact token
            if (!string.IsNullOrEmpty(FarmToken))
                return value == FarmToken;

            // Format-only mode: accept any 64-char lowercase hex string
            if (value.Length != 64) return false;
            foreach (char c in value)
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))
                    return false;
            return true;
        }

        public static void SetForTest()
        {
            // In strict mode use the baked-in token; otherwise use a valid-format placeholder
            var token = !string.IsNullOrEmpty(FarmToken)
                ? FarmToken
                : "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            NativeEnv.Set(Key, token);
            Debug.Log("[AutomationGuard] TEST flag set");
        }

        public static void Consume()
        {
            NativeEnv.Set(Key, "0");
            Debug.Log("[AutomationGuard] TEST flag cleared");
        }
    }
}