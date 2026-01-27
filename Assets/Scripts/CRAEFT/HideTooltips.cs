using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class HideTooltips : MonoBehaviour
{
    void Start()
    {
        // Get the root visual element from the UIDocument component
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        // Query all Label elements that have "tooltip" in their name and hide them
        var tooltipLabels = root.Query<Label>().Where(l => !string.IsNullOrEmpty(l.name) && l.name.Contains("tooltip")).ToList();
        foreach (var label in tooltipLabels)
        {
            label.style.display = DisplayStyle.None;
        }

        // Query all VisualElements and then filter those with "button" in their name.
        var allElements = root.Query<VisualElement>().ToList();
        foreach (var element in allElements)
        {
            // Ensure the name is not null and contains "button"
            if (!string.IsNullOrEmpty(element.name) && element.name.Contains("Button"))
            {
                // Register the pointer enter event callback
                element.RegisterCallback<PointerEnterEvent>(evt => goozover(element, true));
                // Register the pointer leave event callback
                element.RegisterCallback<PointerLeaveEvent>(evt => goozover(element, false));
            }
        }
    }

    // This function will be called when the mouse enters or leaves any element with "button" in its name.
    private void goozover(VisualElement element, bool isPointerEnter)
    {
        // Query for any Label inside the VisualElement that has "tooltip" in its name
        var tooltipLabels = element.Query<Label>().Where(l => !string.IsNullOrEmpty(l.name) && l.name.Contains("tooltip")).ToList();

        // If such a Label is found, set its display style based on whether the pointer entered or left
        if (tooltipLabels.Count > 0)
        {
            if (isPointerEnter)
            {
                tooltipLabels[0].style.display = DisplayStyle.Flex;
            }
            else
            {
                tooltipLabels[0].style.display = DisplayStyle.None;
            }
        }
    }
}
