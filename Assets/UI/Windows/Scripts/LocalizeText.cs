using Core.Scripts;
using TMPro;
using UnityEngine;

namespace UI.Windows.Scripts
{
    public class LocalizeText : MonoBehaviour
    {
        [SerializeField] private string _localeKey;
        [SerializeField] private TMP_Text _text;
        
        private void Start()
        {
            LocalizationManager.Instance.OnLanguageChanged += UpdateLocale;
            UpdateLocale();
        }

        private void OnDestroy()
        {
            LocalizationManager.Instance.OnLanguageChanged -= UpdateLocale;
        }

        private void UpdateLocale()
        {
            LocalizationManager.Instance.TryGetLocale(_localeKey, out var text);
            _text.text = text;
        }
    }
}
