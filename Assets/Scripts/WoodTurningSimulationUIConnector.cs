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

            orbitCameraButton.RegisterCallback<ClickEvent>(OnClickCameraOrbitingButton);
            chiselButton.RegisterCallback<ClickEvent>(OnClickChiselButton);
            turnTableToggle.RegisterCallback<ChangeEvent<bool>>(OnTurntableValueChange);
            turnTableSpeed.RegisterCallback<ChangeEvent<float>>(OnTurntableSpeedChange);
            turnTableSpeed.visible = false;


            OnTurntableSliderCheck();
            OnDisabledChisel();

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
            chiselButton.style.backgroundColor = new StyleColor(Color.white);
            groupAxis.visible = false;
            helpPanel.text = "";
        }
        else
        {
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


