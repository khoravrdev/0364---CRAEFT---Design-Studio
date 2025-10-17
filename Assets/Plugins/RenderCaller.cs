using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;
using MitsubaRendererLibrary; // Your DLL
using System.Threading.Tasks;

public class RenderCaller : MonoBehaviour
{
    // RawImages where the rendered PNGs will be shown (assign in Inspector)
    public RawImage[] rawImages;

    // Spinner icon 
    public RectTransform spinner;
    public float spinSpeed = 240f;

    // The *relative* config path that PresetPicker passes to us:
    // e.g. "photobooth_gpu\\config_photobooth_000.json" or "photobooth2_cpu\\config_photobooth_004.json"
    private string selectedConfigPath;

    //FREEZE During Rendering
    public Button renderButton;
    public Button backToPresetsButton;
    public Button backToRendererOptionsButton;

    public GameObject filePickerSaver;
    public Button selectFiledButton;
    /// <summary>
    /// Main entry from the UI "Render" button.
    /// Decides which set of JSONs to run (simple vs textured, cpu vs gpu),
    /// launches Mitsuba for each config, and then loads the resulting PNGs into the UI.
    /// </summary>

    // internal flag to rotate spinner in Update
    private bool _spinning = false;
    private bool _isRendering = false;

    // rotate spinner each frame (only while _spinning == true)
    private void Update()
    {
        if (_spinning && spinner != null)
            spinner.Rotate(0f, 0f, -spinSpeed * Time.deltaTime);
    }



    private void SetUIBusy(bool busy)
    {
        // Hide buttons while busy, show them when done
        if (renderButton) renderButton.gameObject.SetActive(!busy);
        if (backToPresetsButton) backToPresetsButton.gameObject.SetActive(!busy);
        if (backToRendererOptionsButton) backToRendererOptionsButton.gameObject.SetActive(!busy);
    }

    

    public async void RunRender()
    {
        if (string.IsNullOrEmpty(selectedConfigPath))
        {
            Debug.LogError("[Render] No preset selected. Call SetSelectedConfig first.");
            return;
        }

        // Prevent double-clicks while already rendering
        if (_isRendering)
        {
            Debug.LogWarning("[Render] Render already in progress.");
            return;
        }

        _isRendering = true;

        // Show spinner and start rotating
        if (spinner != null) spinner.gameObject.SetActive(true);
        _spinning = true;

        SetUIBusy(true);

        // Build absolute working paths based on Desktop\MitsubaFiles
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string workingDir = Path.Combine(desktopPath, "MitsubaFiles");
        string venvPath = Path.Combine(workingDir, @"mitsuba3-util-main\venv\Scripts\activate");

        //Figure out which pipeline this is (textured vs simple, gpu vs cpu)
        bool isTextured = selectedConfigPath.Contains("photobooth2");
        bool isGPU = selectedConfigPath.Contains("_gpu");

        // Base folder we’ll use for config/output files
        string baseFolder = isTextured
            ? (isGPU ? "photobooth2_gpu" : "photobooth2_cpu")
            : (isGPU ? "photobooth_gpu" : "photobooth_cpu");

        // Choose which config JSONs to run and which PNGs to load afterward
        string[] configPaths;
        string[] outputImagePaths;

        if (isTextured)
        {
            // 3 views for textured
            configPaths = new string[]
            {
                $@"{baseFolder}\config_photobooth_004.json",
                $@"{baseFolder}\config_photobooth_005.json",
                $@"{baseFolder}\config_photobooth_006.json"
            };

            outputImagePaths = new string[]
            {
                $@"{baseFolder}\output_004.png",
                $@"{baseFolder}\output_005.png",
                $@"{baseFolder}\output_006.png"
            };
        }
        else
        {
            // 4 views for simple
            configPaths = new string[]
            {
                $@"{baseFolder}\config_photobooth_000.json",
                $@"{baseFolder}\config_photobooth_001.json",
                $@"{baseFolder}\config_photobooth_002.json",
                $@"{baseFolder}\config_photobooth_003.json"
            };

            outputImagePaths = new string[]
            {
                $@"{baseFolder}\output_000.png",
                $@"{baseFolder}\output_001.png",
                $@"{baseFolder}\output_002.png",
                $@"{baseFolder}\output_003.png"
            };
        }

        Debug.Log($"[Render] WorkingDir: {workingDir}");
        Debug.Log($"[Render] venv:       {venvPath}");
        Debug.Log($"[Render] BaseFolder: {baseFolder}");

        // Run Mitsuba for each JSON in a background thread
        await Task.Run(() =>
        {
            for (int i = 0; i < configPaths.Length; i++)
            {
                string relativeConfig = configPaths[i];
                MitsubaRunner.RunRender(venvPath, workingDir, relativeConfig);
                //string result = MitsubaRunner.RunRender(venvPath, workingDir, relativeConfig);

            }
        });

        // Back on main thread: load images & stop spinner
        LoadImages(workingDir, outputImagePaths);

        _spinning = false;
        if (spinner != null) spinner.gameObject.SetActive(false);
        SetUIBusy(false);
        _isRendering = false;

        Debug.Log("[Render] Finished. Images assigned (where found) and spinner hidden.");
    }

    /// <summary>
    /// Loads PNG files produced by Mitsuba and assigns them to the RawImages array.
    /// </summary>
    private void LoadImages(string workingDir, string[] relativePaths)
    {
        for (int i = 0; i < relativePaths.Length; i++)
        {
            string imagePath = Path.Combine(workingDir, relativePaths[i]);

            if (File.Exists(imagePath))
            {
                byte[] bytes = File.ReadAllBytes(imagePath);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(bytes);

                if (i < rawImages.Length)
                    rawImages[i].texture = tex;
            }
            else
            {
                Debug.LogWarning("[Render] Image not found: " + imagePath);
            }
        }
    }

    public void SetSelectedConfig(string configPath)
    {
        selectedConfigPath = configPath; // relative path
    }

    public void ClearSelectedConfig()
    {
        selectedConfigPath = null;
    }

    public void ClearImages()
    {
        foreach (RawImage img in rawImages)
            img.texture = null;
    }
}
