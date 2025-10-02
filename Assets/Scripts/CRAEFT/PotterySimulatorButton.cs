using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;

public class PotterySimulatorButton : MonoBehaviour
{
    public UIDocument uIDocument;

    private VisualElement potterySimulatorButton;
    public WindowSwitcher windowSwitch;
    private VisualElement root;

    private VisualElement potterySimulatorWindow;
    private VisualElement mainWindow;
    private VisualElement backgroundImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        root = uIDocument.rootVisualElement;
        potterySimulatorWindow = root.Q<VisualElement>("PotterySimulationWindow");
        mainWindow = root.Q<VisualElement>("MainWindow");
        backgroundImage = root.Q<VisualElement>("BackgroundImage");
        backgroundImage.SendToBack();
        backgroundImage.SendToBack();        
        backgroundImage.visible = true;
        
       potterySimulatorButton = root.Q<VisualElement>("PotteryButton");
        if (potterySimulatorButton == null)
        {
            Debug.LogError("PotteryButton not found in the UI.");
            return;
        }

        potterySimulatorButton.RegisterCallback<ClickEvent>(OnClickPotteryButton);
    }

    private void OnClickPotteryButton(ClickEvent evt)
    {
        SceneManager.LoadScene("Scene", LoadSceneMode.Additive);
        windowSwitch.ShowWindow(potterySimulatorWindow);
        backgroundImage.style.height = Length.Percent(13);
        backgroundImage.style.maxHeight = Length.Percent(13);
        backgroundImage.style.minHeight = Length.Percent(13);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
