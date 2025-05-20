using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PotterySimulatorToolUIConnector : MonoBehaviour
{
    public UIDocument uIDocument;
    private VisualElement subtractiveToolButton;
    private VisualElement additiveToolButton;
    private VisualElement messPreservingToolButton;
    private VisualElement root;
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
            subtractiveToolButton = root.Q<VisualElement>("SubtractTool");
        }
        
    }

    // Update is called once per frame
        void Update()
    {
        
    }
}
