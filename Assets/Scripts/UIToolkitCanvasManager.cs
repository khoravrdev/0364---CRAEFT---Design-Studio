using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIToolkitCanvasManager : MonoBehaviour
{
    void Start()
    {
        // Route UI Toolkit panel events via the active EventSystem
        UnityEngine.EventSystems.EventSystem.SetUITookitEventSystemOverride(null, true, true);
    }

    void Update()
    {
      
    }
}
