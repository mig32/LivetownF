using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Utils;

namespace Core.Scripts
{
    public class LocalizationManager : StaticReadyChecker<LocalizationManager>
    {
        public enum Lang
        {
            RU,
            EN
        }
        
        [Serializable]
        public class CsvLocale
        {
            public string Key;
            public string Language;
            public string Text;

            static public CsvLocale[] LoadData(string csvText)
            {
                return CsvSerializer.Deserialize<CsvLocale>(csvText);
            }
        }
        
        [SerializeField] private URLConfig _urlsConfig;
        
        private readonly Dictionary<Lang, Dictionary<string, string>> _localesDict = new ();
        
        public Lang SelectedLanguage { get; private set; }
        public event Action OnLanguageChanged;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"Second singleton {GetType()} {gameObject.name}");
                return;
            }
            
            DontDestroyOnLoad(this);
            Downloader.DoOnReady(LoadLocales);
        }

        private void LoadLocales()
        {
            Downloader.Instance.Download(_urlsConfig.LocalesURL, OnDownloaded);
        }
        
        private void OnDownloaded(UnityWebRequest request)
        {
            if (request.isDone && request.result == UnityWebRequest.Result.Success)
            {
                ParseLocales(request.downloadHandler.text);
                SetReady(this);
            }
        }

        private void ParseLocales(string localesCsv)
        {
            _localesDict.Clear();
            var csvLocales = CsvLocale.LoadData(localesCsv);
            foreach (var csvLocale in csvLocales)
            {
                if (Enum.TryParse<Lang>(csvLocale.Language, out var lang))
                {
                    if (!_localesDict.TryGetValue(lang, out var localesDisc))
                    {
                        localesDisc = new Dictionary<string, string>();
                        _localesDict.Add(lang, localesDisc);
                    }
                    
                    localesDisc.Add(csvLocale.Key, csvLocale.Text);
                }
            }
        }
        
        public bool TryGetLocale(string key, out string value) 
        {
            if (_localesDict.TryGetValue(SelectedLanguage, out var localesDisc))
            {
                return localesDisc.TryGetValue(key, out value);
            }

            value = "ERROR";
            return false;
        }

        public void SetLanguage(Lang lang)
        {
            if (lang == SelectedLanguage)
            {
                return;
            }
            
            SelectedLanguage = lang;
            OnLanguageChanged?.Invoke();
        }
    }
}