using UnityEngine;
using System.Diagnostics;
using System;
using System.IO;
using UnityEngine.UI;
using MitsubaRendererLibrary; // Your DLL
using System.Threading;
using System.Threading.Tasks;
using TMPro;

using SFB;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using System.Linq;


public class RenderCaller : MonoBehaviour
{
    public RawImage[] rawImages;

    public RectTransform spinner;
    public float spinSpeed = 240f;

    private string selectedConfigPath;

    public Button renderButton;
    public TextMeshProUGUI renderButtonText;

    [Header("Guide UI")]
    public Button guideButton;
    public GameObject guidePanel;
    public TextMeshProUGUI guideContentText; 
    [Tooltip("Color of the guide button when the guide is open")]
    public Color guideButtonActiveColor = Color.cyan; 
    private Color _guideButtonOriginalColor; 

    [Header("Error Feedback")]
    public TextMeshProUGUI errorMessageText;

    public Button showRenderedFilesButton;
    public Button restartButton; // <<< NEW
    public Button backToPresetsButton;
    public Button backToRendererOptionsButton;

    public GameObject filePickerSaver;
    public Button selectFiledButton;

    public GameObject rawImagesPanel;

    private bool _spinning = false;
    public bool _isRendering = false;

    private string outputFolder;
    // <<< NEW: cancellation support
    private CancellationTokenSource _cts;


    // --- Internal state ---
    private string _workingDir;            // e.g. "C:\\Users\\John\\Desktop\\MitsubaFiles"
    private List<string> _configList;      // relative config paths passed to DLL
    private string[] _expectedPngs;        // relative output PNGs to show afterwards

     [Header("Optional preview targets")]
    public RawImage[] previewTargets;  // 4 for Simple, 3 for Textured (leave empty if not needed)

    private void Start()
    {
        if (guideButton != null)
        {
            guideButton.onClick.AddListener(ToggleGuide);
            if (guideButton.image != null)
            {
                _guideButtonOriginalColor = guideButton.image.color;
            }
        }

        if (errorMessageText != null)
        {
            errorMessageText.gameObject.SetActive(false);
        }

        if (guideContentText != null)
        {
             guideContentText.text = GUIDE_TEXT;
             // Force readable defaults
             guideContentText.color = Color.black; 
             //guideContentText.alignment = TextAlignmentOptions.MiddleTop;
             guideContentText.fontSize = 35;
             guideContentText.enableWordWrapping = true;
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartRenderer);
            restartButton.gameObject.SetActive(false);
        }
    }

    public void ToggleGuide()
    {
        if (guidePanel != null)
        {
            bool isActive = !guidePanel.activeSelf;
            guidePanel.SetActive(isActive);

            // Toggle Button Color
            if (guideButton != null && guideButton.image != null)
            {
                guideButton.image.color = isActive ? guideButtonActiveColor : _guideButtonOriginalColor;
            }
        }
    }

    private void Update()
    {
        if (_spinning && spinner != null)
            spinner.Rotate(0f, 0f, -spinSpeed * Time.deltaTime);
    }

    private void SetUIBusy(bool busy)
    {
        selectFiledButton.interactable = !busy;

        // Toggle render button text based on state
        renderButtonText.text = busy ? "Stop rendering" : "Start rendering";
        // (Uncomment if you want to hide other buttons during render)
        // if (backToPresetsButton) backToPresetsButton.gameObject.SetActive(!busy);
        // if (backToRendererOptionsButton) backToRendererOptionsButton.gameObject.SetActive(!busy);
    }

    // <<< NEW: Hook your "Stop" button to this, or reuse the same button as a toggle:
    public void CancelRender()
    {
        if (_isRendering && _cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            Debug.Log("[Render] Cancel requested.");
        }
    }

    // Optional: if you use ONE button as Start/Stop toggle, point it to this:
    public void OnRenderButtonClicked()
    {
        if (_isRendering)
            //CancelRender();
            HardKill();
        else
            RunRender();
    }

    public async void RunRender()
    {
        if (string.IsNullOrEmpty(selectedConfigPath))
        {
            Debug.LogError("[Render] No preset selected. Call SetSelectedConfig first.");
            return;
        }

        if (_isRendering)
        {
            Debug.LogWarning("[Render] Already rendering.");
            return;
        }

        _isRendering = true;
        _cts = new CancellationTokenSource();        // <<< NEW

        if (spinner != null) spinner.gameObject.SetActive(true);
        _spinning = true;
        SetUIBusy(true);

        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        _workingDir = Path.Combine(desktopPath, "MitsubaFiles");

        if (!Directory.Exists(_workingDir))
        {
             Debug.LogError($"[Render] MitsubaFiles not found at: {_workingDir}");
             ShowMitsubaFilesMissingError();
             
             // Reset state since we are aborting
             _isRendering = false;
             SetUIBusy(false);
             if (spinner != null) spinner.gameObject.SetActive(false);
             _spinning = false;
             return;
        }
        else
        {
            HideMitsubaFilesError();
        }
        string venvPath   = "";

        bool isTextured = selectedConfigPath.Contains("photobooth2");
        bool isGPU      = selectedConfigPath.Contains("_gpu");

        string baseFolder = isTextured
            ? (isGPU ? "photobooth2_gpu" : "photobooth2_cpu")
            : (isGPU ? "photobooth_gpu" : "photobooth_cpu");

        string[] configPaths;
        string[] outputImagePaths;

        if (isTextured)
        {
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
        _configList = configPaths.ToList();
        _expectedPngs = outputImagePaths;
        
        Debug.Log($"[Render] WorkingDir: {_workingDir}");
        Debug.Log($"[Render] venv:       {venvPath}");
        Debug.Log($"[Render] BaseFolder: {baseFolder}");

        bool cancelled = false; // <<< NEW

         // Kick off async rendering (returns immediately)
        try
        {
         MitsubaRunner.StartSequence(
            workingDir: _workingDir,
            relativeConfigs: _configList,
            venvDirRel: venvPath,      // <--- Explicitly pass null here
            pythonRel: "python",   // <--- Change "python.exe" to just "python"
            allowSystemPythonFallback: true
        );        

            // Optionally await completion here (so we can load previews afterwards)
            var task = MitsubaRunner.CurrentTask;
            if (task != null) await task;

            // If we got here without a stop, try to display results
            TryLoadResults();           
        }
        catch (Exception ex)
        {
            Debug.LogError("[Bridge] StartSequence failed: " + ex.Message);
            
        }

        // Back on main thread: stop spinner and update UI
        _spinning = false;
        if (spinner != null) spinner.gameObject.SetActive(false);
        SetUIBusy(false);
        _isRendering = false;

        // Completed all renders -> load images
        //LoadImages(_workingDir, outputImagePaths);
        rawImagesPanel.SetActive(true);
        renderButton.gameObject.SetActive(false);
        showRenderedFilesButton.gameObject.SetActive(true);
        if (restartButton != null) restartButton.gameObject.SetActive(true); // <<< NEW
        outputFolder = _workingDir + "\\" + baseFolder;
        //Add here button to go to the output folder depending on what kind of rendering we are doing
        Debug.Log("[Render] Finished. Images assigned and spinner hidden.");
    }

    public void HardKill()
    {
        MitsubaRunner.RequestHardKill();
        Debug.Log("HARD KILLED");
        //if (statusText) statusText.text = "Hard stop requested.";
    }

    private void TryLoadResults()
    {
        if (previewTargets == null || previewTargets.Length == 0) return;

        for (int i = 0; i < _expectedPngs.Length && i < previewTargets.Length; i++)
        {
            string abs = Path.Combine(_workingDir, _expectedPngs[i]);
            if (!File.Exists(abs))
            {
                Debug.LogWarning("[Bridge] Result not found: " + abs);
                continue;
            }

            byte[] bytes = File.ReadAllBytes(abs);
            var tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            if (i < rawImages.Length)
                previewTargets[i].texture = tex;
        }
    }

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

    public void ShowOutputFolder()
    {
        string outputPath = outputFolder;
        //StandaloneFileBrowser.OpenFilePanel("Output Folder", "@" + outputFolder, "", false);
        if (System.IO.Directory.Exists(outputPath))
        {
            // Open the folder in the file explorer
            Process.Start("explorer.exe", outputPath);
        }
        else
        {
            Debug.LogError("Folder path does not exist: " + outputPath);
        }
    }

    public void SetSelectedConfig(string configPath)  { selectedConfigPath = configPath; }
    public void ClearSelectedConfig()                 { selectedConfigPath = null; }
    public void ClearImages()                         { foreach (RawImage img in rawImages) img.texture = null; }

    public void ShowMitsubaFilesMissingError()
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = "Error: 'MitsubaFiles' folder not found on Desktop.\n<size=80%>Please run the setup file first, and make sure MitsubaFiles folder is on the Desktop.</size>";
            errorMessageText.color = Color.red;
            errorMessageText.gameObject.SetActive(true);
        }
    }

    public void HideMitsubaFilesError()
    {
        if (errorMessageText != null)
        {
            errorMessageText.gameObject.SetActive(false);
        }
    }

    public void RestartRenderer()
    {
        // Hide results
        rawImagesPanel.SetActive(false);
        showRenderedFilesButton.gameObject.SetActive(false);
        if (restartButton != null) restartButton.gameObject.SetActive(false);

        // Show render button
        if (renderButton != null)
        {
            renderButton.gameObject.SetActive(true);
            renderButtonText.text = "Start rendering"; // ensuring text is reset
        }
        
        // Reset state
        _isRendering = false;
        ClearImages();
    }

    private const string GUIDE_TEXT = @"<b>Instructions For Using The Design Studio App</b>

<b>1. Choose Rendering Method</b>
When the app starts, you will see two options:
- <b>CPU Renderer</b>: Uses Scalar_RGB Mode (Slower, compatible with any PC).
- <b>GPU Renderer</b>: Uses Cuda_RGB Mode (Faster, NVIDIA only).

<b>2. Select a Preset Type</b>
After Rendering Options, choose:
- <b>Simple Obj</b>: Renders a 3D Model without textures.
- <b>Textured Obj</b>: Renders a 3D Model with a texture map.

<b>3. Rendering Phase</b>
Once you choose a preset:
- Click <b>Select FIle</b>.
- Select the input.obj file you want to render.
- Open the file.
- Click <b>Start rendering</b>.
- The process runs in the background (may take a few minutes).
- Rendered PNGs appear on screen when finished.
- Files saved to the pipeline folder (e.g., photobooth_cpu) under MitsubaFiles.

<b>4. Using New Obj Models</b>
You can replace files in the rendering presets.

    <b>For Simple Obj (No Texture):</b>
    - Go to MitsubaFiles/photobooth_cpu (or gpu).
    - Replace <i>input.obj</i> with your model (must be named <i>input.obj</i>).

    <b>For Textured Obj:</b>
    - Go to MitsubaFiles/photobooth2_cpu (or gpu).
    - Replace <i>input_with_texture.obj</i> with your model.
    - Replace <i>texturemap.jpg</i> with your texture.
    - Keep filenames exactly the same.

<b>5. Where are the Rendered Images Saved?</b>
- Photobooth folder for simple mesh.
- Photobooth2 folder for textured.
- Named output_000.png ... output_N.png.

<i>Only .obj files are supported.</i>
";
}
