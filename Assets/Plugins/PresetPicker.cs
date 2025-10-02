using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class PresetPicker : MonoBehaviour
{
    [Header("Preset Toggles")]
    public Toggle toggleSimple;
    public Toggle toggleTextured;

    [Header("Canvases")]
    public GameObject pickPresetsCanvas;
    public GameObject studioCanvas;

    [Header("Descriptions")]
    public GameObject descriptionA; // Simple description (or CPU text if you prefer)
    public GameObject descriptionB; // Textured description (or GPU text if you prefer)

    [Header("Renderer")]
    public RenderCaller renderCaller;

    // Set this from your CPU/GPU first canvas controller
    [Header("Mode")]
    public bool useGPU = false;

    private string lastPresetFolder = "";

    public void SetLastPresetFolder(string folder) => lastPresetFolder = folder;


    [Header("Mode Select Canvas (CPU/GPU)")]
    public GameObject modeSelectCanvas;     // The first canvas (CPU/GPU selection)

    // CPU/GPU toggles (from the first canvas)
    public Toggle cpuToggle;
    public Toggle gpuToggle;

    // Optional: CPU/GPU mode descriptions on the first canvas
    public GameObject cpuModeDescription;
    public GameObject gpuModeDescription;

    public void OnPresetSelected()
    {
        // (Optional) ensure one is selected
        if (!toggleSimple.isOn && !toggleTextured.isOn)
        {
            Debug.LogWarning("[UI] Please select Simple or Textured first.");
            return;
        }
        StartCoroutine(HandlePresetSelection());
    }

    private IEnumerator HandlePresetSelection()
    {
        string selectedConfig;

        // Decide base folder from GPU/CPU and Simple/Textured
        if (toggleTextured.isOn)
        {
            // Textured (photobooth2)
            selectedConfig = useGPU
                ? @"photobooth2_gpu\config_photobooth_004.json"
                : @"photobooth2_cpu\config_photobooth_004.json";

            Debug.Log("[UI] Textured preset selected. useGPU=" + useGPU);
            descriptionA.SetActive(false);
            descriptionB.SetActive(true);
            yield return new WaitForSeconds(5f);
            descriptionB.SetActive(false);
        }
        else
        {
            // Simple (photobooth)
            selectedConfig = useGPU
                ? @"photobooth_gpu\config_photobooth_000.json"
                : @"photobooth_cpu\config_photobooth_000.json";

            Debug.Log("[UI] Simple preset selected. useGPU=" + useGPU);
            descriptionB.SetActive(false);
            descriptionA.SetActive(true);
            yield return new WaitForSeconds(5f);
            descriptionA.SetActive(false);
        }

        // Pass the picked config (just a marker; RenderCaller expands to full set)
        renderCaller.SetSelectedConfig(selectedConfig);
        Debug.Log("[UI] Config passed: " + selectedConfig);

        // Switch canvases
        pickPresetsCanvas.SetActive(false);
        studioCanvas.SetActive(true);
        Debug.Log("[UI] Switched to StudioCanvas.");
    }

    public void OnBackToPresets()
    {
        studioCanvas.SetActive(false);
        pickPresetsCanvas.SetActive(true);

        // Show initial description again if you want
        descriptionA.SetActive(true);
        descriptionB.SetActive(false);

        // Clear images and selection only in UI (don’t delete files)
        if (renderCaller != null)
        {
            renderCaller.ClearImages();
            renderCaller.ClearSelectedConfig();
        }

        Debug.Log("[UI] Back to presets. Cleared images (files preserved).");
    }

    public void OnBackToModeSelect()
    {
        Debug.Log("[UI] Back to CPU/GPU selection requested. Clearing state…");

        // 1) Clear images & selected config in the renderer
        if (renderCaller != null)
        {
            renderCaller.ClearImages();
            renderCaller.ClearSelectedConfig();
        }

        // 2) Clear our Simple/Textured UI state (without triggering events)
        if (toggleSimple) toggleSimple.SetIsOnWithoutNotify(false);
        if (toggleTextured) toggleTextured.SetIsOnWithoutNotify(false);

        // Hide the Simple/Textured descriptions
        if (descriptionA) descriptionA.SetActive(false);
        if (descriptionB) descriptionB.SetActive(false);

        // 3) Reset GPU flag (optional default—forces user to choose again)
        useGPU = false;

        // 4) Switch canvases: hide Studio & Preset, show Mode Select (CPU/GPU)
        if (studioCanvas) studioCanvas.SetActive(false);
        if (pickPresetsCanvas) pickPresetsCanvas.SetActive(false);
        if (modeSelectCanvas) modeSelectCanvas.SetActive(true);

        // 5) Reset CPU/GPU toggles on the first canvas WITHOUT firing their listeners
        if (cpuToggle) cpuToggle.SetIsOnWithoutNotify(false);
        if (gpuToggle) gpuToggle.SetIsOnWithoutNotify(false);

        // 6) Hide CPU/GPU descriptions (optional)
        if (cpuModeDescription) cpuModeDescription.SetActive(false);
        if (gpuModeDescription) gpuModeDescription.SetActive(false);

        Debug.Log("[UI] Returned to CPU/GPU canvas. All selections cleared.");
    }


}
