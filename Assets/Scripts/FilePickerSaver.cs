using UnityEngine;
using System.IO;
using SFB; 

public class FilePickerSaver : MonoBehaviour
{
    public enum CaseId { Case1, Case2, Case3, Case4 }

    [System.Serializable]
    public class CaseRule
    {
        public string displayName = "Case"; // (optional, for Inspector readability)

        // >>> EDIT HERE: EXACT number of files required for this case (1 or 2 in your scenario)
        [Min(1)] public int expectedFileCount = 1;

        // >>> EDIT HERE: New base names (WITHOUT extension), one per file.
        // Length must match expectedFileCount. Leave empty/null to keep original names.
        public string[] targetFilenames;

        // >>> EDIT HERE: Existing destination folder for this case.
        // Option A) Use Desktop root + subfolder name(s) below
        public string desktopRelativeSubpath = ""; // e.g., "MitsubaFiles/Case1"

        // Option B) OR give a full absolute path (takes precedence when not empty)
        public string absoluteFolder = ""; // e.g., "D:/Projects/MitsubaFiles/Case1"
    }

    [Header("Which case runs when button is clicked")]
    // >>> EDIT HERE: set the case this button should perform
    public CaseId currentCase = CaseId.Case1;

    [Header("Per-Case Settings (configure all four)")]
    // >>> EDIT HERE: fill in paths + counts + names for each case
    public CaseRule case1 = new CaseRule {
        displayName = "SimpleCPU",
        expectedFileCount = 1,
        targetFilenames = new string[] { "input.obj" },
        desktopRelativeSubpath = "MitsubaFiles/photobooth_cpu",
        absoluteFolder = ""                             // leave empty to use Desktop path above
    };

    public CaseRule case2 = new CaseRule {
        displayName = "SimpleGPU",
        expectedFileCount = 1,
        targetFilenames = new string[] { "input.obj" },
        desktopRelativeSubpath = "MitsubaFiles/photobooth_gpu",
        absoluteFolder = ""
    };

    public CaseRule case3 = new CaseRule {
        displayName = "TexturedCPU",
        expectedFileCount = 2,
        targetFilenames = new string[] { "input_with_texture.obj", "texturemap.jpg" },
        desktopRelativeSubpath = "MitsubaFiles/photobooth2_cpu",
        absoluteFolder = ""
    };

    public CaseRule case4 = new CaseRule {
        displayName = "TexturedGPU",
        expectedFileCount = 2,
        targetFilenames = new string[] { "input_with_texture.obj", "texturemap.jpg" },
        desktopRelativeSubpath = "MitsubaFiles/photobooth2_gpu",
        absoluteFolder = ""
    };

    [Header("Optional: File Type Filters in the picker")]
    // >>> EDIT HERE (optional): Turn on to restrict visible extensions
    public bool useFilters = false;

    public void PickFilesAndSave()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        var rule = GetRule(currentCase);
        if (rule == null)
        {
            Debug.LogError("No rule found for current case.");
            return;
        }

        // Resolve destination folder WITHOUT creating it
        string destFolder = ResolveExistingDestination(rule);
        if (string.IsNullOrEmpty(destFolder))
        {
            Debug.LogError("No destination folder configured.");
            return;
        }

        if (!Directory.Exists(destFolder))
        {
            Debug.LogError($"Destination folder does not exist: {destFolder}");
            return; // <-- Do NOT create it; just stop
        }

        // (Optional) filters
        var filters = useFilters
            ? new ExtensionFilter[] {
                // >>> EDIT HERE (optional) add/remove filters
                new ExtensionFilter("Images", "png", "jpg", "jpeg", "exr"),
                new ExtensionFilter("Text/CSV", "txt", "csv"),
                new ExtensionFilter("All Files", "*")
              }
            : null;

        bool multiSelect = rule.expectedFileCount > 1;
        var picked = StandaloneFileBrowser.OpenFilePanel(
            $"Select {rule.expectedFileCount} file(s)", "", filters, multiSelect);

        if (picked == null || picked.Length == 0)
        {
            Debug.Log("Selection canceled.");
            return;
        }

        if (picked.Length != rule.expectedFileCount)
        {
            Debug.LogWarning($"Expected {rule.expectedFileCount} file(s), but got {picked.Length}. Nothing copied.");
            return;
        }

        // Validate target names array if provided
        string[] names = rule.targetFilenames != null ? (string[])rule.targetFilenames.Clone() : null;
        if (names != null && names.Length != rule.expectedFileCount)
        {
            Debug.LogWarning($"targetFilenames length ({names.Length}) != expectedFileCount ({rule.expectedFileCount}). Using original names.");
            names = null;
        }

        for (int i = 0; i < rule.expectedFileCount; i++)
        {
            string src = picked[i];
            if (!File.Exists(src))
            {
                Debug.LogError($"Source file missing: {src}");
                continue;
            }

            string ext = Path.GetExtension(src);
            string baseName = (names != null && !string.IsNullOrWhiteSpace(names[i]))
                ? names[i]
                : Path.GetFileNameWithoutExtension(src);

            string destPath = Path.Combine(destFolder, baseName + ext);

            // Choose one behavior:
            // 1) Do NOT overwrite (default here): make a unique name if a file already exists.
            //destPath = GetUniquePath(destPath);

            // 2) OVERWRITE instead:
            // File.Copy(src, destPath, true);  // and remove GetUniquePath line above.

            try
            {
                File.Copy(src, destPath, false);
                Debug.Log($"Copied: {src} -> {destPath}");
            }
            catch (IOException ioEx)
            {
                Debug.LogError($"Copy failed (I/O): {ioEx.Message}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Copy failed: {ex.Message}");
            }
        }

        Debug.Log($"✅ Copied {rule.expectedFileCount} file(s) into existing folder: {destFolder}");
#else
        Debug.LogWarning("Works in Editor and Windows/macOS/Linux Standalone (not WebGL/mobile).");
#endif
    }

    // ---------- Helpers (no changes needed) ----------

    private CaseRule GetRule(CaseId id)
    {
        switch (id)
        {
            case CaseId.Case1: return case1;
            case CaseId.Case2: return case2;
            case CaseId.Case3: return case3;
            case CaseId.Case4: return case4;
            default: return null;
        }
    }

    private string ResolveExistingDestination(CaseRule rule)
    {
        if (!string.IsNullOrWhiteSpace(rule.absoluteFolder))
            return rule.absoluteFolder;

        if (!string.IsNullOrWhiteSpace(rule.desktopRelativeSubpath))
        {
            string desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            return Path.Combine(desktop, rule.desktopRelativeSubpath.Replace('/', Path.DirectorySeparatorChar));
        }

        return null;
    }

    private static string GetUniquePath(string path)
    {
        if (!File.Exists(path)) return path;
        var dir = Path.GetDirectoryName(path);
        var name = Path.GetFileNameWithoutExtension(path);
        var ext = Path.GetExtension(path);
        int i = 1;
        string candidate;
        do
        {
            candidate = Path.Combine(dir, $"{name} ({i}){ext}");
            i++;
        } while (File.Exists(candidate));
        return candidate;
    }
}
