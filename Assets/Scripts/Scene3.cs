using System.IO;
using UnityEngine;
using VoxelCarving;

public class Scene3 : MonoBehaviour
{
    enum ToolType { Voxels, Convex }
    
    // ✱ CHANGED: Public so other scripts can read it safely
    public bool ToolReady { get; private set; } = false;

    // ... (Your Enums stay the same) ...
    enum KernelSize
    {
        [InspectorName("3")] KernelSize_3 = 3,
        [InspectorName("5")] KernelSize_5 = 5,
        [InspectorName("7")] KernelSize_7 = 7,
        [InspectorName("9")] KernelSize_9 = 9,
        [InspectorName("11")] KernelSize_11 = 11,
        [InspectorName("13")] KernelSize_13 = 13,
        [InspectorName("15")] KernelSize_15 = 15,
        [InspectorName("17")] KernelSize_17 = 17,
        [InspectorName("19")] KernelSize_19 = 19,
        [InspectorName("21")] KernelSize_21 = 21,
        [InspectorName("23")] KernelSize_23 = 23,
        [InspectorName("25")] KernelSize_25 = 25
    }

    // ... (Your Volume Settings stay the same) ...
    [Header("Volume")]
    [SerializeField] [Range(1, 1024)] int m_solidVoxelsX = 128;
    [SerializeField] [Range(1, 1024)] int m_solidVoxelsY = 16;
    [SerializeField] [Range(1, 1024)] int m_solidVoxelsZ = 128;
    [SerializeField] [Range(0.001f, 10.0f)] float m_solidVoxelSize = 0.005f;

    [Header("Tool")]
    [SerializeField] [Range(0.001f, 10.0f)] float m_toolVoxelSize = 0.001f;
    [SerializeField] [Range(8, 255)] int m_toolVertexLimit = 64;
    [SerializeField] [Range(8, 255)] int m_toolPolygonLimit = 64;
    [SerializeField] ToolType m_toolType = ToolType.Convex;

    [Header("Cleanup")]
    [SerializeField] CleanupMode m_cleanupMode = CleanupMode.KeepGuardVoxel;
    [SerializeField] [Range(-1, 1023)] int m_guardVoxelX = -1;
    [SerializeField] [Range(-1, 1023)] int m_guardVoxelY = 0;
    [SerializeField] [Range(-1, 1023)] int m_guardVoxelZ = -1;

    [Header("Mesh Generation")]
    [SerializeField] MeshGenerationMode m_meshGenerationMode = MeshGenerationMode.MarchingCubes;
    [SerializeField] [Range(0, 1024)] int m_cellsX = 0;
    [SerializeField] [Range(0, 1024)] int m_cellsY = 0;
    [SerializeField] [Range(0, 1024)] int m_cellsZ = 0;
    [SerializeField] [Range(0.0f, 1.0f)] float m_isoValue = 0.7f;

    [Header("Smoothing")]
    [SerializeField] SmoothMode m_smoothMode = SmoothMode.AveragingSeparable;
    [SerializeField] KernelSize m_kernelSize = KernelSize.KernelSize_5;
    [SerializeField] [Range(0.0f, 100.0f)] float m_sigma = 0.0f;

    [Header("Turntable")]
    [SerializeField] public bool m_turntableOn = false;
    [SerializeField] [Range(0.0f, 5.0f)] public float m_maxSpeed = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float m_accelaration = 0.01f;

    float m_angle = 0.0f;
    float m_angleStep = 0.0f;
    int m_toolId = 1;

    // ✱ CHANGED: Converted to Serialized Fields for Stability
    [Header("Scene References")]
    [SerializeField] private GameObject m_solid;
    [SerializeField] private GameObject m_tool;     // Corresponds to "ToolTip"
    [SerializeField] private GameObject m_turntable;
    [SerializeField] public GameObject chiselFullObject; // Corresponds to "Tool" (Parent)

    VoxelCarvingSimulator m_voxelCarvingSimulator = null;

    // ✱ CHANGED: Use Awake to ensure state is set BEFORE any other script tries to read it
    void Awake()
    {
        // Fallback: If you forgot to drag them in Inspector, try to find them (but log a warning)
        if (m_solid == null) m_solid = GameObject.Find("Solid");
        if (m_tool == null) m_tool = GameObject.Find("ToolTip");
        if (chiselFullObject == null) chiselFullObject = GameObject.Find("Tool");
        if (m_turntable == null) m_turntable = GameObject.Find("Turntable");

        // ✱ CRITICAL FIX: Ensure the Chisel is HIDDEN immediately.
        // This guarantees "Chisel on by default" is fixed, even if the code crashes later.
        if (chiselFullObject != null)
        {
            chiselFullObject.SetActive(false);
        }
    }

    void Start()
    {
        VoxelCarvingSimulator.setLoggingCallback(onMessage);

        // ✱ CRITICAL FIX: Wrap the external simulation in Try/Catch.
        // If the Voxel Plugin fails on a client machine, it won't break the UI.
        try 
        {
            m_voxelCarvingSimulator = new VoxelCarvingSimulator();

            if (m_solid != null)
            {
                // ... (Your Voxel Setup Logic) ...
                #if true
                    m_voxelCarvingSimulator.setSolid(m_solidVoxelsX, m_solidVoxelsY, m_solidVoxelsZ, m_solidVoxelSize);
                #else
                    Mesh mesh = m_solid.GetComponent<MeshFilter>().sharedMesh;
                    m_voxelCarvingSimulator.setSolid(mesh.vertices, mesh.triangles, 0.005f);
                #endif

                m_voxelCarvingSimulator.setSolidTransform(m_solid.transform.localToWorldMatrix);
                m_voxelCarvingSimulator.setCleanupMode(m_cleanupMode);
                m_voxelCarvingSimulator.setGuardVoxel(m_guardVoxelX, m_guardVoxelY, m_guardVoxelZ);
                m_voxelCarvingSimulator.setMeshGenerationMode(m_meshGenerationMode);
                m_voxelCarvingSimulator.setCells(m_cellsX, m_cellsY, m_cellsZ);
                m_voxelCarvingSimulator.setThreshold(m_isoValue);
                m_voxelCarvingSimulator.setSmoothMode(m_smoothMode);
                m_voxelCarvingSimulator.setKernelSize((int)m_kernelSize);
                m_voxelCarvingSimulator.setSigma(m_sigma);
                m_voxelCarvingSimulator.simulate();

                updateSolidMesh();
            }

            if (m_tool != null)
            {
                // ... (Your Tool Setup Logic) ...
                Mesh mesh = m_tool.GetComponent<MeshFilter>().sharedMesh;

                switch (m_toolType)
                {
                    case ToolType.Voxels:
                        m_voxelCarvingSimulator.setToolVoxels(m_toolId, mesh.vertices, mesh.triangles, m_toolVoxelSize, true);
                        break;
                    case ToolType.Convex:
                        m_voxelCarvingSimulator.setToolConvex(m_toolId, mesh.vertices, m_toolVertexLimit, m_toolPolygonLimit, true);
                        break;
                }

                m_voxelCarvingSimulator.setToolTransform(m_toolId, m_tool.transform.localToWorldMatrix);
                
                // Debug Visualization Logic
                GameObject toolDebug = GameObject.Find("ToolTipDebug");
                if (toolDebug != null)
                {
                    // ... (Your Debug Mesh Logic) ...
                    Vector3[] vertices;
                    Vector3[] normals;
                    int[] triangles;
                    m_voxelCarvingSimulator.getToolVisualizationMesh(m_toolId, out vertices, out normals, out triangles);
                    Mesh meshDebug = toolDebug.GetComponent<MeshFilter>().mesh;
                    meshDebug.Clear();
                    meshDebug.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                    meshDebug.vertices = vertices;
                    meshDebug.normals = normals;
                    meshDebug.triangles = triangles;
                }
            }
            
            // Mark ready only if we succeeded
            ToolReady = (m_tool != null);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Scene3] Voxel Simulator Crashed: {e.Message}\n{e.StackTrace}");
            // Optional: ToolReady remains false so the user can't use a broken tool
        }
    }
    
    // ... (Keep OnApplicationQuit, Update, ResetSolid, processKeyboard, etc. exactly the same) ...
    
    void OnApplicationQuit()
    {
        if(m_voxelCarvingSimulator != null) m_voxelCarvingSimulator.destroy();
    }

    void Update()
    {

        // 1. FAILSAFE: If the tool crashed or isn't ready, FORCE IT to be invisible.
        if (!ToolReady)
        {
            if (chiselFullObject != null && chiselFullObject.activeSelf)
            {
                chiselFullObject.SetActive(false);
            }
            // Stop the rest of Update from running so we don't spam errors
            return; 
        }
        processKeyboard();
    
        // 1. Handle Turntable Rotation
        if (m_turntableOn)
        {
            m_angleStep = Mathf.Min(m_angleStep + m_accelaration, m_maxSpeed);
            m_angle += m_angleStep;

            Quaternion rotation = Quaternion.AngleAxis(m_angle, Vector3.up);

            if (m_turntable != null) m_turntable.transform.localRotation = rotation;
            if (m_solid != null) m_solid.transform.localRotation = rotation;
        }
        else if (m_angleStep > 0.0f)
        {
            m_angleStep = Mathf.Max(m_angleStep - m_accelaration, 0.0f);
            m_angle += m_angleStep;

            Quaternion rotation = Quaternion.AngleAxis(m_angle, Vector3.up);

            if (m_turntable != null) m_turntable.transform.localRotation = rotation;
            if (m_solid != null) m_solid.transform.localRotation = rotation;
        }

        // 2. Safety Check: If the simulator crashed or isn't ready, STOP here.
        // This prevents the "Could not set tool transform" error.
        if (m_voxelCarvingSimulator == null || !ToolReady) return;

        // 3. Update Simulator Transforms (Only runs if safe)
        if (m_solid != null)
        {
            m_voxelCarvingSimulator.setSolidTransform(m_solid.transform.localToWorldMatrix);
        }

        if (m_tool != null)
        {
            // This was the line causing the crash
            m_voxelCarvingSimulator.setToolTransform(m_toolId, m_tool.transform.localToWorldMatrix);
        }

        // 4. Run Simulation
        if (m_solid != null && m_tool != null)
        {
            if (m_voxelCarvingSimulator.simulate())
            {
                updateSolidMesh();
            }
        }
    }

    public void ResetSolid()
    {
        if (m_solid != null && m_voxelCarvingSimulator != null)
        {
            m_voxelCarvingSimulator.setSolid(m_solidVoxelsX, m_solidVoxelsY, m_solidVoxelsZ, m_solidVoxelSize);
            m_voxelCarvingSimulator.setSolidTransform(m_solid.transform.localToWorldMatrix);
            updateSolidMesh();
            Debug.Log("Solid reset");
        }
        else
        {
            Debug.Log("No solid available");
        }
    }
    // ... (Keep the rest of your helper functions: processKeyboard, updateSolidMesh, save/load/export, onMessage) ...
    // Note: Add null check for m_voxelCarvingSimulator in updateSolidMesh just in case
    void updateSolidMesh()
    {
        if (m_solid == null || m_voxelCarvingSimulator == null) return;

        Vector3[] vertices;
        Vector3[] normals;
        int[] triangles;

        m_voxelCarvingSimulator.getSolidVisualizationMesh(out vertices, out normals, out triangles);

        Mesh mesh = m_solid.GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;
    }
    
    // Paste the rest of your Save/Load functions here...
    void processKeyboard() { /* ... */ }
    public void saveSolid(string filename) { if (m_voxelCarvingSimulator != null) m_voxelCarvingSimulator.saveSolid(filename); }
    public void loadSolid(string filename) { if (m_voxelCarvingSimulator != null) m_voxelCarvingSimulator.loadSolid(filename); }
    public void exportSolidMeshObj(string filename) 
    {
         if (m_solid == null || m_voxelCarvingSimulator == null) return;
         // ... existing logic ...
         Vector3[] vertices; Vector3[] normals; int[] indices;
         m_voxelCarvingSimulator.getSolidVisualizationMesh(out vertices, out normals, out indices);
         writeObj(filename, vertices, normals, indices);
    }
    static void writeObj(string filename, Vector3[] vertices, Vector3[] normals, int[] indices) { /* ... existing logic ... */ }
    static void onMessage(string message) { /* ... existing logic ... */ }
}