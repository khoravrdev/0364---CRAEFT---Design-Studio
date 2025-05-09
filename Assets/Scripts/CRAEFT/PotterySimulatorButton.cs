using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System;

public class PotterySimulatorButton : MonoBehaviour
{
    public UIDocument uIDocument;

    private VisualElement    potterySimulatorButton;
    public WindowSwitcher windowSwitch;
    private VisualElement root;

    private VisualElement potterySimulatorWindow;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        root = uIDocument.rootVisualElement;
        potterySimulatorWindow = root.Q<VisualElement>("PotterySimulationWindow");

        // Find the BurgerButton in the UI
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
