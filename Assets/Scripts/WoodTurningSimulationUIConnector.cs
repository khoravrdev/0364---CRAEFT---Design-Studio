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
    private VisualElement xAxis;
    private VisualElement yAxis;
    private VisualElement zAxis;

    //Turntable variables    
    private VisualElement turnTableToggle;
    private VisualElement turnTableSpeed;

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
            groupAxis = root.Q<VisualElement>("AxisGroup");
            xAxis = root.Q<VisualElement>("XAxis");
            yAxis = root.Q<VisualElement>("YAxis");
            zAxis = root.Q<VisualElement>("ZAxis");
            orbitCameraButton = root.Q<Button>("WoodCurvingOrbitingCameraButton");
            chiselButton = root.Q<Button>("ChiselToolButton");
            turnTableSpeed = root.Q<Slider>("WoodTurningTurntableSpeedSlider");
            turnTableToggle = root.Q<Toggle>("WoodTurningToggleTurntableAnimation");

            orbitCameraButton.RegisterCallback<ClickEvent>(OnClickCameraOrbitingButton);
            chiselButton.RegisterCallback<ClickEvent>(OnClickChiselButton);
            turnTableToggle.RegisterCallback<ChangeEvent<bool>>(OnTurntableValueChange);
            turnTableSpeed.RegisterCallback<ChangeEvent<float>>(OnTurntableSpeedChange);
            turnTableSpeed.visible = false;


            OnTurntableSliderCheck();

        }
    }


    private void OnClickCameraOrbitingButton(ClickEvent evt)
    {
        ToggleCameraMode();
    }

    private void OnClickChiselButton(ClickEvent evt)
    {
        //DisableAllOtherButtons(chiselButton);
        chiselButton.style.backgroundColor = new StyleColor(Color.grey);
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
}


