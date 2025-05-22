using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PotterySimulatorToolUIConnector : MonoBehaviour
{
    public UIDocument uIDocument;
    private VisualElement subtractiveToolButton;
    private VisualElement additiveToolButton;
    private VisualElement massPreservingToolButton;

    private VisualElement resetObjectButton;

    private VisualElement cameraOrbitingButton;
    private VisualElement subtractiveMultiToolButton;
    private VisualElement root;

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
            toolIndex = 4;



            subtractiveToolButton.RegisterCallback<ClickEvent>(OnClickSubtractiveToolButton);
            additiveToolButton.RegisterCallback<ClickEvent>(OnClickAdditiveToolButton);
            massPreservingToolButton.RegisterCallback<ClickEvent>(OnClickMassPreservingToolButton);
            resetObjectButton.RegisterCallback<ClickEvent>(OnClickResetButton);
            cameraOrbitingButton.RegisterCallback<ClickEvent>(OnClickCameraOrbitingButton);
            subtractiveMultiToolButton.RegisterCallback<ClickEvent>(OnClickSubtractiveMultiToolButton);

            //Start by setting camera mode ON
            camera.GetComponent<OrbitCamera>().enableCameraMode = true;
            cameraOrbitingButton.style.unityBackgroundImageTintColor = new StyleColor(Color.gray);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnClickCameraOrbitingButton(ClickEvent evt)
    {
        Debug.Log("Clicked Camera Orbiting");
        if (camera.GetComponent<OrbitCamera>().enableCameraMode == false)
        {
            camera.GetComponent<OrbitCamera>().enableCameraMode = true;
            cameraOrbitingButton.style.unityBackgroundImageTintColor = new StyleColor(Color.gray);
            subtractiveToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
            additiveToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
            massPreservingToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
            subtractiveMultiToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
            mainScript.GetComponent<Script>().m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().m_subtractiveTool1.transform.position = new Vector3(1000f, 0f, 0f);
            mainScript.GetComponent<Script>().m_additiveTool.SetActive(false);
            mainScript.GetComponent<Script>().m_subtractiveTool.SetActive(false);
            mainScript.GetComponent<Script>().m_massPreservingTool.SetActive(false);
            mainScript.GetComponent<Script>().m_subtractiveTool1.SetActive(false);

        }
    }

    private void OnClickSubtractiveToolButton(ClickEvent evt)
    {
        Debug.Log("Clicked Sub tool");
        subtractiveToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.gray);
        additiveToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
        massPreservingToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
        subtractiveMultiToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
        mainScript.GetComponent<Script>().m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);

        //If other button than orbit camera pressed resest values
        camera.GetComponent<OrbitCamera>().enableCameraMode = false;
        cameraOrbitingButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);

        toolIndex = 1;
    }

    private void OnClickAdditiveToolButton(ClickEvent evt)
    {
        Debug.Log("Clicked Add tool");
        toolIndex = 2;
        additiveToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.gray);
        subtractiveToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
        massPreservingToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
        subtractiveMultiToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
        mainScript.GetComponent<Script>().m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);

        //If other button than orbit camera pressed resest values
        camera.GetComponent<OrbitCamera>().enableCameraMode = false;
        cameraOrbitingButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
    }

    private void OnClickMassPreservingToolButton(ClickEvent evt)
    {
        Debug.Log("Clicked Mass tool");
        toolIndex = 3;
        massPreservingToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.gray);
        additiveToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
        subtractiveToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
        subtractiveMultiToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
        mainScript.GetComponent<Script>().m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f);

        //If other button than orbit camera pressed resest values
        camera.GetComponent<OrbitCamera>().enableCameraMode = false;
        cameraOrbitingButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
    }

    private void OnClickSubtractiveMultiToolButton(ClickEvent evt)
    {
        Debug.Log("Clicked Subtractive Multi tool");
        toolIndex = 4;
        subtractiveMultiToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.gray);
        massPreservingToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
        additiveToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
        subtractiveToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);

        mainScript.GetComponent<Script>().m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);

        //If other button than orbit camera pressed resest values
        camera.GetComponent<OrbitCamera>().enableCameraMode = false;
        cameraOrbitingButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
    }

    private void OnClickResetButton(ClickEvent evt)
    {
        mainScript.GetComponent<Script>().m_generator.restart();
        mainScript.GetComponent<Script>().m_additiveTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_subtractiveTool.transform.position = new Vector3(1000f, 0f, 0f);
        mainScript.GetComponent<Script>().m_massPreservingTool.transform.position = new Vector3(1000f, 0f, 0f);
    }
}
