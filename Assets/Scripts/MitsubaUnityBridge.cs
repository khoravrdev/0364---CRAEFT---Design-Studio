using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MitsubaRendererLibrary;   // <- your DLL namespace

public class MitsubaUnityBridge : MonoBehaviour
{
    [Header("Selection (wire these from UI if you want)")]
    public bool useGPU = true;         // toggle via UI
    public bool useTextured = false;   // toggle via UI

    [Header("Optional preview targets")]
    public RawImage[] previewTargets;  // 4 for Simple, 3 for Textured (leave empty if not needed)

    [Header("Optional status text")]
    public Text statusText;

    // --- Internal state ---
    private string _workingDir;            // e.g. "C:\\Users\\John\\Desktop\\MitsubaFiles"
    private List<string> _configList;      // relative config paths passed to DLL
    private string[] _expectedPngs;        // relative output PNGs to show afterwards

    void Awake()
    {
        // Resolve Desktop\MitsubaFiles
        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        _workingDir = Path.Combine(desktop, "MitsubaFiles");

        // Hook DLL events (optional)
        MitsubaRunner.OnPassStarted += (pass, total, rel) =>
        {
            Debug.Log($"[MitsubaDLL] Pass {pass}/{total}: {rel}");
            if (statusText) statusText.text = $"Rendering {pass}/{total}�";
        };
        MitsubaRunner.OnOutput += (line) =>
        {
            if (!string.IsNullOrEmpty(line))
                Debug.Log($"[MitsubaDLL] {line}");
        };
        MitsubaRunner.OnExited += (code) =>
        {
            Debug.Log($"[MitsubaDLL] Process exit code: {code}");
        };
    }

    // ---------------------------
    // Public UI hooks
    // ---------------------------
    public void SelectCPU(bool isOn) { if (isOn) useGPU = false; }
    public void SelectGPU(bool isOn) { if (isOn) useGPU = true; }
    public void SelectSimple(bool isOn) { if (isOn) useTextured = false; }
    public void SelectTextured(bool isOn) { if (isOn) useTextured = true; }

    public async void StartRender()
    {
        if (MitsubaRunner.IsRendering)
        {
            Debug.LogWarning("[Bridge] Already rendering.");
            return;
        }

        // Build config list + expected outputs based on current selection
        BuildJob(useGPU, useTextured, out _configList, out _expectedPngs);

        // Kick off async rendering (returns immediately)
        try
        {
            MitsubaRunner.StartSequence(
                workingDir: _workingDir,
                relativeConfigs: _configList,
                venvDirRel: @"mitsuba3-util-main\venv\Scripts", // default
                pythonRel: "python.exe",
                allowSystemPythonFallback: true
            );

            if (statusText) statusText.text = "Rendering�";

            // Optionally await completion here (so we can load previews afterwards)
            var task = MitsubaRunner.CurrentTask;
            if (task != null) await task;

            // If we got here without a stop, try to display results
            TryLoadResults();
            if (statusText) statusText.text = "Done.";
        }
        catch (Exception ex)
        {
            Debug.LogError("[Bridge] StartSequence failed: " + ex.Message);
            if (statusText) statusText.text = "Error starting render.";
        }
    }

    // Soft stop: finish current JSON, stop before the next
    public void SoftStop()
    {
        MitsubaRunner.RequestSoftStop(_workingDir);
        if (statusText) statusText.text = "Soft stop requested�";
    }

    // Hard kill: terminate immediately
    public void HardKill()
    {
        MitsubaRunner.RequestHardKill();
        if (statusText) statusText.text = "Hard stop requested.";
    }

    // ---------------------------
    // Helpers
    // ---------------------------
    private void BuildJob(bool gpu, bool textured, out List<string> configs, out string[] outputs)
    {
        string baseFolder = textured
            ? (gpu ? "photobooth2_gpu" : "photobooth2_cpu")
            : (gpu ? "photobooth_gpu" : "photobooth_cpu");

        if (textured)
        {
            // 3-view pipeline
            configs = new List<string>
            {
                $@"{baseFolder}\config_photobooth_004.json",
                $@"{baseFolder}\config_photobooth_005.json",
                $@"{baseFolder}\config_photobooth_006.json"
            };
            outputs = new[]
            {
                $@"{baseFolder}\output_004.png",
                $@"{baseFolder}\output_005.png",
                $@"{baseFolder}\output_006.png"
            };
        }
        else
        {
            // 4-view pipeline
            configs = new List<string>
            {
                $@"{baseFolder}\config_photobooth_000.json",
                $@"{baseFolder}\config_photobooth_001.json",
                $@"{baseFolder}\config_photobooth_002.json",
                $@"{baseFolder}\config_photobooth_003.json"
            };
            outputs = new[]
            {
                $@"{baseFolder}\output_000.png",
                $@"{baseFolder}\output_001.png",
                $@"{baseFolder}\output_002.png",
                $@"{baseFolder}\output_003.png"
            };
        }
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
            previewTargets[i].texture = tex;
        }
    }
}