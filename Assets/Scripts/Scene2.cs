using System.IO;
using UnityEngine;
using RevolutionSolid;

// Scene2 is similar to Scene1, but it uses "multi-tools" instead of "single-tools". A single tool's
// mesh is approximated by a single sphere. A multi tool's mesh is approximated by a large number
// (hundreds or thousands) of small spheres (or voxels). The approximation is better and the solid's
// carving is more realistic but a multi-tool typically requires more processing power than a
// single-tool. The approximation is performed using the Voxelizer class, when the tool is created
// (in Start method). We display the actual 3D model of the tools instead of just spheres. For
// simplicity, in this scene we only create 2 subtractive tools.

public class Scene2 : MonoBehaviour
{
	// The generated solid. This is a Unity game object. We regularly replace its mesh with a new
	// one in updateMesh(). You can apply transformations to it, but only uniform scaling is
	// currently supported.
	GameObject m_solid = null;

	// The tool objects. These are 3D models which we "voxelize", i.e. approximate with a large
	// number of small spheres. You can apply transformations to the tools, but only uniform scaling
	// is currently supported. During runtime, move the tools closer to the solid (by altering its
	// transformation). When a tool's mesh collides with the solid it will "carve" it. If the 3D
	// model is large, it would be beneficial to only voxelize a smaller part of the model, the one
	// that actually comes in contact with the solid. This part should have the same transformation
	// with the full model (i.e. should be a child of the full model). The full model should be
	// rendered, while the voxelized part should not.
	GameObject m_subtractiveTool1 = null;
	GameObject m_subtractiveTool2 = null;

	// These are used to display the voxelized mesh for debugging purposes (i.e. to determine the
	// correct/sufficient voxelSize value). These should be disabled or removed in the release version.
	GameObject m_subtractiveTool1Debug = null;
	GameObject m_subtractiveTool2Debug = null;

	// The user-defined IDs for the two tools. These are used to identify each tool in the library methods.
	int m_subtractiveTool1Id = 1;
	int m_subtractiveTool2Id = 2;

	// Voxel size to be used when voxelizing the tools' models. The model will be "split" internally
	// to voxels of this size. Unit is the same as in the 3D model file. Using smaller values, the
	// model will be split into more and smaller voxels, thus improving approximation accuracy and
	// realism. However, this will also lead to increased CPU usage. The correct value should be
	// determined with trial-and-error, while inspecting the generated meshes in the debug game
	// objects mentioned above.
	float m_subtractiveTool1VoxelSize = 0.005f;
	float m_subtractiveTool2VoxelSize = 0.005f;

	// The generator instance. Created in Start(), destroyed in OnApplicationQuit().
	Generator m_generator = null;

	// The voxelizer instance, used to voxelize the tools' models. Created in Start(), destroyed
	// in OnApplicationQuit().
	Voxelizer m_voxelizer = null;

	// If true, apply rotation to the solid about its Y axis to emulate the solid being upon a
	// turntable.
	[SerializeField]
	bool m_turntableOn = true;

	// Speed of the turntable animation (degrees per frame).
	[SerializeField]
	[Range(0.0f, 20.0f)]
	float m_turntableSpeed = 8.0f;

	// Acceleration/deceleration.
	[SerializeField]
	[Range(0.0f, 1.0f)]
	float m_turntableAccelaration = 0.1f;

	// These are used for the turntable animation, if enabled.
	float m_turntableAngle = 0.0f;
	float m_turntableAngleStep = 0.0f;

	void Start()
	{
		m_solid = GameObject.Find("Solid");
		m_subtractiveTool1 = GameObject.Find("SubtractiveTool1Tip");
		m_subtractiveTool2 = GameObject.Find("SubtractiveTool2Tip");
		m_subtractiveTool1Debug = GameObject.Find("SubtractiveTool1TipDebug");
		m_subtractiveTool2Debug = GameObject.Find("SubtractiveTool2TipDebug");

		// Set up debug logging for the DLL. Messages will be printed if something goes wrong (for
		// example invalid argument passed to method). You should comment out this in release build.
		Generator.setLoggingCallback(onMessage);

		// Create the generator and voxelizer instances.
		m_generator = new Generator();
		m_voxelizer = new Voxelizer();

		// Set a template to be used as a starting point for carving. The provided template is
		// a white rectangle, which generates a cylinder. You can use other templates, either created
		// with a painting application, or one that was saved with a call to Generator.saveAsTemplate().
		// Many Generator methods will produce an error if a template is not set before calling. You
		// can change the template at any time, even during runtime.
		m_generator.setTemplate(Application.streamingAssetsPath + "/template2.png");
		//m_generator.setTemplate(Application.streamingAssetsPath + "/customTemplate.png");

		// Setup parameters for the texturing shader. This should be called whenever you change the
		// template (i.e. when you call Generator.setTemplate()).
		updateTexturingShader();

		// Add tool 1
		if (m_subtractiveTool1 != null)
		{
			Mesh mesh = m_subtractiveTool1.GetComponent<MeshFilter>().sharedMesh;

			// Voxelize the mesh. Produce both a triangle mesh representing the voxels, which we
			// will use for debugging and the voxel centers, which are used for collision detection.
			// Note that if the voxelSize parameter is too small the method will take a long time to
			// return. Extremely small values may lead to memory exhaustion and crashes...

			m_voxelizer.voxelize(mesh.vertices, mesh.triangles, m_subtractiveTool1VoxelSize, VoxelizationFlags.BuildVoxels | VoxelizationFlags.BuildVoxelCenters);

			// Retrieve voxelization results.

			Vector3[] voxelCenters;
			Vector3[] vertices;
			Vector3[] normals;
			int[] indices;

			m_voxelizer.getVoxels(out vertices, out normals, out indices);
			m_voxelizer.getVoxelCenters(out voxelCenters);

			Debug.Log("SubtractiveTool1 mesh voxelized. Voxel size: " + m_subtractiveTool1VoxelSize + ". Vertices: " + vertices.Length + ". Triangles: " + indices.Length / 3 + ". Occupied voxels: " + voxelCenters.Length + ".");

			// If the debugging game object is enabled in the Unity scene, update its mesh.
			if (m_subtractiveTool1Debug != null)
			{
				Mesh debugMesh = m_subtractiveTool1Debug.GetComponent<MeshFilter>().mesh;

				debugMesh.Clear();

				debugMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

				debugMesh.vertices = vertices;
				debugMesh.normals = normals;
				debugMesh.triangles = indices;
			}

			// Add the tool to the generator. The localToWorldMatrix is used for collision detection.
			// We set the tool type to Subtractive (removes material on collision). Finally we set
			// the tool states to "active" (i.e. enabled). During runtime, press the "1" key on your
			// keyboard to activate/deactivate the tool.
			m_generator.addMultiTool(m_subtractiveTool1Id, voxelCenters, m_subtractiveTool1VoxelSize, m_subtractiveTool1.transform.localToWorldMatrix, ToolType.MultiSubtractive, true);
		}

		// Add tool 2, similar to tool 1.
		if (m_subtractiveTool2 != null)
		{
			Mesh mesh = m_subtractiveTool2.GetComponent<MeshFilter>().sharedMesh;

			m_voxelizer.voxelize(mesh.vertices, mesh.triangles, m_subtractiveTool2VoxelSize, VoxelizationFlags.BuildVoxels | VoxelizationFlags.BuildVoxelCenters);

			Vector3[] voxelCenters;
			Vector3[] vertices;
			Vector3[] normals;
			int[] indices;

			m_voxelizer.getVoxels(out vertices, out normals, out indices);
			m_voxelizer.getVoxelCenters(out voxelCenters);

			Debug.Log("SubtractiveTool2 mesh voxelized. Voxel size: " + m_subtractiveTool2VoxelSize + ". Vertices: " + vertices.Length + ". Triangles: " + indices.Length / 3 + ". Occupied voxels: " + voxelCenters.Length + ".");

			if (m_subtractiveTool2Debug != null)
			{
				Mesh debugMesh = m_subtractiveTool2Debug.GetComponent<MeshFilter>().mesh;

				debugMesh.Clear();

				debugMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

				debugMesh.vertices = vertices;
				debugMesh.normals = normals;
				debugMesh.triangles = indices;
			}

			m_generator.addMultiTool(m_subtractiveTool2Id, voxelCenters, m_subtractiveTool2VoxelSize, m_subtractiveTool2.transform.localToWorldMatrix, ToolType.MultiSubtractive, true);
		}
	}

	void OnApplicationQuit()
	{
		// Always destroy the instances on application quit to avoid surprises, especially when
		// running the DLL in the editor ...

		if (m_voxelizer != null)
		{
			m_voxelizer.destroy();
		}

		if (m_generator != null)
		{
			m_generator.destroy();
		}
	}

	void Update()
	{
		// Apply the keyboard shortcut actions used in this demo.
		processKeyboard();

		// Optional turntable animation.
		if (m_turntableOn)
		{
			// Gradually increase m_turntableAngleStep (i.e. angular velocity) until it reaches
			// m_turntableSpeed. From there on, m_turntableAngleStep remains constant and equal to
			// m_turntableSpeed. At each frame, increment m_turntableAngle by m_turntableAngleStep
			// and use it as the rotation angle about the Y axis.

			m_turntableAngleStep = Mathf.Min(m_turntableAngleStep + m_turntableAccelaration, m_turntableSpeed);
			m_turntableAngle += m_turntableAngleStep;

			if (m_solid != null)
			{
				m_solid.transform.localRotation = Quaternion.AngleAxis(m_turntableAngle, Vector3.up);
			}
		}
		else if (m_turntableAngleStep > 0.0f)
		{
			// Deceleration, i.e. decrement m_turntableAngleStep until it becomes zero.

			m_turntableAngleStep = Mathf.Max(m_turntableAngleStep - m_turntableAccelaration, 0.0f);
			m_turntableAngle += m_turntableAngleStep;

			if (m_solid != null)
			{
				m_solid.transform.localRotation = Quaternion.AngleAxis(m_turntableAngle, Vector3.up);
			}
		}

		// Notify the generator that the solid's and tools' transformations might have changed. In
		// this demo we assume that, in order to do the "carving", you change these transformations
		// with your mouse (or some other controller) from within Unity's editor.

		if (m_solid != null)
		{
			m_generator.setSolidWorldToLocalMatrix(m_solid.transform.worldToLocalMatrix);
		}

		if (m_subtractiveTool1 != null)
		{
			m_generator.setToolLocalToWorldMatrix(m_subtractiveTool1Id, m_subtractiveTool1.transform.localToWorldMatrix);
		}

		if (m_subtractiveTool2 != null)
		{
			m_generator.setToolLocalToWorldMatrix(m_subtractiveTool2Id, m_subtractiveTool2.transform.localToWorldMatrix);
		}

		// Regenerate the solid's mesh.
		updateMesh();
	}

	void updateMesh()
	{
		if (m_solid == null)
		{
			return;
		}

		// Generator.generate() generates the solid's mesh. If the mesh has been regenerated it
		// will return true. If nothing has changed since the last call to generate() and none
		// of the active tools collided with the solid, it will return false.
		if (!m_generator.generate())
		{
			return; // Mesh did not change, no need to do anything.
		}

		// Retrieve the mesh.

		Vector3[] vertices;
		Vector3[] normals;
		int[] indices;

		m_generator.getMesh(out vertices, out normals, out indices);

		// Replace the m_solid's mesh.

		Mesh mesh = m_solid.GetComponent<MeshFilter>().mesh;

		mesh.Clear();

		mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.triangles = indices;

		Debug.Log("Mesh updated: " + vertices.Length + " vertices, " + indices.Length / 3 + " triangles");
	}

	void updateTexturingShader()
	{
		// We rely on a tri-planar shader to generate texture coordinates and texture the object's
		// surface. The provided shader is only basic, you can write a more advanced tri-planar
		// shader if you prefer. The provided shader requires the solid's bounding volume to scale
		// the texture coordinates so that one texture wraps exactly once. If your texture is
		// seamless (the provided is not) you can modify the shader and skip this method call to
		// let the texture wrap. Note that the shader was created with ShaderGraph, so you should
		// install ShaderGraph from Unity's package manager (the shader was created with ShaderGraph
		// version 14.0.10).

		Vector3 worldBoundsMin;
		Vector3 worldBoundsMax;

		m_generator.getWorldBounds(out worldBoundsMin, out worldBoundsMax);

		m_solid.GetComponent<Renderer>().material.SetVector("_WorldBoundsMin", worldBoundsMin);
		m_solid.GetComponent<Renderer>().material.SetVector("_WorldBoundsMax", worldBoundsMax);
	}

	void processKeyboard()
	{
		if (Input.GetKeyDown(KeyCode.Home))
		{
			m_generator.restart(); // Revert to the initial template
		}
		else if (Input.GetKeyDown(KeyCode.S))
		{
			// Save the current geometry to be reverted later. Currently, each snapshot consumes some
			// memory, so we don't automatically push to the undo stack from within the DLL whenever
			// something changes. Instead we provide this function for the user to push to the undo
			// stack at specific moments of their choice.
			m_generator.pushUndo();
		}
		else if (Input.GetKeyDown(KeyCode.Z))
		{
			// Restore the last pushed geometry. Nothing happens if the undo stack is empty.
			m_generator.popUndo();
		}
		else if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			// Activate or deactivate the tool. Intended use of the active/inactive state of tools
			// is to prevent accidental modification of the solid as you move the tools or solid.
			m_generator.activateTool(m_subtractiveTool1Id, m_generator.isToolActive(m_subtractiveTool1Id) ? false : true);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			// Activate or deactivate the tool. Intended use of the active/inactive state of tools
			// is to prevent accidental modification of the solid as you move the tools or solid.
			m_generator.activateTool(m_subtractiveTool2Id, m_generator.isToolActive(m_subtractiveTool2Id) ? false : true);
		}
		else if (Input.GetKeyDown(KeyCode.T))
		{
			// Save the current geometry as a file. Intended use is to create different "starting
			// points" (during development), for the user to select from (in release build). The
			// saved template can be loaded with Generator.setTemplate().
			m_generator.saveAsTemplate(Application.streamingAssetsPath + "/customTemplate.png");
		}
		else if (Input.GetKeyDown(KeyCode.L))
		{
			// Load a custom template instead of the provided one.
			m_generator.setTemplate(Application.streamingAssetsPath + "/customTemplate.png");
			updateTexturingShader();
		}
		else if (Input.GetKeyDown(KeyCode.E))
		{
			// Export the solid's triangular mesh as Wavefront obj file.
			saveSolidAsObj();
		}
		else if (Input.GetKeyDown(KeyCode.A))
		{
			// Toggle turntable animation on or off.
			m_turntableOn = !m_turntableOn;
		}
		else if (Input.GetKeyDown(KeyCode.V))
		{
			// Show/hide the debug visualization window.
			m_generator.enableVisualization(!m_generator.isVisualizationEnabled());
		}
	}

	void saveSolidAsObj()
	{
		if (m_solid == null)
		{
			Debug.Log("No solid available");

			return;
		}

		Vector3[] vertices;
		Vector3[] normals;
		int[] indices;

		m_generator.getMesh(out vertices, out normals, out indices);

		string filename = Application.streamingAssetsPath + "/Solid.obj";

		writeObj(filename, vertices, normals, indices);

		Debug.Log("Solid model saved to " + filename);
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

				for (int i = 0; i < normals.Length; i++)
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
		// Logging callback should not throw exceptions

		try
		{
			Debug.Log(message);
		}
		catch
		{
		}
	}
}
