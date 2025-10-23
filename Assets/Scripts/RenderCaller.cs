using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;
using MitsubaRendererLibrary; // Your DLL
using System.Threading;
using System.Threading.Tasks;
using TMPro;

public class RenderCaller : MonoBehaviour
{
    public RawImage[] rawImages;

    public RectTransform spinner;
    public float spinSpeed = 240f;

    private string selectedConfigPath;

    public Button renderButton;
    public TextMeshProUGUI renderButtonText;
    public Button backToPresetsButton;
    public Button backToRendererOptionsButton;

    public GameObject filePickerSaver;
    public Button selectFiledButton;

    public GameObject rawImagesPanel;

    private bool _spinning = false;
    public bool _isRendering = false;

    // <<< NEW: cancellation support
    private CancellationTokenSource _cts;

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
            CancelRender();
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
        string workingDir = Path.Combine(desktopPath, "MitsubaFiles");
        string venvPath   = Path.Combine(workingDir, @"mitsuba3-util-main\venv\Scripts\activate");

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

        Debug.Log($"[Render] WorkingDir: {workingDir}");
        Debug.Log($"[Render] venv:       {venvPath}");
        Debug.Log($"[Render] BaseFolder: {baseFolder}");

        bool cancelled = false; // <<< NEW

        try
        {
            // Run Mitsuba for each JSON in a background thread, with cancellation checks
            await Task.Run(() =>
            {
                for (int i = 0; i < configPaths.Length; i++)
                {
                    if (_cts.IsCancellationRequested) { cancelled = true; break; } // <<< NEW

                    string relativeConfig = configPaths[i];
                    MitsubaRunner.RunRender(venvPath, workingDir, relativeConfig);

                    // After each render returns, check again
                    if (_cts.IsCancellationRequested) { cancelled = true; break; } // <<< NEW
                }
            }, _cts.Token); // token is attached (useful if you add ThrowIfCancellationRequested in future)
        }
        catch (OperationCanceledException)
        {
            cancelled = true;
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }

        // Back on main thread: stop spinner and update UI
        _spinning = false;
        if (spinner != null) spinner.gameObject.SetActive(false);
        SetUIBusy(false);
        _isRendering = false;

        if (cancelled)
        {
            Debug.Log("[Render] Cancelled by user.");
            return; // do not load images if cancelled (or change if you want partial loads)
        }

        // Completed all renders -> load images
        LoadImages(workingDir, outputImagePaths);
        rawImagesPanel.SetActive(true);
        Debug.Log("[Render] Finished. Images assigned and spinner hidden.");
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

    public void SetSelectedConfig(string configPath)  { selectedConfigPath = configPath; }
    public void ClearSelectedConfig()                 { selectedConfigPath = null; }
    public void ClearImages()                         { foreach (RawImage img in rawImages) img.texture = null; }
}
