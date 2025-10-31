using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using UnityEngine.SceneManagement;
using MitsubaRendererLibrary;

public class WindowSwitcher : MonoBehaviour
{
    // Reference to the UI Document
    public UIDocument uiDocument;

    public GameObject photoBoothRenderCaller;

    // UI Windows (VisualElements)
    private VisualElement visualizationWindow;
    private VisualElement simulationsWindow;
    private VisualElement designStudioWindow;

    private VisualElement potterySimulatorWindow;

    private VisualElement woodCurvingSimulatorWindow;

    private VisualElement photoBoothToolWindow;

    private VisualElement exitButton;
    private VisualElement backgroundImage;
    // Button names (as defined in your UXML)
    private const string visualizationButtonName = "VisualizationToolboxButton";
    private const string simulationsButtonName = "SimulationsButton";
    private const string designStudioButtonName = "Menu_HomeButton"; // Button to open DesignStudioWindow

    private const string potterySimulatorButtonName = "PotteryButton"; // Button to open pottery simulator window

    private const string photoBoothButtonName = "PhotoBoothButton";

    private const string exitButtonName = "Menu_ExitButton";

    // Root VisualElement from the UIDocument
    private VisualElement root;

    void Start()
    {
        // Get the root VisualElement
        root = uiDocument.rootVisualElement;

        // Find the UI windows by their names
        visualizationWindow = root.Q<VisualElement>("VisualizationToolboxWindow");
        simulationsWindow = root.Q<VisualElement>("SimulationsWindow");
        designStudioWindow = root.Q<VisualElement>("DesignStudioWindow");
        potterySimulatorWindow = root.Q<VisualElement>("PotterySimulationWindow");
        woodCurvingSimulatorWindow = root.Q<VisualElement>("WoodCurvingSimulationWindow");
        photoBoothToolWindow = root.Q<VisualElement>("PhotoBoothSimulationWindow");
        backgroundImage = root.Q<VisualElement>("BackgroundImage");
        exitButton = root.Q<VisualElement>("Menu_ExitButton");

        // Query for all buttons with the given names (even if they share the same name but different parents)
        List<Button> visualizationButtons = root.Query<Button>(visualizationButtonName).ToList();
        List<Button> simulationsButtons = root.Query<Button>(simulationsButtonName).ToList();
        List<Button> designStudioButtons = root.Query<Button>(designStudioButtonName).ToList();

        // Register callbacks for each Visualization Toolbox button
        foreach (var btn in visualizationButtons)
        {
            btn.RegisterCallback<ClickEvent>(evt => ShowWindow(visualizationWindow));
        }

        // Register callbacks for each Simulations button
        foreach (var btn in simulationsButtons)
        {
            btn.RegisterCallback<ClickEvent>(evt => ShowWindow(simulationsWindow));
        }

        // Register callbacks for each Design Studio (home) button
        foreach (var btn in designStudioButtons)
        {
            btn.RegisterCallback<ClickEvent>(evt => ShowWindow(designStudioWindow));
        }

        exitButton.RegisterCallback<ClickEvent>(evt => Application.Quit());

        // Optionally, show a default window (e.g., DesignStudioWindow)
        ShowWindow(designStudioWindow);

    }

    // This method hides all windows and then shows the specified window.
    public void ShowWindow(VisualElement windowToShow)
    {
        if (visualizationWindow != null)
            visualizationWindow.style.display = DisplayStyle.None;

        if (simulationsWindow != null)
            simulationsWindow.style.display = DisplayStyle.None;

        if (designStudioWindow != null)
            designStudioWindow.style.display = DisplayStyle.None;
        
        if (potterySimulatorWindow != null)
            potterySimulatorWindow.style.display = DisplayStyle.None;
        if (woodCurvingSimulatorWindow != null)
            woodCurvingSimulatorWindow.style.display = DisplayStyle.None;
        if (photoBoothToolWindow != null)
            photoBoothToolWindow.style.display = DisplayStyle.None;

        if (windowToShow != null)
            {
                if (windowToShow != potterySimulatorWindow)
                {
                    if (SceneManager.GetSceneByName("Scene").isLoaded)
                    {
                        SceneManager.UnloadSceneAsync("Scene");
                        backgroundImage.style.height = Length.Percent(100);
                        backgroundImage.style.maxHeight = Length.Percent(100);
                        backgroundImage.style.minHeight = Length.Percent(100);
                    }

                }
                if (windowToShow != woodCurvingSimulatorWindow)
                {
                    if (SceneManager.GetSceneByName("Scene3").isLoaded)
                    {
                        SceneManager.UnloadSceneAsync("Scene3");
                        backgroundImage.style.height = Length.Percent(100);
                        backgroundImage.style.maxHeight = Length.Percent(100);
                        backgroundImage.style.minHeight = Length.Percent(100);
                    }
                }
                if (windowToShow != photoBoothToolWindow)
                {
                    if (SceneManager.GetSceneByName("SampleScene").isLoaded)
                    {                     
                        MitsubaRunner.RequestHardKill();
                        SceneManager.UnloadSceneAsync("SampleScene");
                        backgroundImage.style.height = Length.Percent(100);
                        backgroundImage.style.maxHeight = Length.Percent(100);
                        backgroundImage.style.minHeight = Length.Percent(100);
                    }
                }
                windowToShow.style.display = DisplayStyle.Flex;
            }
    }


}
