using System.ComponentModel.Design.Serialization;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Data.SqlTypes;

public class PotterySimulatorToolUIConnector : MonoBehaviour
{
    public UIDocument uIDocument;
    private VisualElement subtractiveToolButton;
    private VisualElement additiveToolButton;
    private VisualElement massPreservingToolButton;

    private VisualElement resetObjectButton;

    private VisualElement cameraOrbitingButton;
    private VisualElement subtractiveMultiToolButton;
    private VisualElement additiveMultiToolButton;

    //Load template button and drop menu
    private Button loadTemplateButton;
    private DropdownField loadListTemplates;
    //Save File button and text input
    private VisualElement saveTemplateButton;
    private TextField saveTemplateText;

    //Turntable buttons and slider
    private VisualElement turnTableToggle;
    private VisualElement turnTableSpeed;

    //Undo Button
    private VisualElement undoButton;
    private VisualElement root;

    //Triangle and square tools
    private Button triangleTool;
    private Button squareTool;

    public int toolIndex;

    public GameObject mainScript;
    public GameObject camera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SceneManager.GetSceneByName("CRAEFTUI").isLoaded)
        {
            foreach (var obj in SceneManager.GetSceneByName("CRAEFTUI").GetRootGameObjects())
            {
                if (obj.name == "UIDocument")
                {
                    uIDocument = obj.GetComponent<UIDocument>();
                }
            }
            root = uIDocument.rootVisualElement;
            subtractiveToolButton = root.Q<Button>("SubtractTool");
            additiveToolButton = root.Q<Button>("AdditionTool");
            massPreservingToolButton = root.Q<Button>("MassPreservingTool");
            resetObjectButton = root.Q<Button>("ResetButton");
            cameraOrbitingButton = root.Q<Button>("OrbitingCameraButton");
            subtractiveMultiToolButton = root.Q<Button>("SubtractiveMultiTool");
            additiveMultiToolButton = root.Q<Button>("AdditiveMultiTool");

            undoButton = root.Q<Button>("UndoButton");

            saveTemplateButton = root.Q<Button>("SaveTemplate");
            saveTemplateText = root.Q<TextField>("SavedFileName");

            loadTemplateButton = root.Q<Button>("LoadTemplate");
            loadListTemplates = root.Q<DropdownField>("ListTemplates");

            turnTableSpeed = root.Q<Slider>("TurntableSpeedSlider");
            turnTableToggle = root.Q<Toggle>("ToggleTurntableAnimation");

            triangleTool = root.Q<Button>("TriangleToolTip");
            squareTool = root.Q<Button>("SquareToolTip");
            toolIndex = 0;

            // Detect when user starts interacting with the slider
            turnTableSpeed.RegisterCallback<FocusEvent>(evt =>
            {
                camera.GetComponent<OrbitCamera>().enableCameraMode = false;
    
            });

            // Detect when user releases the slider
            root.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (camera.GetComponent<OrbitCamera>().enableCameraMode == false)
                {
                    if (cameraOrbitingButton.style.unityBackgroundImageTintColor == Color.gray)
                    {
                        camera.GetComponent<OrbitCamera>().enableCameraMode = true;
                    }
                    
                }
            });



            subtractiveToolButton.RegisterCallback<ClickEvent>(OnClickSubtractiveToolButton);
            additiveToolButton.RegisterCallback<ClickEvent>(OnClickAdditiveToolButton);
            //massPreservingToolButton.RegisterCallback<ClickEvent>(OnClickMassPreservingToolButton);
            resetObjectButton.RegisterCallback<ClickEvent>(OnClickResetButton);
            cameraOrbitingButton.RegisterCallback<ClickEvent>(OnClickCameraOrbitingButton);
            
            undoButton.RegisterCallback<ClickEvent>(OnClickUndoButton);

            saveTemplateButton.RegisterCallback<ClickEvent>(OnClickSaveTemplateButton);
            //saveTemplateText.RegisterCallback<ChangeEvent<string>>(OnSaveTemplateTextChange);

            //loadTemplateButton.RegisterCallback<ClickEvent>(OnClickLoadTemplateButton);
            loadListTemplates.RegisterCallback<ChangeEvent<string>>(OnDropDownListChanged);

            turnTableToggle.RegisterCallback<ChangeEvent<bool>>(OnTurntableValueChange);
            turnTableSpeed.RegisterCallback<ChangeEvent<float>>(OnTurntableSpeedChange);

            triangleTool.RegisterCallback<ClickEvent>(OnClickTriangleToolButton);
            //squareTool.RegisterCallback<ClickEvent>(OnClickSquareToolButton);


            //Start by setting camera mode ON
            camera.GetComponent<OrbitCamera>().enableCameraMode = true;
            cameraOrbitingButton.style.backgroundColor = new StyleColor(Color.gray);

        }
        PopulateDropdown();
    }

    private void OnClickTriangleToolButton(ClickEvent evt)
    {
        Debug.Log("Clicked triangle tool");
        triangleTool.style.backgroundColor = new StyleColor(Color.gray);
        squareTool.style.backgroundColor = new StyleColor(Color.white);

        mainScript.GetComponent<Script>().triangleToolFullObject.SetActive(true);
        mainScript.GetComponent<Script>().squareToolFullObject.SetActive(false);
        mainScript.GetComponent<Script>().squareToolFullObject.transform.position = new Vector3(0f, 0f, -1.0f);
        mainScript.GetComponent<Script>().EnableToolControls(mainScript.GetComponent<Script>().triangleToolFullObject);
		mainScript.GetComponent<Script>().DisableToolControl(mainScript.GetComponent<Script>().squareToolFullObject);
        root.Q<VisualElement>("PotterySimulationWindow")?.Focus();
        mainScript.GetComponent<Script>().m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f);

        //If other button than orbit camera pressed reset values
        camera.GetComponent<OrbitCamera>().enableCameraMode = false;
        cameraOrbitingButton.style.backgroundColor = new StyleColor(Color.white);

        toolIndex = 4;

    }

    private void OnDropDownListChanged(ChangeEvent<string> evt)
    {
        string selectedFile = evt.newValue;
        string fullPath = Path.Combine(Application.streamingAssetsPath, selectedFile);
        mainScript.GetComponent<Script>().m_generator.setTemplate(fullPath);
        mainScript.GetComponent<Script>().updateTexturingShader();
        Debug.Log("Selected file: " + fullPath);
    }
    private void PopulateDropdown()
    {
        string[] files = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath));
        List<string> fileNames = new List<string>();

        foreach (var file in files)
        {
            Debug.Log("File name:" + file);
             if (Path.GetExtension(file).Equals(".png", System.StringComparison.OrdinalIgnoreCase))
            {
                fileNames.Add(Path.GetFileName(file));
            }           
        }

        loadListTemplates.choices = fileNames;

        if (fileNames.Count > 0)
            loadListTemplates.value = fileNames[0]; // Select first by default
        else
            loadListTemplates.value = ""; // Empty state
    }

    private void OnClickSaveTemplateButton(ClickEvent evt)
    {
        if (!string.IsNullOrWhiteSpace(saveTemplateText.value))
        {
            mainScript.GetComponent<Script>().m_generator.saveAsTemplate(Application.streamingAssetsPath + "/" + saveTemplateText.value + ".png");
            Debug.Log("Save template text:" + saveTemplateText.value);
            PopulateDropdown();

            saveTemplateText.value = "";
        }
    }

    private void OnClickUndoButton(ClickEvent evt)
    {
        Debug.Log("Undo button pressed");
        mainScript.GetComponent<Script>().m_generator.popUndo();
        mainScript.GetComponent<Script>().m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_triangleTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_squareTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_additiveTool.SetActive(false);
        mainScript.GetComponent<Script>().m_subtractiveTool.SetActive(false);
        mainScript.GetComponent<Script>().m_massPreservingTool.SetActive(false);
        mainScript.GetComponent<Script>().m_triangleTool.SetActive(false);
        mainScript.GetComponent<Script>().m_squareTool.SetActive(false);
    }

    private void OnTurntableValueChange(ChangeEvent<bool> evt)
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

  

    private void OnTurntableSpeedChange(ChangeEvent<float> evt)
    {
        mainScript.GetComponent<Script>().m_turntableSpeed = evt.newValue;
    }

    private void OnClickCameraOrbitingButton(ClickEvent evt)
    {
        Debug.Log("Clicked Camera Orbiting");
        if (camera.GetComponent<OrbitCamera>().enableCameraMode == false)
        {
            camera.GetComponent<OrbitCamera>().enableCameraMode = true;
            cameraOrbitingButton.style.backgroundColor = new StyleColor(Color.gray);
            subtractiveToolButton.style.backgroundColor = new StyleColor(Color.white);
            additiveToolButton.style.backgroundColor = new StyleColor(Color.white);
            //massPreservingToolButton.style.backgroundColor = new StyleColor(Color.white);
            //subtractiveMultiToolButton.style.backgroundColor = new StyleColor(Color.white);
            mainScript.GetComponent<Script>().m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f);
            //mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f);
            //mainScript.GetComponent<Script>().m_triangleTool.transform.position = new Vector3(1000f, 0f, 0f);
            //mainScript.GetComponent<Script>().m_squareTool.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().m_additiveTool.SetActive(false);
            mainScript.GetComponent<Script>().m_subtractiveTool.SetActive(false);
            mainScript.GetComponent<Script>().m_massPreservingTool.SetActive(false);
            //mainScript.GetComponent<Script>().m_triangleTool.SetActive(false);
           // mainScript.GetComponent<Script>().m_squareTool.SetActive(false);

        }
    }
    /*
    private void OnClickAdditiveMultiToolButton(ClickEvent evt)
    {
        Debug.Log("Clicked additive multi tool");
        additiveMultiToolButton.style.backgroundColor = new StyleColor(Color.gray);
        subtractiveToolButton.style.backgroundColor = new StyleColor(Color.white);
        additiveToolButton.style.backgroundColor = new StyleColor(Color.white);
        massPreservingToolButton.style.backgroundColor = new StyleColor(Color.white);
        subtractiveMultiToolButton.style.backgroundColor = new StyleColor(Color.white);

        mainScript.GetComponent<Script>().m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_triangleTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_squareTool.transform.position = new Vector3(1000f, 0f, 0f);
        //If other button than orbit camera pressed resest values
        camera.GetComponent<OrbitCamera>().enableCameraMode = false;
        cameraOrbitingButton.style.backgroundColor = new StyleColor(Color.white);

        toolIndex = 5;

    }
    */
    
    /*
    private void OnClickSubtractiveMultiToolButton(ClickEvent evt)
    {
        Debug.Log("Clicked Subtractive Multi tool");
        toolIndex = 4;
        subtractiveMultiToolButton.style.backgroundColor = new StyleColor(Color.gray);
        massPreservingToolButton.style.backgroundColor = new StyleColor(Color.white);
        additiveToolButton.style.backgroundColor = new StyleColor(Color.white);
        subtractiveToolButton.style.backgroundColor = new StyleColor(Color.white);
        additiveMultiToolButton.style.backgroundColor = new StyleColor(Color.white);
        mainScript.GetComponent<Script>().m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_squareTool.transform.position = new Vector3(1000f, 0f, 0f);
        //If other button than orbit camera pressed resest values
        camera.GetComponent<OrbitCamera>().enableCameraMode = false;
        cameraOrbitingButton.style.backgroundColor = new StyleColor(Color.white);
    }
    */
    private void OnClickSubtractiveToolButton(ClickEvent evt)
    {
        Debug.Log("Clicked Sub tool");
        subtractiveToolButton.style.backgroundColor = new StyleColor(Color.gray);
        additiveToolButton.style.backgroundColor = new StyleColor(Color.white);
        massPreservingToolButton.style.backgroundColor = new StyleColor(Color.white);
        subtractiveMultiToolButton.style.backgroundColor = new StyleColor(Color.white);
        additiveMultiToolButton.style.backgroundColor = new StyleColor(Color.white);
        mainScript.GetComponent<Script>().m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_squareTool.transform.position = new Vector3(1000f, 0f, 0f);

        //If other button than orbit camera pressed resest values
        camera.GetComponent<OrbitCamera>().enableCameraMode = false;
        cameraOrbitingButton.style.backgroundColor = new StyleColor(Color.white);

        toolIndex = 1;
    }

    private void OnClickAdditiveToolButton(ClickEvent evt)
    {
        Debug.Log("Clicked Add tool");
        toolIndex = 2;
        additiveToolButton.style.backgroundColor = new StyleColor(Color.gray);
        subtractiveToolButton.style.backgroundColor = new StyleColor(Color.white);
        massPreservingToolButton.style.backgroundColor = new StyleColor(Color.white);
        subtractiveMultiToolButton.style.backgroundColor = new StyleColor(Color.white);
        additiveMultiToolButton.style.backgroundColor = new StyleColor(Color.white);
        mainScript.GetComponent<Script>().m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_triangleTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_squareTool.transform.position = new Vector3(1000f, 0f, 0f);
        //If other button than orbit camera pressed resest values
        camera.GetComponent<OrbitCamera>().enableCameraMode = false;
        cameraOrbitingButton.style.backgroundColor = new StyleColor(Color.white);
    }

    private void OnClickMassPreservingToolButton(ClickEvent evt)
    {
        Debug.Log("Clicked Mass tool");
        toolIndex = 3;
        massPreservingToolButton.style.backgroundColor = new StyleColor(Color.gray);
        additiveToolButton.style.backgroundColor = new StyleColor(Color.white);
        subtractiveToolButton.style.backgroundColor = new StyleColor(Color.white);
        subtractiveMultiToolButton.style.backgroundColor = new StyleColor(Color.white);
        additiveMultiToolButton.style.backgroundColor = new StyleColor(Color.white);
        mainScript.GetComponent<Script>().m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_triangleTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_squareTool.transform.position = new Vector3(1000f, 0f, 0f);
        //If other button than orbit camera pressed resest values
        camera.GetComponent<OrbitCamera>().enableCameraMode = false;
        cameraOrbitingButton.style.backgroundColor = new StyleColor(Color.white);
    }

  
    private void OnClickResetButton(ClickEvent evt)
    {
        mainScript.GetComponent<Script>().m_generator.restart();
        mainScript.GetComponent<Script>().m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_triangleTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_squareTool.transform.position = new Vector3(1000f, 0f, 0f);
    }
}
