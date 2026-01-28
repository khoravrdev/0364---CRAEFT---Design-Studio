using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;

public class UILocalizer : MonoBehaviour
{
    [Header("Setup")]
    public UIDocument uiDocument;
    public string tableCollectionName = "UIText"; 

    void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        StartCoroutine(InitializeAndLoad());
    }

    void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private IEnumerator InitializeAndLoad()
    {
        yield return LocalizationSettings.InitializationOperation;
        UpdateUIText();
    }

    void OnLocaleChanged(Locale locale)
    {
        UpdateUIText();
    }

    public void UpdateUIText()
    {
        if (uiDocument == null) return;
        var root = uiDocument.rootVisualElement;
        
        var stringTableOp = LocalizationSettings.StringDatabase.GetTableAsync(tableCollectionName);
        
        stringTableOp.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                var table = op.Result;

                foreach (var sharedEntry in table.SharedData.Entries)
                {
                    string keyName = sharedEntry.Key;
                    var entry = table.GetEntry(sharedEntry.Id);
                    
                    if (entry == null || string.IsNullOrEmpty(entry.Value)) continue;

                    string translatedText = entry.Value;

                    // --- NEW LOGIC: FIND ALL INSTANCES ---
                    // "Query" finds ALL elements with this name, not just the first one.
                    // We loop through the results and update them all.

                    // 1. LABELS
                    var labels = root.Query<Label>(keyName).Build();
                    foreach (var label in labels) label.text = translatedText;

                    // 2. BUTTONS
                    var buttons = root.Query<Button>(keyName).Build();
                    foreach (var btn in buttons) btn.text = translatedText;

                    // 3. TOGGLES
                    var toggles = root.Query<Toggle>(keyName).Build();
                    foreach (var toggle in toggles) toggle.label = translatedText;

                    // 4. SLIDERS
                    var sliders = root.Query<Slider>(keyName).Build();
                    foreach (var slider in sliders) slider.label = translatedText;
                    
                    var slidersInt = root.Query<SliderInt>(keyName).Build();
                    foreach (var slider in slidersInt) slider.label = translatedText;

                    // 5. TEXT FIELDS
                    var textFields = root.Query<TextField>(keyName).Build();
                    foreach (var field in textFields) field.label = translatedText;

                    // 6. DROPDOWNS
                    var dropdowns = root.Query<DropdownField>(keyName).Build();
                    foreach (var dropdown in dropdowns) dropdown.label = translatedText;

                    // 7. GROUP BOXES (Headings)
                    var groupBoxes = root.Query<GroupBox>(keyName).Build();
                    foreach (var box in groupBoxes) box.text = translatedText;
                    
                    // 8. RADIO BUTTONS
                    var radios = root.Query<RadioButton>(keyName).Build();
                    foreach (var radio in radios) radio.label = translatedText;
                }
            }
        };
    }
}