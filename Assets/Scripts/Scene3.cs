using System.IO;
using UnityEngine;
using VoxelCarving;

public class Scene3 : MonoBehaviour
{
	enum ToolType
	{
		Voxels,
		Convex
	}

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

	[Header("Volume")]

	[SerializeField]
	[Range(1, 1024)]
	int m_solidVoxelsX = 128;

	[SerializeField]
	[Range(1, 1024)]
	int m_solidVoxelsY = 16;

	[SerializeField]
	[Range(1, 1024)]
	int m_solidVoxelsZ = 128;

	[SerializeField]
	[Range(0.001f, 10.0f)]
	float m_solidVoxelSize = 0.005f;

	[Header("Tool")]

	[SerializeField]
	[Range(0.001f, 10.0f)]
	float m_toolVoxelSize = 0.001f;

	[SerializeField]
	[Range(8, 255)]
	int m_toolVertexLimit = 64;

	[SerializeField]
	[Range(8, 255)]
	int m_toolPolygonLimit = 64;

	[SerializeField]
	ToolType m_toolType = ToolType.Convex;

	[Header("Cleanup")]

	[SerializeField]
	CleanupMode m_cleanupMode = CleanupMode.KeepGuardVoxel;

	[SerializeField]
	[Range(-1, 1023)]
	int m_guardVoxelX = -1;

	[SerializeField]
	[Range(-1, 1023)]
	int m_guardVoxelY = 0;

	[SerializeField]
	[Range(-1, 1023)]
	int m_guardVoxelZ = -1;

	[Header("Mesh Generation")]

	[SerializeField]
	MeshGenerationMode m_meshGenerationMode = MeshGenerationMode.MarchingCubes;

	[SerializeField]
	[Range(0, 1024)]
	int m_cellsX = 0;

	[SerializeField]
	[Range(0, 1024)]
	int m_cellsY = 0;

	[SerializeField]
	[Range(0, 1024)]
	int m_cellsZ = 0;

	[SerializeField]
	[Range(0.0f, 1.0f)]
	float m_isoValue = 0.7f;

	[Header("Smoothing")]

	[SerializeField]
	SmoothMode m_smoothMode = SmoothMode.AveragingSeparable;

	[SerializeField]
	KernelSize m_kernelSize = KernelSize.KernelSize_5;

	[SerializeField]
	[Range(0.0f, 100.0f)]
	float m_sigma = 0.0f;

	[Header("Turntable")]

	[SerializeField]
	bool m_turntableOn = false;

	[SerializeField]
	[Range(0.0f, 10.0f)]
	float m_maxSpeed = 1.0f;

	[SerializeField]
	[Range(0.0f, 1.0f)]
	float m_accelaration = 0.01f;

	float m_angle = 0.0f;
	float m_angleStep = 0.0f;

	int m_toolId = 1;

	GameObject m_solid;
	GameObject m_tool;
	GameObject m_turntable;

	VoxelCarvingSimulator m_voxelCarvingSimulator = null;

	void Start()
	{
		m_solid = GameObject.Find("Solid");
		m_tool = GameObject.Find("ToolTip");
		m_turntable = GameObject.Find("Turntable");

		VoxelCarvingSimulator.setLoggingCallback(onMessage);

		m_voxelCarvingSimulator = new VoxelCarvingSimulator();

		if (m_solid != null)
		{
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
			Mesh mesh = m_tool.GetComponent<MeshFilter>().sharedMesh;

			switch (m_toolType)
			{
				case ToolType.Voxels:
					int voxels = m_voxelCarvingSimulator.setToolVoxels(m_toolId, mesh.vertices, mesh.triangles, m_toolVoxelSize, true);
					Debug.Log("Tool voxels: " + voxels);
					break;
				case ToolType.Convex:
					int planes = m_voxelCarvingSimulator.setToolConvex(m_toolId, mesh.vertices, m_toolVertexLimit, m_toolPolygonLimit, true);
					Debug.Log("Tool planes: " + planes);
					break;
			}

			m_voxelCarvingSimulator.setToolTransform(m_toolId, m_tool.transform.localToWorldMatrix);

			GameObject toolDebug = GameObject.Find("ToolTipDebug");

			if (toolDebug != null)
			{
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
	}

	void OnApplicationQuit()
	{
		m_voxelCarvingSimulator.destroy();
	}

	void Update()
	{
		processKeyboard();
	/*
		if (m_turntableOn)
		{
			m_angleStep = Mathf.Min(m_angleStep + m_accelaration, m_maxSpeed);
			m_angle += m_angleStep;

			Quaternion rotation = Quaternion.AngleAxis(m_angle, Vector3.up);

			if (m_turntable != null)
			{
				m_turntable.transform.localRotation = rotation;
			}

			if (m_solid != null)
			{
				m_solid.transform.localRotation = rotation;
			}
		}
		else if (m_angleStep > 0.0f)
		{
			m_angleStep = Mathf.Max(m_angleStep - m_accelaration, 0.0f);
			m_angle += m_angleStep;

			Quaternion rotation = Quaternion.AngleAxis(m_angle, Vector3.up);

			if (m_turntable != null)
			{
				m_turntable.transform.localRotation = rotation;
			}

			if (m_solid != null)
			{
				m_solid.transform.localRotation = rotation;
			}
		}
*/
		if (m_solid != null)
		{
			m_voxelCarvingSimulator.setSolidTransform(m_solid.transform.localToWorldMatrix);
		}

		if (m_tool != null)
		{
			m_voxelCarvingSimulator.setToolTransform(m_toolId, m_tool.transform.localToWorldMatrix);
		}

		if (m_solid != null && m_tool != null)
		{
#if false
			m_voxelCarvingSimulator.setCleanupMode(m_cleanupMode);
			m_voxelCarvingSimulator.setGuardVoxel(m_guardVoxelX, m_guardVoxelY, m_guardVoxelZ);

			m_voxelCarvingSimulator.setMeshGenerationMode(m_meshGenerationMode);
			m_voxelCarvingSimulator.setCells(m_cellsX, m_cellsY, m_cellsZ);
			m_voxelCarvingSimulator.setThreshold(m_isoValue);

			m_voxelCarvingSimulator.setSmoothMode(m_smoothMode);
			m_voxelCarvingSimulator.setKernelSize((int)m_kernelSize);
			m_voxelCarvingSimulator.setSigma(m_sigma);
#endif

			if (m_voxelCarvingSimulator.simulate())
			{
				updateSolidMesh();
			}
		}
	}

	void processKeyboard()
	{
		if (Input.GetKeyDown(KeyCode.Home))
		{
			if (m_solid != null)
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
		/*
		else if (Input.GetKeyDown(KeyCode.S))
		{
			saveSolid(Application.streamingAssetsPath + "/Solid.bin");
		}
		else if (Input.GetKeyDown(KeyCode.L))
		{
			loadSolid(Application.streamingAssetsPath + "/Solid.bin");
		}
		else if (Input.GetKeyDown(KeyCode.E))
		{
			exportSolidMeshObj(Application.streamingAssetsPath + "/Solid.obj");
		}
		*/
	}

	void updateSolidMesh()
	{
		if (m_solid == null)
		{
			return;
		}

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

		Debug.Log("Mesh updated. Vertices: " + vertices.Length + ". Triangles: " + triangles.Length / 3);
	}

	void saveSolid(string filename)
	{
		if (m_solid == null)
		{
			Debug.Log("No solid available");

			return;
		}

		m_voxelCarvingSimulator.saveSolid(filename);

		Debug.Log("Solid saved to " + filename);
	}

	void loadSolid(string filename)
	{
		if (m_solid == null)
		{
			Debug.Log("No solid available");

			return;
		}

		m_voxelCarvingSimulator.loadSolid(filename);

		Debug.Log("Solid loaded from " + filename);
	}

	void exportSolidMeshObj(string filename)
	{
		if (m_solid == null)
		{
			Debug.Log("No solid available");

			return;
		}

		Vector3[] vertices;
		Vector3[] normals;
		int[] indices;

		m_voxelCarvingSimulator.getSolidVisualizationMesh(out vertices, out normals, out indices);

		writeObj(filename, vertices, normals, indices);

		Debug.Log("Solid mesh saved to " + filename);
	}

	static void writeObj(string filename, Vector3[] vertices, Vector3[] normals, int[] indices)
	{
		if (vertices.Length != normals.Length)
		{
			throw new System.Exception("Invalid mesh");
		}

		if (indices.Length % 3 != 0)
		{
			throw new System.Exception("Invalid mesh");
		}

		int triangles = indices.Length / 3;

		using (FileStream file = File.OpenWrite(filename))
		{
			using (StreamWriter writer = new StreamWriter(file, System.Text.Encoding.ASCII, 1024))
			{
				writer.Write("# Vertices: " + vertices.Length + "\n");
				writer.Write("# Faces: " + triangles + "\n");

				for (int i = 0; i < vertices.Length; i++)
				{
					Vector3 v = vertices[i];

					writer.Write(string.Format("v {0:F6} {1:F6} {2:F6}\n", v.x, v.y, v.z));
				}

				for (int i = 0; i < vertices.Length; i++)
				{
					Vector3 n = normals[i];

					writer.Write(string.Format("vn {0:F6} {1:F6} {2:F6}\n", n.x, n.y, n.z));
				}

				for (int i = 0, j = 0; i < triangles; i++, j += 3)
				{
					int i1 = indices[j] + 1;
					int i2 = indices[j + 1] + 1;
					int i3 = indices[j + 2] + 1;

					writer.Write(string.Format("f {0}//{0} {1}//{1} {2}//{2}\n", i1, i2, i3));
				}
			}
		}
	}

	static void onMessage(string message)
	{
		try
		{
			Debug.Log(message);
		}
		catch
		{
		}
	}
}
