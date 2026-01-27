using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

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

    private System.Collections.IEnumerator InitializeAndLoad()
    {
        yield return LocalizationSettings.InitializationOperation;
        UpdateUIText();
    }

    void OnLocaleChanged(Locale locale)
    {
        UpdateUIText();
    }

    void UpdateUIText()
    {
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;
        
        // Load the String Table
        var stringTableOp = LocalizationSettings.StringDatabase.GetTableAsync(tableCollectionName);
        
        stringTableOp.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                var table = op.Result;

                // Loop through SharedData to get the "Keys" (the names you gave your UI elements)
                foreach (var sharedEntry in table.SharedData.Entries)
                {
                    string keyName = sharedEntry.Key;
                    
                    // Get the translation for the current language using the ID
                    var entry = table.GetEntry(sharedEntry.Id);
                    
                    // FIX 1: Check if entry is null or the Value is empty
                    if (entry == null || string.IsNullOrEmpty(entry.Value)) continue;

                    // FIX 2: Use .Value to get the raw string directly
                    string translatedText = entry.Value;

                    // Update Labels
                    var label = root.Q<Label>(keyName);
                    if (label != null)
                    {
                        label.text = translatedText;
                    }
                    
                    // Update Buttons
                    var button = root.Q<Button>(keyName);
                    if (button != null)
                    {
                        button.text = translatedText;
                    }
                }
            }
            else
            {
                Debug.LogError($"Could not load String Table: {tableCollectionName}");
            }
        };
    }
}