using System.ComponentModel.Design.Serialization;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Collections;

public class PotterySimulatorToolUIConnector : MonoBehaviour
{
    public UIDocument uIDocument;
    private VisualElement subtractiveToolButton;
    private VisualElement additiveToolButton;
    private VisualElement massPreservingToolButton;

    private Toggle massPreservingToggle;

    private VisualElement resetObjectButton;

    private VisualElement cameraOrbitingButton;
    private VisualElement subtractiveMultiToolButton;
    private VisualElement additiveMultiToolButton;

    //Load template button and drop menu
    private Button loadTemplateButton;
    private DropdownField loadListTemplates;
    private PopupField<string> loadListTemplatesPopup;  
    //Save File button and text input
    private VisualElement saveTemplateButton;
    private TextField saveTemplateText;

    //Turntable buttons and slider
    // ✱ CHANGED: use concrete types so we can reset without firing events
    private Toggle turnTableToggle;      // ✱ CHANGED
    private Slider turnTableSpeed;       // ✱ CHANGED

    //Undo Button
    private VisualElement undoButton;
    private VisualElement root;

    //Triangle and square tools
    private Button triangleTool;
    private Button squareTool;

    //Axis visual elements
    public VisualElement groupAxis;
    public VisualElement xAxis;
    public VisualElement yAxis;
    public VisualElement zAxis;
    public int toolIndex;

    //Help bar text
    public Label helpPanelText;
    public string useToolTipText;
    public GameObject mainScript;
    public GameObject camera;

    // ✱ CHANGED: track init and allow safe re-init
    private bool _initialized = false;   // ✱ CHANGED
    private bool _lateAssertQueued = false; // ✱ CHANGED
    // ✱ CHANGED: remember which template was last loaded
    private string lastLoadedTemplate;               
    private const string LastTemplateKey = "Pottery.LastTemplate";  // for persistence between scene reloads



    // ✱ CHANGED: lifecycle hooks to (un)register and reset safely
    private void OnEnable() // ✱ CHANGED
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // rebind if CRAEFTUI loads later
        TryInit(); // attempt to init now if CRAEFTUI already loaded
    }

    private void OnDisable() // ✱ CHANGED
    {
        UnregisterCallbacksAndClear(); // break dangling delegates
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy() // ✱ CHANGED
    {
        UnregisterCallbacksAndClear();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) // ✱ CHANGED
    {
        if (!_initialized) TryInit();
    }

    // Start kept but empty to avoid double-run; original logic moved to TryInit
    // ✱ CHANGED: Run the dropdown population after a small delay if needed
    private void Start()
    {
        StartCoroutine(WaitForSceneAndPopulate());
    }

    private IEnumerator WaitForSceneAndPopulate()
    {
        yield return null;  // wait a frame for initialization
        PopulateDropdown(); // now safely populate
    }

    // ✱ CHANGED: factor original Start() setup into here, with guards and reset
    private void TryInit() // ✱ CHANGED
    {
        if (_initialized && uIDocument != null) return; // already good

        mainScript = GameObject.Find("Solid"); // original line
        if (camera == null) camera = GameObject.Find("Main Camera"); // ✱ CHANGED: ensure camera

        if (!SceneManager.GetSceneByName("CRAEFTUI").isLoaded) return;

        foreach (var obj in SceneManager.GetSceneByName("CRAEFTUI").GetRootGameObjects())
        {
            if (obj.name == "UIDocument")
            {
                uIDocument = obj.GetComponent<UIDocument>();
            }
        }
        if (uIDocument == null) return;

        root = uIDocument.rootVisualElement;
        if (root == null) return;

        subtractiveToolButton = root.Q<Button>("SubtractTool");
        additiveToolButton = root.Q<Button>("AdditionTool");
        massPreservingToolButton = root.Q<Button>("MassPreservingTool");
        resetObjectButton = root.Q<Button>("ResetButton");
        cameraOrbitingButton = root.Q<Button>("OrbitingCameraButton");
        subtractiveMultiToolButton = root.Q<Button>("SubtractiveMultiTool");
        additiveMultiToolButton = root.Q<Button>("AdditiveMultiTool");
        massPreservingToggle = root.Q<Toggle>("MassPreservingToggle");
        undoButton = root.Q<Button>("UndoButton");

        saveTemplateButton = root.Q<Button>("SaveTemplate");
        saveTemplateText = root.Q<TextField>("SavedFileName");

        loadTemplateButton = root.Q<Button>("LoadTemplate");
        loadListTemplates = root.Q<DropdownField>("ListTemplates");
        // ✱ CHANGED: fallback if it’s actually a PopupField<string> or the name differs slightly
        if (loadListTemplates == null)
        {
            loadListTemplatesPopup = root.Q<PopupField<string>>("ListTemplates");
            if (loadListTemplatesPopup == null)
            {
                // last-ditch: grab the first DropdownField/PopupField in the tree if there’s only one
                loadListTemplates = root.Q<DropdownField>();
                if (loadListTemplates == null)
                    loadListTemplatesPopup = root.Q<PopupField<string>>();
            }
        }

        turnTableSpeed = root.Q<Slider>("TurntableSpeedSlider");     // ✱ CHANGED
        turnTableToggle = root.Q<Toggle>("ToggleTurntableAnimation"); // ✱ CHANGED

        triangleTool = root.Q<Button>("TriangleToolTip");
        squareTool = root.Q<Button>("SquareToolTip");

        groupAxis = root.Q<VisualElement>("AxisGroup");
        xAxis = root.Q<VisualElement>("XAxis");
        yAxis = root.Q<VisualElement>("YAxis");
        zAxis = root.Q<VisualElement>("ZAxis");

        groupAxis.visible = false;

        helpPanelText = root.Q<Label>("HelpPanelText");
        toolIndex = 0;

        // --- register callbacks (named methods where needed so we can Unregister later) ---
        if (turnTableSpeed != null) turnTableSpeed.RegisterCallback<FocusEvent>(OnTurntableSliderFocus); // ✱ CHANGED
        if (root != null) root.RegisterCallback<PointerUpEvent>(OnRootPointerUp);              // ✱ CHANGED

        subtractiveToolButton?.RegisterCallback<ClickEvent>(OnClickSubtractiveToolButton);
        additiveToolButton?.RegisterCallback<ClickEvent>(OnClickAdditiveToolButton);
        //massPreservingToolButton?.RegisterCallback<ClickEvent>(OnClickMassPreservingToolButton);
        resetObjectButton?.RegisterCallback<ClickEvent>(OnClickResetButton);
        cameraOrbitingButton?.RegisterCallback<ClickEvent>(OnClickCameraOrbitingButton);

        undoButton?.RegisterCallback<ClickEvent>(OnClickUndoButton);

        saveTemplateButton?.RegisterCallback<ClickEvent>(OnClickSaveTemplateButton);
        //saveTemplateText?.RegisterCallback<ChangeEvent<string>>(OnSaveTemplateTextChange);

        //loadTemplateButton?.RegisterCallback<ClickEvent>(OnClickLoadTemplateButton);
        loadListTemplates?.RegisterCallback<ChangeEvent<string>>(OnDropDownListChanged);

        turnTableToggle?.RegisterCallback<ChangeEvent<bool>>(OnTurntableValueChange);
        turnTableSpeed?.RegisterCallback<ChangeEvent<float>>(OnTurntableSpeedChange);

        triangleTool?.RegisterCallback<ClickEvent>(OnClickTriangleToolButton);
        squareTool?.RegisterCallback<ClickEvent>(OnClickSquareToolButton);

        massPreservingToggle?.RegisterCallback<ChangeEvent<bool>>(OnMassPreservingValueChange);

        PopulateDropdown();

        // ✱ CHANGED: reset UI to starting condition (camera orbit ON etc.) without firing events
        ResetUIState(); // ✱ CHANGED

        // Re-assert defaults once at end of frame in case another script flips them. // ✱ CHANGED
        if (!_lateAssertQueued)
        {
            _lateAssertQueued = true;
            StartCoroutine(LateAssertDefaults());
        }


        _initialized = true;
    }
    
    private System.Collections.IEnumerator LateAssertDefaults() // ✱ CHANGED
    {
        yield return null; // wait one frame

        // Re-assert orbit ON (matches the button’s grey visual)
        if (camera != null)
        {
            var orbit = camera.GetComponent<OrbitCamera>();
            if (orbit != null) orbit.enableCameraMode = true;
        }
        if (cameraOrbitingButton != null)
            cameraOrbitingButton.style.backgroundColor = new StyleColor(Color.gray);

        // Re-assert turntable OFF to match the toggle // ✱ CHANGED
        if (turnTableToggle != null) turnTableToggle.SetValueWithoutNotify(false);
        if (turnTableSpeed != null)  turnTableSpeed.visible = false;

        if (mainScript != null)
        {
            var s = mainScript.GetComponent<Script>();
            if (s != null) s.m_turntableOn = false; // authoritative off
        }
    }


    // ✱ CHANGED: cleanly unregister everything and clear references to avoid stale delegates on unload
    private void UnregisterCallbacksAndClear() // ✱ CHANGED
    {
        if (!_initialized) return;

        // unregister specific ones we added
        if (turnTableSpeed != null) turnTableSpeed.UnregisterCallback<FocusEvent>(OnTurntableSliderFocus);
        if (root != null)           root.UnregisterCallback<PointerUpEvent>(OnRootPointerUp);

        subtractiveToolButton?.UnregisterCallback<ClickEvent>(OnClickSubtractiveToolButton);
        additiveToolButton?.UnregisterCallback<ClickEvent>(OnClickAdditiveToolButton);
        //massPreservingToolButton?.UnregisterCallback<ClickEvent>(OnClickMassPreservingToolButton);
        resetObjectButton?.UnregisterCallback<ClickEvent>(OnClickResetButton);
        cameraOrbitingButton?.UnregisterCallback<ClickEvent>(OnClickCameraOrbitingButton);

        undoButton?.UnregisterCallback<ClickEvent>(OnClickUndoButton);

        saveTemplateButton?.UnregisterCallback<ClickEvent>(OnClickSaveTemplateButton);
        //saveTemplateText?.UnregisterCallback<ChangeEvent<string>>(OnSaveTemplateTextChange);

        //loadTemplateButton?.UnregisterCallback<ClickEvent>(OnClickLoadTemplateButton);
        loadListTemplates?.UnregisterCallback<ChangeEvent<string>>(OnDropDownListChanged);

        turnTableToggle?.UnregisterCallback<ChangeEvent<bool>>(OnTurntableValueChange);
        turnTableSpeed?.UnregisterCallback<ChangeEvent<float>>(OnTurntableSpeedChange);

        triangleTool?.UnregisterCallback<ClickEvent>(OnClickTriangleToolButton);
        squareTool?.UnregisterCallback<ClickEvent>(OnClickSquareToolButton);

        massPreservingToggle?.UnregisterCallback<ChangeEvent<bool>>(OnMassPreservingValueChange);

        // clear refs so GC can collect and we don't keep stale objects
        uIDocument = null;
        root = null;
        subtractiveToolButton = null;
        additiveToolButton = null;
        massPreservingToolButton = null;
        resetObjectButton = null;
        cameraOrbitingButton = null;
        subtractiveMultiToolButton = null;
        additiveMultiToolButton = null;
        massPreservingToggle = null;
        undoButton = null;
        saveTemplateButton = null;
        saveTemplateText = null;
        loadTemplateButton = null;
        loadListTemplates = null;
        turnTableToggle = null;
        turnTableSpeed = null;
        triangleTool = null;
        squareTool = null;
        groupAxis = null;
        xAxis = yAxis = zAxis = null;
        helpPanelText = null;

        _initialized = false;
    }

    // ✱ CHANGED: these are the named versions of your inline lambdas so we can unregister them
    private void OnTurntableSliderFocus(FocusEvent evt) // ✱ CHANGED
    {
        if (camera != null) camera.GetComponent<OrbitCamera>().enableCameraMode = false;
    }

    private void OnRootPointerUp(PointerUpEvent evt) // ✱ CHANGED
    {
        if (camera != null)
        {
            var orbit = camera.GetComponent<OrbitCamera>();
            if (orbit != null && orbit.enableCameraMode == false)
            {
                if (cameraOrbitingButton != null && cameraOrbitingButton.style.unityBackgroundImageTintColor == Color.gray)
                {
                    orbit.enableCameraMode = true;
                }
            }
        }
    }

    // ✱ CHANGED: reset visuals and toggles to your starting condition WITHOUT firing events
    private void ResetUIState()
    {
        // Orbit ON + grey button (visual & logic)                        // (unchanged)
        if (camera != null)
        {
            var orbit = camera.GetComponent<OrbitCamera>();
            if (orbit != null) orbit.enableCameraMode = true;
        }
        if (cameraOrbitingButton != null)
            cameraOrbitingButton.style.backgroundColor = new StyleColor(Color.gray);

        // Mass-preserving OFF (silent)
        massPreservingToggle?.SetValueWithoutNotify(false);

        // Turntable OFF + hide slider (silent + logic)                    // ✱ CHANGED
        if (turnTableToggle != null) turnTableToggle.SetValueWithoutNotify(false);
        if (turnTableSpeed != null)  turnTableSpeed.visible = false;

        if (mainScript != null)                                          // ✱ CHANGED
        {
            var s = mainScript.GetComponent<Script>();
            if (s != null) s.m_turntableOn = false; // ensure underlying sim is off
        }

        // Reset tool button visuals (typed API)
        if (subtractiveToolButton != null)      subtractiveToolButton.style.backgroundColor = new StyleColor(Color.white);
        if (additiveToolButton != null)         additiveToolButton.style.backgroundColor = new StyleColor(Color.white);
        if (massPreservingToolButton != null)   massPreservingToolButton.style.backgroundColor = new StyleColor(Color.white);
        if (triangleTool != null)               triangleTool.style.backgroundColor = new StyleColor(Color.white);
        if (squareTool != null)                 squareTool.style.backgroundColor = new StyleColor(Color.white);

        // Hide axis + clear help (unchanged)
        if (groupAxis != null) groupAxis.visible = false;
        if (helpPanelText != null) helpPanelText.text = "";

        // Park tools & disable (null-safe) (unchanged if you already had this)
        if (mainScript != null)
        {
            var s = mainScript.GetComponent<Script>();
            if (s != null)
            {
                if (s.m_additiveTool != null)        { s.m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f); s.m_additiveTool.SetActive(false); }
                if (s.m_subtractiveTool != null)     { s.m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f); s.m_subtractiveTool.SetActive(false); }
                if (s.m_massPreservingTool != null)  { s.m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f); s.m_massPreservingTool.SetActive(false); }
                if (s.triangleToolFullObject != null){ s.triangleToolFullObject.transform.position = new Vector3(1000f, 0f, 0f); }
                if (s.squareToolFullObject != null)  { s.squareToolFullObject.transform.position = new Vector3(1000f, 0f, 0f); }

                // Only call SetActive on m_triangleTool/m_squareTool if they’re assigned // ✱ CHANGED
                if (s.m_triangleTool != null) s.m_triangleTool.SetActive(false);
                if (s.m_squareTool != null)   s.m_squareTool.SetActive(false);
            }
        }

        toolIndex = 0;
    }


    // ------------------- ORIGINAL HANDLERS BELOW (unchanged) -------------------

    private void OnClickTriangleToolButton(ClickEvent evt)
    {
        if (mainScript != null)
        {
            Debug.Log("Clicked triangle tool");
            triangleTool.style.backgroundColor = new StyleColor(Color.gray);
            squareTool.style.backgroundColor = new StyleColor(Color.white);

            mainScript.GetComponent<Script>().triangleToolFullObject.SetActive(true);

            mainScript.GetComponent<Script>().triangleToolFullObject.SetActive(true);
            mainScript.GetComponent<Script>().squareToolFullObject.SetActive(false);
            mainScript.GetComponent<Script>().squareToolFullObject.transform.position = new Vector3(-1.0f, 0f, 0f);
            mainScript.GetComponent<Script>().squareToolFullObject.transform.rotation = Quaternion.Euler(0, 90, 0);

            mainScript.GetComponent<Script>().triangleToolFullObject.transform.position = new Vector3(-1.0f, 0f, 0f);
            mainScript.GetComponent<Script>().triangleToolFullObject.transform.rotation = Quaternion.Euler(0, 90, 0);

            subtractiveToolButton.style.backgroundColor = new StyleColor(Color.white);
            additiveToolButton.style.backgroundColor = new StyleColor(Color.white);

            mainScript.GetComponent<Script>().EnableToolControls(mainScript.GetComponent<Script>().triangleToolFullObject);
            mainScript.GetComponent<Script>().DisableToolControl(mainScript.GetComponent<Script>().squareToolFullObject);

            yAxis.style.backgroundColor = new StyleColor(Color.white);
            xAxis.style.backgroundColor = new StyleColor(Color.white);
            zAxis.style.backgroundColor = new StyleColor(Color.white);

            mainScript.GetComponent<Script>().m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f);

            helpPanelText.text = useToolTipText;
            groupAxis.visible = true;

            toolIndex = 4;
        }
    }

    private void OnClickSquareToolButton(ClickEvent evt)
    {
        if (mainScript != null)
        {
            Debug.Log("Clicked square tool");
            triangleTool.style.backgroundColor = new StyleColor(Color.white);
            squareTool.style.backgroundColor = new StyleColor(Color.grey);
            mainScript.GetComponent<Script>().triangleToolFullObject.SetActive(false);
            mainScript.GetComponent<Script>().squareToolFullObject.SetActive(true);
            mainScript.GetComponent<Script>().triangleToolFullObject.transform.position = new Vector3(-1.0f, 0f, 0f);
            mainScript.GetComponent<Script>().triangleToolFullObject.transform.rotation = Quaternion.Euler(0, 90, 0);

            mainScript.GetComponent<Script>().squareToolFullObject.transform.position = new Vector3(-1.0f, 0f, 0f);
            mainScript.GetComponent<Script>().squareToolFullObject.transform.rotation = Quaternion.Euler(0, 90, 0);

            subtractiveToolButton.style.backgroundColor = new StyleColor(Color.white);
            additiveToolButton.style.backgroundColor = new StyleColor(Color.white);

            mainScript.GetComponent<Script>().EnableToolControls(mainScript.GetComponent<Script>().squareToolFullObject);
            mainScript.GetComponent<Script>().DisableToolControl(mainScript.GetComponent<Script>().triangleToolFullObject);

            yAxis.style.backgroundColor = new StyleColor(Color.white);
            xAxis.style.backgroundColor = new StyleColor(Color.white);
            zAxis.style.backgroundColor = new StyleColor(Color.white);

            mainScript.GetComponent<Script>().m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f);

            helpPanelText.text = useToolTipText;
            groupAxis.visible = true;
            toolIndex = 5;
        }
    }

   private void ApplySelectedTemplate(string fileName)
    {
        if (string.IsNullOrEmpty(fileName) || mainScript == null)
            return;

        var s = mainScript.GetComponent<Script>();
        if (s == null || s.m_generator == null)
            return;

        string fullPath = Path.Combine(Application.streamingAssetsPath, fileName);
        s.m_generator.setTemplate(fullPath);
        s.updateTexturingShader();
        Debug.Log("[PotteryUI] Loaded template: " + fullPath);

        // remember the selected template and save it to PlayerPrefs
        PlayerPrefs.SetString(LastTemplateKey, fileName);
    }

    // ✱ CHANGED: helper to get the current loaded template name from generator
    private string GetCurrentTemplateName()  // ✱ NEW
    {
        if (mainScript == null) return null;
        var s = mainScript.GetComponent<Script>();
        if (s == null || s.m_generator == null) return null;

        string path = s.m_generator.getTemplate(); // <-- if you have this method
        if (string.IsNullOrEmpty(path)) return null;

        return Path.GetFileName(path);
    }
    
    private void OnDropDownListChanged(ChangeEvent<string> evt)
    {
        if (evt == null || string.IsNullOrEmpty(evt.newValue))
            return;

        ApplySelectedTemplate(evt.newValue); // this also saves PlayerPrefs
    }
    private void PopulateDropdown()
    {
        if (loadListTemplates == null && loadListTemplatesPopup == null)
        {
            Debug.LogWarning("PopulateDropdown: no DropdownField or PopupField found for 'ListTemplates'.");
            return;
        }

        string[] files = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath));
        List<string> fileNames = new List<string>();
        foreach (var file in files)
        {
            if (Path.GetExtension(file).Equals(".png", System.StringComparison.OrdinalIgnoreCase))
                fileNames.Add(Path.GetFileName(file));
        }

        // ✱ CHANGED: pick the last loaded template if available
        string savedTemplate = PlayerPrefs.GetString(LastTemplateKey, null);

        // Check if the saved template exists and is valid
        string selectedTemplate = null;
        if (!string.IsNullOrEmpty(savedTemplate) && fileNames.Contains(savedTemplate))
        {
            selectedTemplate = savedTemplate;
        }
        else if (fileNames.Count > 0)
        {
            selectedTemplate = fileNames[0]; // default to first file if nothing saved
        }

        // ✱ CHANGED: update the dropdown first with the selected template
        if (loadListTemplates != null)
        {
            loadListTemplates.choices = fileNames;
            loadListTemplates.SetValueWithoutNotify(selectedTemplate);  // Update the dropdown label
        }
        else if (loadListTemplatesPopup != null)
        {
            var container = loadListTemplatesPopup.parent;
            int index = container.IndexOf(loadListTemplatesPopup);
            container.RemoveAt(index);

            var newPopup = new PopupField<string>(fileNames, selectedTemplate);
            newPopup.name = "ListTemplates";
            newPopup.RegisterCallback<ChangeEvent<string>>(OnDropDownListChanged);
            container.Insert(index, newPopup);
            loadListTemplatesPopup = newPopup;
        }

        // ✱ CHANGED: Now apply the template after the dropdown value is set
        ApplySelectedTemplate(selectedTemplate);
    }


    private void OnClickSaveTemplateButton(ClickEvent evt)
    {
        if (mainScript != null)
        {
            if (!string.IsNullOrWhiteSpace(saveTemplateText.value))
            {
                mainScript.GetComponent<Script>().m_generator.saveAsTemplate(Application.streamingAssetsPath + "/" + saveTemplateText.value + ".png");
                Debug.Log("Save template text:" + saveTemplateText.value);
                PopulateDropdown();

                saveTemplateText.value = "";
            }
        }
    }

    private void OnClickUndoButton(ClickEvent evt)
    {
        if (mainScript != null)
        {
            Debug.Log("Undo button pressed");
            mainScript.GetComponent<Script>().m_generator.popUndo();
            mainScript.GetComponent<Script>().m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().triangleToolFullObject.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().squareToolFullObject.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().m_additiveTool.SetActive(false);
            mainScript.GetComponent<Script>().m_subtractiveTool.SetActive(false);
            mainScript.GetComponent<Script>().m_massPreservingTool.SetActive(false);
            //mainScript.GetComponent<Script>().m_triangleTool.SetActive(false);
            //mainScript.GetComponent<Script>().m_squareTool.SetActive(false);
            groupAxis.visible = false;
            helpPanelText.text = "";
            var s = mainScript.GetComponent<Script>();
            if (s != null)
            {
                if (s.m_triangleTool != null) s.m_triangleTool.SetActive(false);
                if (s.m_squareTool != null)   s.m_squareTool.SetActive(false);
            }
        }
    }

    private void OnTurntableValueChange(ChangeEvent<bool> evt)
    {
        if (mainScript != null)
        {
            if (evt.newValue == true)
            {
                turnTableSpeed.visible = true;
                mainScript.GetComponent<Script>().m_turntableOn = evt.newValue;
            }
            else if (evt.newValue == false)
            {
                turnTableSpeed.visible = false;
                mainScript.GetComponent<Script>().m_turntableOn = evt.newValue;
            }
        }
    }

    private void OnTurntableSpeedChange(ChangeEvent<float> evt)
    {
        if (mainScript != null)
        {
            mainScript.GetComponent<Script>().m_turntableSpeed = evt.newValue;
        }
    }

    private void OnClickCameraOrbitingButton(ClickEvent evt)
    {
        if (mainScript != null)
        {
            Debug.Log("Clicked Camera Orbiting");
            if (camera.GetComponent<OrbitCamera>().enableCameraMode == false)
            {
                camera.GetComponent<OrbitCamera>().enableCameraMode = true;
                cameraOrbitingButton.style.backgroundColor = new StyleColor(Color.gray);
                subtractiveToolButton.style.backgroundColor = new StyleColor(Color.white);
                additiveToolButton.style.backgroundColor = new StyleColor(Color.white);
                massPreservingToggle.value = false;
                mainScript.GetComponent<Script>().m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f);
                mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);
                mainScript.GetComponent<Script>().m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f);
                mainScript.GetComponent<Script>().m_additiveTool.SetActive(false);
                mainScript.GetComponent<Script>().m_subtractiveTool.SetActive(false);
                mainScript.GetComponent<Script>().m_massPreservingTool.SetActive(false);
            }
        }
    }

    private void OnClickSubtractiveToolButton(ClickEvent evt)
    {
        if (mainScript != null)
        {
            Debug.Log("Clicked Sub tool");
            subtractiveToolButton.style.backgroundColor = new StyleColor(Color.gray);
            additiveToolButton.style.backgroundColor = new StyleColor(Color.white);
            triangleTool.style.backgroundColor = new StyleColor(Color.white);
            squareTool.style.backgroundColor = new StyleColor(Color.white);
            massPreservingToggle.value = false;
            mainScript.GetComponent<Script>().m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().triangleToolFullObject.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().squareToolFullObject.transform.position = new Vector3(1000f, 0f, 0f);
            groupAxis.visible = false;
            helpPanelText.text = "";
            camera.GetComponent<OrbitCamera>().enableCameraMode = false;
            cameraOrbitingButton.style.backgroundColor = new StyleColor(Color.white);
            toolIndex = 1;
        }
    }

    private void OnClickAdditiveToolButton(ClickEvent evt)
    {
        if (mainScript != null)
        {
            Debug.Log("Clicked Add tool");
            toolIndex = 2;
            additiveToolButton.style.backgroundColor = new StyleColor(Color.gray);
            subtractiveToolButton.style.backgroundColor = new StyleColor(Color.white);
            triangleTool.style.backgroundColor = new StyleColor(Color.white);
            squareTool.style.backgroundColor = new StyleColor(Color.white);
            massPreservingToggle.value = false;
            mainScript.GetComponent<Script>().m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().triangleToolFullObject.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().squareToolFullObject.transform.position = new Vector3(1000f, 0f, 0f);
            groupAxis.visible = false;
            helpPanelText.text = "";
            camera.GetComponent<OrbitCamera>().enableCameraMode = false;
            cameraOrbitingButton.style.backgroundColor = new StyleColor(Color.white);
        }
    }

    private void OnMassPreservingValueChange(ChangeEvent<bool> evt)
    {
        if (mainScript != null)
        {
            if (evt.newValue == true)
            {
                Debug.Log("Clicked Mass tool");
                toolIndex = 3;

                additiveToolButton.style.backgroundColor = new StyleColor(Color.white);
                subtractiveToolButton.style.backgroundColor = new StyleColor(Color.white);

                mainScript.GetComponent<Script>().m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f);
                mainScript.GetComponent<Script>().m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f);
                mainScript.GetComponent<Script>().triangleToolFullObject.transform.position = new Vector3(1000f, 0f, 0f);
                mainScript.GetComponent<Script>().squareToolFullObject.transform.position = new Vector3(1000f, 0f, 0f);
                groupAxis.visible = false;
                helpPanelText.text = "";
                camera.GetComponent<OrbitCamera>().enableCameraMode = false;
                cameraOrbitingButton.style.backgroundColor = new StyleColor(Color.white);
            }
            else
            {
                mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);
            }
        }
    }

    private void OnClickResetButton(ClickEvent evt)
    {
        if (mainScript != null)
        {
            mainScript.GetComponent<Script>().m_generator.restart();
            mainScript.GetComponent<Script>().m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().triangleToolFullObject.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().squareToolFullObject.transform.position = new Vector3(1000f, 0f, 0f);
        }
    }
}
