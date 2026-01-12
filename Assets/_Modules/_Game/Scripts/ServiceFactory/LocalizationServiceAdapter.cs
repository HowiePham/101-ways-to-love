using System;
using Mimi.Localizations;

namespace Mimi
{
    public class LocalizationServiceAdapter : ILocalizationService
    {
        private readonly ILocalizer localizer;

        public Language CurrentLanguage => this.localizer.CurrentLanguage;
        public event Action<Language> OnLanguageChanged;

        public LocalizationServiceAdapter(ILocalizer localizer)
        {
            this.localizer = localizer;
            localizer.OnLanguageChanged += LanguageChangedHandler;
        }

        private void LanguageChangedHandler(Language language)
        {
            OnLanguageChanged?.Invoke(language);
        }

        public string Localize(string key)
        {
            return this.localizer.Localize(key);
        }

        public string Localize(string key, params object[] parameters)
        {
            return this.localizer.Localize(key, parameters);
        }

        public void SetCurrentLanguage(Language language)
        {
            this.localizer.SetCurrentLanguage(language);
        }

        public bool IsLanguageSupported(Language language)
        {
            return this.localizer.IsLanguageSupported(language);
        }
    }
}