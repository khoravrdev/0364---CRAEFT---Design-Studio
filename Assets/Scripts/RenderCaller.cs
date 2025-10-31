using UnityEngine;
using System.Diagnostics;
using System;
using System.IO;
using UnityEngine.UI;
using MitsubaRendererLibrary; // Your DLL
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEditor.Search;
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

    public Button showRenderedFilesButton;
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

    private void Update()
    {
        if (_spinning && spinner != null)
            spinner.Rotate(0f, 0f, -spinSpeed * Time.deltaTime);
    }

    private void SetUIBusy(bool busy)
    {
        selectFiledButton.interactable = !busy;

        // Toggle render button text based on state
        renderButtonText.text = busy ? "Stop rendering" : "Render";
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
        string venvPath   = Path.Combine(_workingDir, @"mitsuba3-util-main\venv\Scripts");

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
                venvDirRel: venvPath, // default
                pythonRel: "python.exe",
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
}
