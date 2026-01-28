using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;
using System.Collections;

public class LanguageSelectorController : MonoBehaviour
{
    [Header("Setup")]
    public UIDocument uiDocument;

    // We link sprites to codes here in the Inspector
    [System.Serializable]
    public struct LangData
    {
        public string code; // e.g. "en", "el"
        public Texture2D flag;
    }

    public List<LangData> languageIcons; // Fill this in Inspector

    private VisualElement _optionsList;
    private VisualElement _currentFlagIcon;
    private bool _isOpen = false;

    void OnEnable()
    {
        // Wait for localization to load
        StartCoroutine(Initialize());
    }

    IEnumerator Initialize()
    {
        yield return LocalizationSettings.InitializationOperation;

        var root = uiDocument.rootVisualElement;

        // 1. Find the main parts
        _optionsList = root.Q<VisualElement>("OptionsList");
        _currentFlagIcon = root.Q<VisualElement>("CurrentFlagIcon");
        var headerBtn = root.Q<Button>("DropdownHeader");

        // 2. Setup the Header Click
        if (headerBtn != null)
        {
            headerBtn.clicked += ToggleDropdown;
        }

        // 3. Setup the 4 Language Buttons
        SetupLanguageButton(root, "Btn_en", "en");
        SetupLanguageButton(root, "Btn_el", "el");
        SetupLanguageButton(root, "Btn_fr", "fr");
        SetupLanguageButton(root, "Btn_es", "es");

        // 4. Set initial flag
        UpdateHeaderFlag(LocalizationSettings.SelectedLocale.Identifier.Code);
    }

    void SetupLanguageButton(VisualElement root, string buttonName, string langCode)
    {
        var btn = root.Q<Button>(buttonName);
        if (btn != null)
        {
            btn.clicked += () => OnLanguageSelected(langCode);
        }
    }

    void ToggleDropdown()
    {
        _isOpen = !_isOpen;
        // Switch between Flex (Visible) and None (Hidden)
        _optionsList.style.display = _isOpen ? DisplayStyle.Flex : DisplayStyle.None;
    }

    void OnLanguageSelected(string langCode)
    {
        // 1. Close list
        _isOpen = false;
        _optionsList.style.display = DisplayStyle.None;

        // 2. Change Game Language
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(langCode);

        // 3. Update the Top Flag
        UpdateHeaderFlag(langCode);
    }

    void UpdateHeaderFlag(string langCode)
    {
        // Find the matching texture in our Inspector list
        foreach (var data in languageIcons)
        {
            // We use Contains because sometimes codes are "en-US" vs "en"
            if (langCode.Contains(data.code))
            {
                _currentFlagIcon.style.backgroundImage = new StyleBackground(data.flag);
                return;
            }
        }
    }
}