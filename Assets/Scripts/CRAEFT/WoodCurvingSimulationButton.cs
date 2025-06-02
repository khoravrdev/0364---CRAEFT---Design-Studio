using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class WoodCurvingSimulationButton : MonoBehaviour
{
    public UIDocument uIDocument;

    private VisualElement woodCurvingSimulatorButton;
    public WindowSwitcher windowSwitch;
    private VisualElement root;

    private VisualElement woodCurvingSimulatorWindow;
    private VisualElement mainWindow;
    private VisualElement backgroundImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        root = uIDocument.rootVisualElement;
        woodCurvingSimulatorWindow = root.Q<VisualElement>("WoodCurvingSimulationWindow");
        mainWindow = root.Q<VisualElement>("MainWindow");
        backgroundImage = root.Q<VisualElement>("BackgroundImage");
        backgroundImage.SendToBack();
        backgroundImage.SendToBack();        
        backgroundImage.visible = true;
        
       woodCurvingSimulatorButton = root.Q<VisualElement>("WoodturningButton");
        if (woodCurvingSimulatorButton == null)
        {
            Debug.LogError("WoodCurvingButton not found in the UI.");
            return;
        }

        woodCurvingSimulatorButton.RegisterCallback<ClickEvent>(OnClickWoodCurvingButton);
    }

    private void OnClickWoodCurvingButton(ClickEvent evt)
    {
        SceneManager.LoadScene("Scene3", LoadSceneMode.Additive);
        windowSwitch.ShowWindow(woodCurvingSimulatorWindow);
        backgroundImage.style.height = Length.Percent(13);
        backgroundImage.style.maxHeight = Length.Percent(13);
        backgroundImage.style.minHeight = Length.Percent(13);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
