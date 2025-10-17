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

    [Header("Preset Toggles")]
    public Toggle toggleSimple;
    public Toggle toggleTextured;
    public GameObject fileSaver;

    public RenderCaller renderCaller;

    public PresetPicker presetPickerScript; // Assign ImageLoaderManager in inspector

    void Start()
    {
        cpuToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                presetPickerScript.useGPU = false;
                //ShowCPUDescription();
                //Debug.Log("CPU mode selected.");
                //StopAllCoroutines();
                //StartCoroutine(DelayedCanvasSwitch());
                if (toggleSimple.isOn)
                {
                    fileSaver.GetComponent<FileSaveCases>().currentCase = FileSaveCases.CaseId.Case1;
                    renderCaller.SetSelectedConfig(@"photobooth_cpu\config_photobooth_000.json");
                    Debug.Log("[UI] SimpleTextured preset selected. useGPU=" + presetPickerScript.useGPU + " case : " + fileSaver.GetComponent<FileSaveCases>().currentCase);
                }
                else if (toggleTextured.isOn)
                {
                    fileSaver.GetComponent<FileSaveCases>().currentCase = FileSaveCases.CaseId.Case3;
                    renderCaller.SetSelectedConfig(@"photobooth2_cpu\config_photobooth_004.json");
                    Debug.Log("[UI] Textured preset selected. useGPU=" + presetPickerScript.useGPU + " case : " + fileSaver.GetComponent<FileSaveCases>().currentCase);
                }

            }
        });

        gpuToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                presetPickerScript.useGPU = true;
                //ShowGPUDescription();
                if (toggleSimple.isOn)
                {
                    fileSaver.GetComponent<FileSaveCases>().currentCase = FileSaveCases.CaseId.Case2;
                    renderCaller.SetSelectedConfig(@"photobooth_gpu\config_photobooth_000.json");         
                    Debug.Log("[UI] Simple preset selected. useGPU=" + presetPickerScript.useGPU + " case : " + fileSaver.GetComponent<FileSaveCases>().currentCase);
                }
                else if(toggleTextured.isOn)
                {
                    fileSaver.GetComponent<FileSaveCases>().currentCase = FileSaveCases.CaseId.Case4;                  
                    renderCaller.SetSelectedConfig(@"photobooth2_gpu\config_photobooth_004.json");                            
                    Debug.Log("[UI] Textured preset selected. useGPU=" + presetPickerScript.useGPU + " case : " + fileSaver.GetComponent<FileSaveCases>().currentCase);
                }
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
