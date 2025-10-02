using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIToolkitCanvasManager : MonoBehaviour
{
    public UIDocument uiDocument; // Reference to the UI Toolkit UIDocument
    public GameObject panelSettings;  // PanelSettings for UI Toolkit

    private Camera mainCamera; // The camera to use for raycasting

    void Start()
    {
        // Find PanelSettings for UI Toolkit
        panelSettings = GameObject.Find("New Panel Settings");

        // Get the main camera for raycasting
        mainCamera = GameObject.FindGameObjectWithTag("Camera2").GetComponent<Camera>();;
    }

    void Update()
    {
        panelSettings = GameObject.Find("New Panel Settings");
        // Check if the pointer is over a specific UI Toolkit element by name
        if (IsPointerOverSpecificUIElement("PhotoBoothSimulationWindow"))
        {
            // Disable PanelSettings if pointer is over the specific UI element
            DisablePanelSettings();
        }
        else
        {
            // Enable PanelSettings if pointer is not over the specific UI element
            EnablePanelSettings();
        }
    }

    // Check if the pointer is over a specific UI element by its name
    private bool IsPointerOverSpecificUIElement(string elementName)
    {
        var root = uiDocument.rootVisualElement;
        
        // Loop through all the child elements of the root and check if the pointer is over any of them
        foreach (var child in root.Children().First().Children())
        {
            if (child.name == elementName)
            {
                if (child.visible == true)
                {
                    // Register pointer events for the specific element
                    child.RegisterCallback<PointerOverEvent>(evt =>
                    {
                        Debug.Log($"Pointer is over the element: {elementName}");
                        //return true;  // Return true to stop propagation
                        return;
                    });

                    // Check if the pointer is over the specific UI element
                    if (child.ContainsPoint(mainCamera.ScreenToWorldPoint(Input.mousePosition)))
                    {
                        return true;
                    } 
                }
                
            }
        }

        return false; // Return false if the pointer is not over the specific element
    }

    // Disable UI Toolkit PanelSettings (raycasting and event handling)
    private void DisablePanelSettings()
    {
        if (panelSettings != null)
        {
            panelSettings.SetActive(false);  // Disable the PanelSettings GameObject
        }
    }

    // Enable UI Toolkit PanelSettings
    private void EnablePanelSettings()
    {
        if (panelSettings != null)
        {
            panelSettings.SetActive(true);  // Enable the PanelSettings GameObject
        }
    }
}
