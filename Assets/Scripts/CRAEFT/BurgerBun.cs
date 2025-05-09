using UnityEngine;
using UnityEngine.UIElements;

public class BurgerBun : MonoBehaviour
{
    // Reference to the UI document
    public UIDocument uiDocument;
    private Button burgerButton;
    private VisualElement menuBar;
    private VisualElement root;

    // Textures for the burger button's background image
    public Texture2D defaultTexture;   // Original burger icon
    public Texture2D closingTexture;   // Closing image (e.g., closing.png)

    // Toggle state flag
    private bool isOpen = false;

    void Start()
    {
        // Get the root VisualElement from the assigned UIDocument
        root = uiDocument.rootVisualElement;

        // Find the BurgerButton in the UI
        burgerButton = root.Q<Button>("Burgerbutton");
        if (burgerButton == null)
        {
            Debug.LogError("BurgerButton not found in the UI.");
            return;
        }

        // Find the menuBar in the UI
        menuBar = root.Q<VisualElement>("menuBar");
        if (menuBar == null)
        {
            Debug.LogError("menuBar not found in the UI.");
            return;
        }

        // Set the initial background image for the burger button
        burgerButton.style.backgroundImage = new StyleBackground(defaultTexture);
        // Optionally, set an initial class for the menuBar
        menuBar.AddToClassList("sheetout");

        // Register the click event callback on the burger button
        burgerButton.RegisterCallback<ClickEvent>(OnClickBurgerButton);

        // Register a global pointer down event (capturing phase) on the root.
        // This will capture clicks anywhere in the UI.
        root.RegisterCallback<PointerDownEvent>(OnGlobalPointerDown, TrickleDown.TrickleDown);
    }

    private void OnClickBurgerButton(ClickEvent evt)
    {
        // Toggle the menu
        ToggleMenu();

        // Stop propagation so the global pointer event doesn't immediately close the menu.
        evt.StopPropagation();
    }

    // Global pointer down callback for closing the menu when clicking outside.
    private void OnGlobalPointerDown(PointerDownEvent evt)
    {
        // Only process if the menu is open.
        if (!isOpen)
            return;

        Vector2 clickPos = evt.position;

        // Check if the click is within the bounds of the burger button or menuBar.
        if (burgerButton.worldBound.Contains(clickPos) || menuBar.worldBound.Contains(clickPos))
            return;

        // Otherwise, close the menu.
        ToggleMenu();
    }

    // Toggle the menu state and update the visuals accordingly.
    private void ToggleMenu()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            burgerButton.style.backgroundImage = new StyleBackground(closingTexture);

            if (menuBar.ClassListContains("sheetout"))
            {
                menuBar.RemoveFromClassList("sheetout");
                menuBar.AddToClassList("sheetout--in");
            }
        }
        else
        {
            burgerButton.style.backgroundImage = new StyleBackground(defaultTexture);

            if (menuBar.ClassListContains("sheetout--in"))
            {
                menuBar.RemoveFromClassList("sheetout--in");
                menuBar.AddToClassList("sheetout");
            }
        }
    }
}
