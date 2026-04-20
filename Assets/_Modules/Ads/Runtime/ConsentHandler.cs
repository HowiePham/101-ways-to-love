using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace _Modules.Ads
{
    public class ConsentHandler
    {
        public bool IsConsentShowed { private set; get; }
        public bool IsConsentLoaded { private set; get; }
        public bool IsConsentLoadFailed { private set; get; }
        public ConsentForm ConsentForm { private set; get; }

        public event Action OnConsentLoaded;
        public event Action OnConsentLoadFailed;
        public event Action OnConsentShowed;
        public event Action OnConsentShowFailed;

        private const float TimeOutSeconds = 2f;

        public async UniTask LoadConsentAsync()
        {
            ConsentRequestParameters request = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false,
            };

#if DEVELOPMENT
            request.ConsentDebugSettings = new ConsentDebugSettings
            {
                DebugGeography = DebugGeography.EEA,
                TestDeviceHashedIds =
                    new List<string>
                    {
                        "BAB7D139-24B3-4699-A6AF-3DC8DFF555F4",
                    }
            };
#endif

#if !UNITY_EDITOR
            if (ConsentInformation.CanRequestAds())
            {
                Debug.Log("[UMP] Consent cached — skipping form load, refreshing in background");
                this.IsConsentLoaded = true;
                ConsentInformation.Update(request, _ => { });
                return;
            }
#endif

            ConsentInformation.Update(request, OnConsentInfoUpdated);

            var cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(TimeOutSeconds));

            try
            {
                await UniTask.WaitUntil(() => this.IsConsentLoaded || this.IsConsentLoadFailed, cancellationToken: cts.Token);
                Debug.Log("[UMP] Consent form loaded");
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken == cts.Token)
                    Debug.Log("[UMP] Consent load timeout");
            }
            finally
            {
                cts.Cancel();
                cts.Dispose();
            }
        }

        public void ShowConsentIfNeeded()
        {
            if (!this.IsConsentLoadFailed && CanShowConsent())
            {
                ShowConsent();
            }
        }

        public async UniTask InitAdmobConsent()
        {
            await LoadConsentAsync();
            ShowConsentIfNeeded();
        }

        public void ShowConsent()
        {
            if (this.ConsentForm == null)
            {
                Debug.LogWarning("[UMP] ConsentForm is null, cannot show consent");
                return;
            }

            this.ConsentForm.Show(ConsentShowHandler);
        }

        public bool CanShowConsent()
        {
#if UNITY_EDITOR
            return false;
#else
            return this.IsConsentLoaded && !this.IsConsentShowed && !ConsentInformation.CanRequestAds();
#endif
        }

        private void ConsentShowHandler(FormError consentError)
        {
            if (consentError != null)
            {
                // Handle the error.
                Debug.LogError("[UMP] " + consentError.Message);
                OnConsentShowFailed?.Invoke();
                return;
            }

            this.IsConsentShowed = true;
            OnConsentShowed?.Invoke();
        }

        private void OnConsentInfoUpdated(FormError consentError)
        {
            if (consentError != null)
            {
                // Handle the error.
                Debug.LogError("[UMP] " + consentError.Message);
                this.IsConsentLoadFailed = true;
                return;
            }

            // If the error is null, the consent information state was updated.

            //Load Consent
            ConsentForm.Load(ConsentLoadHandler);
        }

        private void ConsentLoadHandler(ConsentForm consentForm, FormError consentError)
        {
            if (consentError != null)
            {
                // Consent Load Error
                Debug.LogError("[UMP] " + consentError.Message);
                OnConsentLoadFailed?.Invoke();
                this.IsConsentLoadFailed = true;
                return;
            }

            // Consent Load Completed
            Debug.Log("[UMP] Consent Load Completed!");
            this.ConsentForm = consentForm;
            this.IsConsentLoaded = true;
            OnConsentLoaded?.Invoke();
        }
    }
}