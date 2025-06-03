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

    //Chiesel button
    private Button chiselButton;

    //Axis visual elementes
    private VisualElement groupAxis;
    public VisualElement xAxis;
    public VisualElement yAxis;
    public VisualElement zAxis;

    //Turntable variables    
    private VisualElement turnTableToggle;
    private VisualElement turnTableSpeed;

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
    void Start()
    {
        if (SceneManager.GetSceneByName("CRAEFTUI").isLoaded)
        {
            //Initiallize UIDocument
            foreach (var obj in SceneManager.GetSceneByName("CRAEFTUI").GetRootGameObjects())
            {
                if (obj.name == "UIDocument")
                {
                    uIDocument = obj.GetComponent<UIDocument>();
                }
            }
            camera = GameObject.Find("Main Camera");
            root = uIDocument.rootVisualElement;

            //Initiallize Visual Elements
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

            orbitCameraButton.RegisterCallback<ClickEvent>(OnClickCameraOrbitingButton);
            chiselButton.RegisterCallback<ClickEvent>(OnClickChiselButton);
            turnTableToggle.RegisterCallback<ChangeEvent<bool>>(OnTurntableValueChange);
            turnTableSpeed.RegisterCallback<ChangeEvent<float>>(OnTurntableSpeedChange);
            turnTableSpeed.visible = false;
            saveSolidButton.RegisterCallback<ClickEvent>(OnClickSaveSolidButton);
            loadListSolids.RegisterCallback<ChangeEvent<string>>(OnDropDownListChanged);
            exportObjButton.RegisterCallback<ClickEvent>(OnClickExportObjButton);
            resetObjectButton.RegisterCallback<ClickEvent>(OnClickResetObjectButton);


            OnTurntableSliderCheck();
            OnDisabledChisel();
            PopulateDropdown();
            this.GetComponent<Scene3>().chiselFullObject.SetActive(false);
        }
    }

    private void OnClickResetObjectButton(ClickEvent evt)
    {
        this.GetComponent<Scene3>().ResetSolid();
    }

    private void OnClickExportObjButton(ClickEvent evt)
    {
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(selectedFile);
        this.GetComponent<Scene3>().exportSolidMeshObj(Application.streamingAssetsPath + "/" + fileNameWithoutExtension + ".obj");
    }

     private void OnDropDownListChanged(ChangeEvent<string> evt)
    {

        selectedFile = evt.newValue;
        string fullPath = Path.Combine(Application.streamingAssetsPath, selectedFile);
        this.GetComponent<Scene3>().loadSolid(fullPath);
        Debug.Log("Selected file: " + fullPath);

    }
    private void PopulateDropdown()
    {
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
        
        if (!string.IsNullOrWhiteSpace(saveSolidText.value))
        {
            this.GetComponent<Scene3>().saveSolid(Application.streamingAssetsPath + "/" + saveSolidText.value + ".bin");
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
        //DisableAllOtherButtons(chiselButton);
        if (chiselButton.style.backgroundColor == new StyleColor(Color.grey))
        {
            this.GetComponent<Scene3>().chiselFullObject.SetActive(false);
            chiselButton.style.backgroundColor = new StyleColor(Color.white);
            groupAxis.visible = false;
            helpPanel.text = "";
        }
        else
        {
            this.GetComponent<Scene3>().chiselFullObject.SetActive(true);
            chiselButton.style.backgroundColor = new StyleColor(Color.grey);
            groupAxis.visible = true;
            xAxis.style.backgroundColor = new StyleColor(Color.white);
            yAxis.style.backgroundColor = new StyleColor(Color.white);
            zAxis.style.backgroundColor = new StyleColor(Color.white);
            helpPanel.text = woodTurningToolTipText;
        }
        
    }

    private void DisableAllOtherButtons(Button clickedButton)
    {
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
        if (camera.GetComponent<OrbitCamera>().enableCameraMode == true)
        {
            camera.GetComponent<OrbitCamera>().enableCameraMode = false;
            orbitCameraButton.style.backgroundColor = new StyleColor(Color.white);
        }
        else
        {
            camera.GetComponent<OrbitCamera>().enableCameraMode = true;
            orbitCameraButton.style.backgroundColor = new StyleColor(Color.grey);
        }
    }

    private void OnTurntableValueChange(ChangeEvent<bool> evt)
    {
        if (evt.newValue == true)
        {
            turnTableSpeed.visible = true;
            this.GetComponent<Scene3>().m_turntableOn = evt.newValue;
        }
        else if (evt.newValue == false)
        {
            turnTableSpeed.visible = false;
            this.GetComponent<Scene3>().m_turntableOn = evt.newValue;

        }
    }

    private void OnTurntableSpeedChange(ChangeEvent<float> evt)
    {
        if (this.GetComponent<Scene3>().m_turntableOn == true)
        {
            this.GetComponent<Scene3>().m_maxSpeed = evt.newValue;
        }
        else
        {
            turnTableSpeed.visible = false;
        }
    }
    //Doesn't work right now... 
    private void OnTurntableSliderCheck()
    {
        // Detect when user starts interacting with the slider
        turnTableSpeed.RegisterCallback<FocusEvent>(evt =>
        {
            Debug.Log("Slider Interact");
            camera.GetComponent<OrbitCamera>().enableCameraMode = false;

        });

        // Detect when user releases the slider
        root.RegisterCallback<PointerUpEvent>(evt =>
        {
            if (camera != null)
            {
                if (camera.GetComponent<OrbitCamera>().enableCameraMode == false)
                {
                    if (orbitCameraButton.style.unityBackgroundImageTintColor == Color.gray)
                    {
                        camera.GetComponent<OrbitCamera>().enableCameraMode = true;
                    }
                }
            }

        });
    }

    private void OnDisabledChisel()
    {
        if (chiselButton.style.backgroundColor == new StyleColor(Color.white))
        {
            helpPanel.text = "";
        }

    }
}


