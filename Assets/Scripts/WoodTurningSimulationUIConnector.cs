using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class WoodTurningSimulationUIConnector : MonoBehaviour
{
    public UIDocument uIDocument;
    private VisualElement root;

    //Camera orbit button
    private Button orbitCameraButton;

    //Chisel button
    private Button chiselButton;

    //Axis visual elementes
    private VisualElement groupAxis;
    public VisualElement xAxis;
    public VisualElement yAxis;
    public VisualElement zAxis;

    //Turntable variables    
    private Toggle turnTableToggle;          // ✱ CHANGED
    private Slider turnTableSpeed;           // ✱ CHANGED

    //Help bar text
    public Label helpPanel;
    public string woodTurningToolTipText;

    //Main Camera
    public GameObject camera;

    //Load template button and drop menu
    private Button loadSolidButton;
    private DropdownField loadListSolids;
    //Save File button and text input
    private VisualElement saveSolidButton;
    public TextField saveSolidText;
    string selectedFile;
    //Export obj
    private Button exportObjButton;

    //Reset object
    private Button resetObjectButton;

    // ✱ CHANGED: keep an init flag so we don’t double-register
    private bool _initialized = false;

    // ✱ CHANGED: cache Scene3 once
    private Scene3 _scene3;

    // ✱ CHANGED: move initialization out of Start and make it callable
    private void Awake()
    {
        _scene3 = GetComponent<Scene3>(); // may be null in editor if not present, that’s ok
    }

    // ✱ CHANGED: register sceneLoaded so we can (re)bind when CRAEFTUI loads
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryInitializeUIAndRegisterCallbacks();
        Debug.Log($"UI refs: chisel={chiselButton != null}, orbit={orbitCameraButton != null}, toggle={turnTableToggle != null}, slider={turnTableSpeed != null}");
        uIDocument.rootVisualElement.Blur();

        if (saveSolidText != null)
        {
            // When the user presses Enter/Return, release focus (Blur)
           saveSolidText.RegisterCallback<KeyDownEvent>(evt => 
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    saveSolidText.Blur(); // Stop typing
                }
            });
        }
    }

    // ✱ CHANGED: always unregister to break dangling delegates
    private void OnDisable()
    {
        UnregisterCallbacksAndClearRefs();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ✱ CHANGED: extra safety
    private void OnDestroy()
    {
        UnregisterCallbacksAndClearRefs();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ✱ CHANGED: keep Start empty or remove it entirely (optional)
    void Start() { }

    // ✱ CHANGED: scene-loaded hook in case CRAEFTUI is loaded/reloaded later
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!_initialized) TryInitializeUIAndRegisterCallbacks();
    }

    // ✱ CHANGED: new method that (re)binds UI and registers callbacks
    private void TryInitializeUIAndRegisterCallbacks()
    {
        if (_initialized) return;

        var uiScene = SceneManager.GetSceneByName("CRAEFTUI");
        if (!uiScene.isLoaded) return;

        // Find UIDocument from CRAEFTUI scene
        foreach (var obj in uiScene.GetRootGameObjects())
        {
            if (obj.name == "UIDocument")
            {
                uIDocument = obj.GetComponent<UIDocument>();
                break;
            }
        }

        if (uIDocument == null) return;

        // Refresh camera ref each enable (avoid stale object)
        camera = GameObject.Find("Main Camera");

        root = uIDocument.rootVisualElement;
        if (root == null) return;

        // Initiallize Visual Elements
        groupAxis = root.Q<VisualElement>("WoodAxisGroup");
        xAxis = root.Q<VisualElement>("XAxisWood");
        yAxis = root.Q<VisualElement>("YAxisWood");
        zAxis = root.Q<VisualElement>("ZAxisWood");
        orbitCameraButton = root.Q<Button>("WoodCurvingOrbitingCameraButton");
        chiselButton = root.Q<Button>("ChiselToolButton");
        turnTableSpeed = root.Q<Slider>("WoodTurningTurntableSpeedSlider");
        turnTableToggle = root.Q<Toggle>("WoodTurningToggleTurntableAnimation");
        helpPanel = root.Q<Label>("WoodTurningHelpPanelText");
        saveSolidButton = root.Q<Button>("SaveTemplateWood");
        saveSolidText = root.Q<TextField>("SavedFileNameWood");
        exportObjButton = root.Q<Button>("ExportObjWood");
        loadSolidButton = root.Q<Button>("LoadedSolidWood");
        loadListSolids = root.Q<DropdownField>("ListSolidsWood");
        resetObjectButton = root.Q<Button>("ResetButtonWood");

        // Guard nulls before registering
        if (orbitCameraButton != null) orbitCameraButton.RegisterCallback<ClickEvent>(OnClickCameraOrbitingButton);    // ✱ CHANGED
        if (chiselButton != null) chiselButton.RegisterCallback<ClickEvent>(OnClickChiselButton);               // ✱ CHANGED
        if (turnTableToggle != null) turnTableToggle.RegisterCallback<ChangeEvent<bool>>(OnTurntableValueChange);  // ✱ CHANGED
        if (turnTableSpeed != null) turnTableSpeed.RegisterCallback<ChangeEvent<float>>(OnTurntableSpeedChange); // ✱ CHANGED
        if (saveSolidButton != null) saveSolidButton.RegisterCallback<ClickEvent>(OnClickSaveSolidButton);         // ✱ CHANGED
        if (loadListSolids != null) loadListSolids.RegisterCallback<ChangeEvent<string>>(OnDropDownListChanged);  // ✱ CHANGED
        if (exportObjButton != null) exportObjButton.RegisterCallback<ClickEvent>(OnClickExportObjButton);         // ✱ CHANGED
        if (resetObjectButton != null) resetObjectButton.RegisterCallback<ClickEvent>(OnClickResetObjectButton);     // ✱ CHANGED

        if (turnTableSpeed != null) turnTableSpeed.visible = false;

        
        if (saveSolidText != null)
        {
            // Step 1: Ensure it starts "Ignored" by the navigation system
           saveSolidText.focusable = false;

            // Step 2: When clicked, WAKE IT UP
            saveSolidText.RegisterCallback<PointerDownEvent>(evt => 
            {
               saveSolidText.focusable = true; // Allow focus
               saveSolidText.schedule.Execute(() => saveSolidText.Focus());
                //saveSolidText.Focus(); // Force focus immediately
            }, TrickleDown.TrickleDown);

            // Step 3: When looking away (Blur), GO BACK TO SLEEP
            saveSolidText.RegisterCallback<BlurEvent>(evt => 
            {
                saveSolidText.focusable = false; // Make it invisible to WASD again
            });
        }

        OnTurntableSliderCheck(); // registers focus/pointer callbacks (handled in Unregister too)
        OnDisabledChisel();
        PopulateDropdown();

        //if (_scene3 != null && _scene3.chiselFullObject != null)
        //    _scene3.chiselFullObject.SetActive(false);

        // ✱ CHANGED: ensure clean visual state on first load and every reload
        
        ResetUIState();  // ✱ CHANGED

        _initialized = true; // ✱ CHANGED
    }
    // ✱ CHANGED: new helper to reset everything to "starting" state (chisel disabled, buttons unpressed)
    private void ResetUIState()   // ✱ CHANGED (NEW)
    {
        // 1) Chisel tool disabled
        if (_scene3 != null && _scene3.chiselFullObject != null)
        {
           _scene3.chiselFullObject.SetActive(false);
        }

        if (chiselButton != null)
            chiselButton.style.backgroundColor = new StyleColor(Color.white);

        if (groupAxis != null) groupAxis.visible = false;
        if (xAxis != null) xAxis.style.backgroundColor = new StyleColor(Color.white);
        if (yAxis != null) yAxis.style.backgroundColor = new StyleColor(Color.white);
        if (zAxis != null) zAxis.style.backgroundColor = new StyleColor(Color.white);
        if (helpPanel != null) helpPanel.text = "";

        // 2) Orbit camera off, button unpressed
        if (camera != null)
        {
            var orbit = camera.GetComponent<OrbitCamera>();
            if (orbit != null) orbit.enableCameraMode = false;
        }
        if (orbitCameraButton != null)
            orbitCameraButton.style.backgroundColor = new StyleColor(Color.white);

        // 3) Turntable off, hide speed slider (do it silently so your callbacks aren’t fired)
        if (turnTableToggle != null)
            turnTableToggle.SetValueWithoutNotify(false);  // ✱ CHANGED

        if (turnTableSpeed != null)
        {
            // Optional: also reset slider value silently if you want a default
            // turnTableSpeed.SetValueWithoutNotify(0f);    // uncomment if you have a desired default
            turnTableSpeed.visible = false;
        }

        if (_scene3 != null)
            _scene3.m_turntableOn = false;

        // 4) Reset ONLY this tool's buttons
        if (orbitCameraButton != null) orbitCameraButton.style.backgroundColor = new StyleColor(Color.white);
        if (chiselButton != null) chiselButton.style.backgroundColor = new StyleColor(Color.white);
        if (saveSolidButton != null) saveSolidButton.style.backgroundColor = new StyleColor(Color.white);
        if (exportObjButton != null) exportObjButton.style.backgroundColor = new StyleColor(Color.white);
        if (loadSolidButton != null) loadSolidButton.style.backgroundColor = new StyleColor(Color.white);
        if (resetObjectButton != null) resetObjectButton.style.backgroundColor = new StyleColor(Color.white);
    }


    // ✱ CHANGED: central place to unregister everything and clear refs
    private void UnregisterCallbacksAndClearRefs()
    {
        if (!_initialized) return;

        if (orbitCameraButton != null) orbitCameraButton.UnregisterCallback<ClickEvent>(OnClickCameraOrbitingButton);
        if (chiselButton != null)      chiselButton.UnregisterCallback<ClickEvent>(OnClickChiselButton);
        if (turnTableToggle != null)   turnTableToggle.UnregisterCallback<ChangeEvent<bool>>(OnTurntableValueChange);
        if (turnTableSpeed != null)    turnTableSpeed.UnregisterCallback<ChangeEvent<float>>(OnTurntableSpeedChange);
        if (saveSolidButton != null)   saveSolidButton.UnregisterCallback<ClickEvent>(OnClickSaveSolidButton);
        if (loadListSolids != null)    loadListSolids.UnregisterCallback<ChangeEvent<string>>(OnDropDownListChanged);
        if (exportObjButton != null)   exportObjButton.UnregisterCallback<ClickEvent>(OnClickExportObjButton);
        if (resetObjectButton != null) resetObjectButton.UnregisterCallback<ClickEvent>(OnClickResetObjectButton);

        // Unregister the extra callbacks attached in OnTurntableSliderCheck
        if (turnTableSpeed != null) turnTableSpeed.UnregisterCallback<FocusEvent>(OnTurntableSliderFocus); // ✱ CHANGED
        if (root != null)           root.UnregisterCallback<PointerUpEvent>(OnRootPointerUp);              // ✱ CHANGED

        // Clear references so we don’t hold stale objects after scene unloads
        orbitCameraButton = null;
        chiselButton      = null;
        groupAxis         = null;
        xAxis             = null;
        yAxis             = null;
        zAxis             = null;
        turnTableToggle   = null;
        turnTableSpeed    = null;
        helpPanel         = null;
        saveSolidButton   = null;
        saveSolidText     = null;
        exportObjButton   = null;
        loadSolidButton   = null;
        loadListSolids    = null;
        resetObjectButton = null;
        root              = null;
        uIDocument        = null;

        _initialized = false; // allow re-init on next enable
    }

    private void OnClickResetObjectButton(ClickEvent evt)
    {
        if (_scene3 != null) _scene3.ResetSolid();
    }

    private void OnClickExportObjButton(ClickEvent evt)
    {
        if (_scene3 == null) return;
        if (string.IsNullOrEmpty(selectedFile)) return;
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(selectedFile);
        _scene3.exportSolidMeshObj(Application.streamingAssetsPath + "/" + fileNameWithoutExtension + ".obj");
    }

    private void OnDropDownListChanged(ChangeEvent<string> evt)
    {
        selectedFile = evt.newValue;
        if (_scene3 == null || string.IsNullOrEmpty(selectedFile)) return;

        string fullPath = Path.Combine(Application.streamingAssetsPath, selectedFile);
        _scene3.loadSolid(fullPath);
        Debug.Log("Selected file: " + fullPath);
    }

    private void PopulateDropdown()
    {
        if (loadListSolids == null) return;

        string[] files = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath));
        List<string> fileNames = new List<string>();

        foreach (var file in files)
        {
            Debug.Log("File name:" + file);
            if (Path.GetExtension(file).Equals(".bin", System.StringComparison.OrdinalIgnoreCase))
            {
                fileNames.Add(Path.GetFileName(file));
            }
        }

        loadListSolids.choices = fileNames;

        if (fileNames.Count > 0)
            loadListSolids.value = fileNames[0]; // Select first by default
        else
            loadListSolids.value = ""; // Empty state
    }

    private void OnClickSaveSolidButton(ClickEvent evt)
    {
        if (_scene3 == null || saveSolidText == null) return;

        if (!string.IsNullOrWhiteSpace(saveSolidText.value))
        {
            _scene3.saveSolid(Application.streamingAssetsPath + "/" + saveSolidText.value + ".bin");
            Debug.Log("Save template text:" + saveSolidText.value);
            PopulateDropdown();
            saveSolidText.value = "";
        }
    }

    private void OnClickCameraOrbitingButton(ClickEvent evt)
    {
        ToggleCameraMode();
    }

    private void OnClickChiselButton(ClickEvent evt)
    {
        // 1. Safety Checks
        if (_scene3 == null || chiselButton == null) return;

        // 2. Check if the backend is ready (calculated in Scene3.cs)
        if (!_scene3.ToolReady)
        {
            Debug.LogWarning("Chisel tool not ready yet."); 
            return;
        }

        // 3. Determine the TRUE state of the chisel (trust the object, not the button color)
        bool isChiselCurrentlyActive = false; // <--- This is the variable that was missing
        
        if (_scene3.chiselFullObject != null)
        {
            isChiselCurrentlyActive = _scene3.chiselFullObject.activeSelf;
        }

        // 4. Toggle Logic based on the TRUE state
        if (isChiselCurrentlyActive)
        {
            // Case: Chisel is ON -> Turn it OFF
            if (_scene3.chiselFullObject != null) _scene3.chiselFullObject.SetActive(false);
            
            // Sync Visuals
            chiselButton.style.backgroundColor = new StyleColor(Color.white);
            if (groupAxis != null) groupAxis.visible = false;
            if (helpPanel != null) helpPanel.text = "";
        }
        else
        {
            // Case: Chisel is OFF -> Turn it ON
            if (_scene3.chiselFullObject != null) _scene3.chiselFullObject.SetActive(true);
            
            // Sync Visuals
            chiselButton.style.backgroundColor = new StyleColor(Color.grey);
            if (groupAxis != null) 
            {
                groupAxis.visible = true;
                if (xAxis != null) xAxis.style.backgroundColor = new StyleColor(Color.white);
                if (yAxis != null) yAxis.style.backgroundColor = new StyleColor(Color.white);
                if (zAxis != null) zAxis.style.backgroundColor = new StyleColor(Color.white);
            }
            if (helpPanel != null) helpPanel.text = woodTurningToolTipText;
        }
    }

    private void DisableAllOtherButtons(Button clickedButton)
    {
        if (root == null) return;
        root.Query<Button>().ForEach(button =>
        {
            if (button != clickedButton)
            {
                button.style.backgroundColor = new StyleColor(Color.white);
            }
            else
            {
                button.style.backgroundColor = new StyleColor(Color.grey);
            }
        });
    }

    private void ToggleCameraMode()
    {
        if (camera == null) return;
        var orbit = camera.GetComponent<OrbitCamera>();
        if (orbit == null || orbitCameraButton == null) return;

        if (orbit.enableCameraMode == true)
        {
            orbit.enableCameraMode = false;
            orbitCameraButton.style.backgroundColor = new StyleColor(Color.white);
        }
        else
        {
            orbit.enableCameraMode = true;
            orbitCameraButton.style.backgroundColor = new StyleColor(Color.grey);
        }
    }

    private void OnTurntableValueChange(ChangeEvent<bool> evt)
    {
        if (turnTableSpeed != null) turnTableSpeed.visible = evt.newValue;
        if (_scene3 != null) _scene3.m_turntableOn = evt.newValue;
    }

    private void OnTurntableSpeedChange(ChangeEvent<float> evt)
    {
        if (_scene3 == null) return;

        if (_scene3.m_turntableOn == true)
        {
            _scene3.m_maxSpeed = evt.newValue;
        }
        else
        {
            if (turnTableSpeed != null) turnTableSpeed.visible = false;
        }
    }

    // ✱ CHANGED: keep the delegates as methods so we can Unregister them by reference
    private void OnTurntableSliderFocus(FocusEvent evt)
    {
        Debug.Log("Slider Interact");
        if (camera == null) return;
        var orbit = camera.GetComponent<OrbitCamera>();
        if (orbit != null) orbit.enableCameraMode = false;
    }

    private void OnRootPointerUp(PointerUpEvent evt)
    {
        if (camera == null || orbitCameraButton == null) return;
        var orbit = camera.GetComponent<OrbitCamera>();
        if (orbit == null) return;

        if (orbit.enableCameraMode == false)
        {
            if (orbitCameraButton.style.unityBackgroundImageTintColor == Color.gray)
            {
                orbit.enableCameraMode = true;
            }
        }
    }

    //Doesn't work right now... 
    private void OnTurntableSliderCheck()
    {
        if (turnTableSpeed != null)
        {
            // ✱ CHANGED: register named method so we can unregister later
            turnTableSpeed.RegisterCallback<FocusEvent>(OnTurntableSliderFocus);
        }

        if (root != null)
        {
            // ✱ CHANGED: register named method so we can unregister later
            root.RegisterCallback<PointerUpEvent>(OnRootPointerUp);
        }
    }

    private void OnDisabledChisel()
    {
        if (chiselButton != null && helpPanel != null)
        {
            if (chiselButton.style.backgroundColor == new StyleColor(Color.white))
            {
                helpPanel.text = "";
            }
        }
    }
}
