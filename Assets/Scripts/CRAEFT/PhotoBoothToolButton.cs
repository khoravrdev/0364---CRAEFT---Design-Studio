using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PhotoBoothToolButton : MonoBehaviour
{
    public UIDocument uIDocument;

    private VisualElement photoBoothToolButton;
    public WindowSwitcher windowSwitch;
    private VisualElement root;

    private VisualElement photoBoothToolWindow;
    private VisualElement mainWindow;
    private VisualElement backgroundImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        root = uIDocument.rootVisualElement;
        photoBoothToolWindow = root.Q<VisualElement>("PhotoBoothSimulationWindow");
        mainWindow = root.Q<VisualElement>("MainWindow");
        backgroundImage = root.Q<VisualElement>("BackgroundImage");
        backgroundImage.SendToBack();
        backgroundImage.SendToBack();
        backgroundImage.SendToBack();
        backgroundImage.visible = true;

        photoBoothToolButton = root.Q<VisualElement>("PhotoBoothButton");
        if (photoBoothToolButton == null)
        {
            Debug.LogError("PotteryButton not found in the UI.");
            return;
        }

        photoBoothToolButton.RegisterCallback<ClickEvent>(OnClickPhotoBoothButton);
        uIDocument.rootVisualElement.RegisterCallback<PointerDownEvent>(e => Debug.Log("Clicked" + e.target));
    }

    private void OnClickPhotoBoothButton(ClickEvent evt)
    {
        SceneManager.LoadScene("SampleScene", LoadSceneMode.Additive);
        windowSwitch.ShowWindow(photoBoothToolWindow);
        backgroundImage.style.height = Length.Percent(0);
        backgroundImage.style.maxHeight = Length.Percent(0);
        backgroundImage.style.minHeight = Length.Percent(0);
        // Find the generated PanelSettings GameObject under the EventSystem
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        var panelSettingsObject = GameObject.Find("New Panel Settings");

        if (panelSettingsObject != null)
        {
            // Remove the components that are causing the conflict
            Destroy(panelSettingsObject.GetComponent<PanelEventHandler>());
            Destroy(panelSettingsObject.GetComponent<PanelRaycaster>());
            Destroy(panelSettingsObject); // Remove the PanelSettings GameObject entirely
        }
        */
    }
}
