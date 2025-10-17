using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenderingModeSelector : MonoBehaviour
{
    public Toggle cpuToggle;
    public Toggle gpuToggle;

    public GameObject presetPickerCanvas;
    public GameObject thisCanvas;

    public GameObject cpuDescription;
    public GameObject gpuDescription;

    public PresetPicker presetPickerScript; // Assign ImageLoaderManager in inspector

    void Start()
    {
        cpuToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                presetPickerScript.useGPU = false;
                //ShowCPUDescription();
                Debug.Log("CPU mode selected.");
                //StopAllCoroutines();
                //StartCoroutine(DelayedCanvasSwitch());
            }
        });

        gpuToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                presetPickerScript.useGPU = true;
                //ShowGPUDescription();
                Debug.Log("GPU mode selected.");
                //StopAllCoroutines();
                //StartCoroutine(DelayedCanvasSwitch());
            }
        });
    }

    void ShowCPUDescription()
    {
        cpuDescription.SetActive(true);
        gpuDescription.SetActive(false);
    }

    void ShowGPUDescription()
    {
        cpuDescription.SetActive(false);
        gpuDescription.SetActive(true);
    }

    IEnumerator DelayedCanvasSwitch()
    {
        yield return new WaitForSeconds(5f); // Wait 5 seconds
        presetPickerCanvas.SetActive(true);
        thisCanvas.SetActive(false);
    }
}
