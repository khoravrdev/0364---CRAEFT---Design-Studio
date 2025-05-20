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
    private VisualElement root;

    public int toolIndex;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(SceneManager.GetSceneByName("CRAEFTUI").isLoaded)
        {
            foreach(var obj in SceneManager.GetSceneByName("CRAEFTUI").GetRootGameObjects())
            {
                if(obj.name == "UIDocument")
                {
                    uIDocument = obj.GetComponent<UIDocument>();
                }
            }
            root = uIDocument.rootVisualElement;            
            subtractiveToolButton = root.Q<Button>("SubtractTool");
            additiveToolButton = root.Q<Button>("AdditionTool");
            massPreservingToolButton = root.Q<Button>("MassPreservingTool");
            toolIndex = 0;

            subtractiveToolButton.RegisterCallback<ClickEvent>(OnClickSubtractiveToolButton);
            additiveToolButton.RegisterCallback<ClickEvent>(OnClickAdditiveToolButton);
            massPreservingToolButton.RegisterCallback<ClickEvent>(OnClickMassPreservingToolButton);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnClickSubtractiveToolButton(ClickEvent evt)
    {
        Debug.Log("Clicked Sub tool");
        subtractiveToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.gray);
        additiveToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
        massPreservingToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
        toolIndex = 1;
    }

    private void OnClickAdditiveToolButton(ClickEvent evt)
    {
        Debug.Log("Clicked Add tool");
        toolIndex = 2;
        additiveToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.gray);
        subtractiveToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
        massPreservingToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);

    }

    private void OnClickMassPreservingToolButton(ClickEvent evt)
    {
        Debug.Log("Clicked Mass tool");
        toolIndex = 3;
        massPreservingToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.gray);
        additiveToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
        subtractiveToolButton.style.unityBackgroundImageTintColor = new StyleColor(Color.white);

    }
}
