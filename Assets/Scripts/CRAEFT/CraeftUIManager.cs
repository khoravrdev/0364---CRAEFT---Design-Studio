using UnityEngine;
using UnityEngine.UIElements;

public class CraeftUIManager : MonoBehaviour
{

    private Label _toolTip;

    private VisualElement _frame01;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _toolTip = root.Q<Label>("tooltip");
       
        _frame01 = root.Q<VisualElement>("Frame01");

        _frame01.RegisterCallback<MouseEnterEvent>(OnMouseHover);
        _frame01.RegisterCallback<MouseLeaveEvent>(OnMouseExit);
    }

  
    private void OnMouseHover(MouseEnterEvent evt)
    {
        _toolTip.style.display = DisplayStyle.Flex;
    }

    private void OnMouseExit(MouseLeaveEvent evt)
    {
        _toolTip.style.display = DisplayStyle.None;
    }
}
