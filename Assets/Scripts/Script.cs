using UnityEngine;
using RevolutionSolid;
using System;
using System.Collections;
using UnityEditor;


public class Script : MonoBehaviour
{
	// The generated solid. This is a Unity game object. We regularly replace its mesh with a new
	// one in updateMesh(). We use a cube game object because the cube's default mesh's dimensions
	// are similar to the dimensions of the generated mesh, using the provided template, so we can
	// build our scene around it in Unity's editor (the actual mesh is only generated during
	// runtime). However, you can use any game object, even one with an empty mesh. You can apply
	// transformations to it, but only uniform scaling is currently supported.
	GameObject m_solid = null;

	// The tool objects. These area sphere game objects. You can apply transformations to them, but
	// only uniform scaling is currently supported. During runtime, move the spheres closer to the
	// solid (by applying translation). When the spheres collide with the solid they will "carve" it.
	public GameObject m_subtractiveTool = null;
	public GameObject m_additiveTool = null;
	public GameObject m_massPreservingTool = null;

	public GameObject m_triangleTool = null;
	public GameObject m_squareTool = null;
	int m_triangleTool1Id = 4;
	int m_squareToolId = 5;
	float m_triangleTool1VoxelSize = 0.005f;
	float m_squareToolVoxelSize = 0.005f;
	//Triangle and Square gameObjects
	public GameObject triangleToolFullObject;
	public GameObject squareToolFullObject;
	// The generator instance. Created in Start(), destroyed in OnApplicationQuit().
	public Generator m_generator = null;
	Voxelizer m_voxelizer = null;

	// If true, apply rotation to the solid about its Y axis to emulate the solid being upon a
	// turntable.
	public bool animate = true;

	// These are used for the solid animation, if enabled.
	//float m_angle = 0.0f;
	//float m_angleStep = 0.0f;

	// If true, apply rotation to the solid about its Y axis to emulate the solid being upon a
	// turntable.
	[SerializeField]
	public bool m_turntableOn = true;

	// Speed of the turntable animation (degrees per frame).
	[SerializeField]
	[Range(0.0f, 20.0f)]
	public float m_turntableSpeed = 8.0f;

	// Acceleration/deceleration.
	[SerializeField]
	[Range(0.0f, 1.0f)]
	float m_turntableAccelaration = 0.1f;

	// These are used for the turntable animation, if enabled.
	float m_turntableAngle = 0.0f;
	float m_turntableAngleStep = 0.0f;

	public bool raycastHit;
	public bool enableOrbitCameraMode;
	MeshCollider meshCollider;

	public GameObject toolUIConnector;
	public GameObject camera;
	private int toolIndex;

	public float raycastInterval = 0.5f; // Time in seconds between each raycast
	private float nextRaycastTime = 0f;

	public float subtractiveToolStrength;

	private int undoStackCounter;
	public int undo_stack_index;
	void Start()
	{
		m_solid = GameObject.Find("Solid");
		m_subtractiveTool = GameObject.Find("SubtractiveTool");
		m_additiveTool = GameObject.Find("AdditiveTool");
		m_massPreservingTool = GameObject.Find("MassPreservingTool");
		m_triangleTool = GameObject.Find("TriangleToolTip");
		m_squareTool = GameObject.Find("SquareToolTip");
		triangleToolFullObject = GameObject.Find("TriangleTool");
		squareToolFullObject = GameObject.Find("SquareTool");
		subtractiveToolStrength = 0.17f;
		// Set up debug logging for the DLL. Messages will be printed if something goes wrong (for
		// example invalid argument passed to method). You should comment out this in release build.
		Generator.setLoggingCallback(onMessage);
		undoStackCounter = 0;
		undo_stack_index = 50;
		// Create the generator.
		m_generator = new Generator();
		m_voxelizer = new Voxelizer();

		// Set a template to be used as a starting point for carving. The provided template is
		// a white rectangle, which generates a cylinder. You can use other templates, either created
		// with a painting application, or one that was saved with a call to Generator.saveAsTemplate().
		// Many Generator methods will produce an error if a template is not set before calling. You
		// can change the template at any time, even during runtime.
		m_generator.setTemplate(Application.streamingAssetsPath + "/template2.png");
		//m_generator.setTemplate(Application.streamingAssetsPath + "/customTemplate.png");
		updateTexturingShader();
		// Add our tools. We choose 1, 2 and 3 as the tool IDs. The localToWorldMatrix is used for
		// collision detection. We set the tool types to Subtractive (removes material on collision),
		// Additive (adds material on collision) and MassPreserving (moves material on collision).
		// Finally we set the tool states to "active" (i.e. enabled). During runtime, press "1", "2"
		// or "3" key on your keyboard to activate/deactivate the corresponding tool.
		m_generator.addTool(1, m_subtractiveTool.GetComponent<Renderer>().localToWorldMatrix, ToolType.Subtractive, true);
		m_generator.addTool(2, m_additiveTool.GetComponent<Renderer>().localToWorldMatrix, ToolType.Additive, true);
		m_generator.addTool(3, m_massPreservingTool.GetComponent<Renderer>().localToWorldMatrix, ToolType.MassPreserving, true);

		// We choose to indicate the "active" tool state with a bright color and the "inactive" with
		// a dark color. The state and corresponding color is changed in processKeyboard().
		m_subtractiveTool.GetComponent<Renderer>().material.color = m_generator.isToolActive(1) ? new Color(1, 0, 0) : new Color(0.25f, 0, 0);
		m_additiveTool.GetComponent<Renderer>().material.color = m_generator.isToolActive(2) ? new Color(0, 1, 0) : new Color(0, 0.25f, 0);
		m_massPreservingTool.GetComponent<Renderer>().material.color = m_generator.isToolActive(3) ? new Color(1, 1, 0) : new Color(0.25f, 0.25f, 0);

		// Add tool 1
		if (m_triangleTool != null)
		{
			Mesh mesh = m_triangleTool.GetComponent<MeshFilter>().sharedMesh;

			// Voxelize the mesh. Produce both a triangle mesh representing the voxels, which we
			// will use for debugging and the voxel centers, which are used for collision detection.
			// Note that if the voxelSize parameter is too small the method will take a long time to
			// return. Extremely small values may lead to memory exhaustion and crashes...

			m_voxelizer.voxelize(mesh.vertices, mesh.triangles, m_triangleTool1VoxelSize, VoxelizationFlags.BuildVoxels | VoxelizationFlags.BuildVoxelCenters);

			// Retrieve voxelization results.

			Vector3[] voxelCenters;
			Vector3[] vertices;
			Vector3[] normals;
			int[] indices;

			m_voxelizer.getVoxels(out vertices, out normals, out indices);
			m_voxelizer.getVoxelCenters(out voxelCenters);

			Debug.Log("SubtractiveTool1 mesh voxelized. Voxel size: " + m_triangleTool1VoxelSize + ". Vertices: " + vertices.Length + ". Triangles: " + indices.Length / 3 + ". Occupied voxels: " + voxelCenters.Length + ".");

			// Add the tool to the generator. The localToWorldMatrix is used for collision detection.
			// We set the tool type to Subtractive (removes material on collision). Finally we set
			// the tool states to "active" (i.e. enabled). During runtime, press the "1" key on your
			// keyboard to activate/deactivate the tool.
			m_generator.addMultiTool(m_triangleTool1Id, voxelCenters, m_triangleTool1VoxelSize, m_triangleTool.transform.localToWorldMatrix, ToolType.MultiSubtractive, true);
		}

		// Add additive multi tool 
		if (m_squareTool != null)
		{
			Mesh mesh = m_squareTool.GetComponent<MeshFilter>().sharedMesh;

			// Voxelize the mesh. Produce both a triangle mesh representing the voxels, which we
			// will use for debugging and the voxel centers, which are used for collision detection.
			// Note that if the voxelSize parameter is too small the method will take a long time to
			// return. Extremely small values may lead to memory exhaustion and crashes...

			m_voxelizer.voxelize(mesh.vertices, mesh.triangles, m_squareToolVoxelSize, VoxelizationFlags.BuildVoxels | VoxelizationFlags.BuildVoxelCenters);

			// Retrieve voxelization results.

			Vector3[] voxelCenters;
			Vector3[] vertices;
			Vector3[] normals;
			int[] indices;

			m_voxelizer.getVoxels(out vertices, out normals, out indices);
			m_voxelizer.getVoxelCenters(out voxelCenters);

			Debug.Log("Additive Multi Tool mesh voxelized. Voxel size: " + m_squareToolVoxelSize + ". Vertices: " + vertices.Length + ". Triangles: " + indices.Length / 3 + ". Occupied voxels: " + voxelCenters.Length + ".");

			// Add the tool to the generator. The localToWorldMatrix is used for collision detection.
			// We set the tool type to Subtractive (removes material on collision). Finally we set
			// the tool states to "active" (i.e. enabled). During runtime, press the "1" key on your
			// keyboard to activate/deactivate the tool.
			m_generator.addMultiTool(m_squareToolId, voxelCenters, m_squareToolVoxelSize, m_squareTool.transform.localToWorldMatrix, ToolType.MultiSubtractive, true);
		}
		// Note that you can add or remove tools during runtime. You can add as many tools as you
		// want but keep in mind that tools consume CPU time when active (no CPU impact when the
		// tool is inactive).

		// Optionally, setup some parameters below. All parameters can also be changed during
		// runtime (for example in Update() method).

		// Generate high quality mesh (many small triangles in the Y axis). You can inspect the
		// difference in quality if you enable wireframe rendering in Unity.
		//m_generator.setResolution(100); // Default is 60

		// Generate high quality mesh (many small triangles in the XZ plane). You can inspect the
		// difference in quality if you enable wireframe rendering in Unity.
		//m_generator.setSlices(180); // Default is 72

		// Smoothing is enabled by default. You can change the amount of smoothing by changing the
		// kernel size and deviation. If you use excessive amounts of smoothing, the generated mesh
		// might look unrealistic.
		//m_generator.enableSmoothing(true); // Default is true
		//m_generator.setSmoothingKernelSize(15); // Default is 5
		//m_generator.setSmoothingKernelDeviation(100); // Default is 0

		// Show debug visualization window. You should not use it this release.
		//m_generator.enableVisualization();

		// Setup parameters for the texturing shader. This should be called whenever you change the
		// template (i.e. when you call Generator.setTemplate()).
		updateTexturingShader();
		m_generator.clearUndoStack();
		meshCollider = m_solid.GetComponent<MeshCollider>();
		enableOrbitCameraMode = true;
		m_additiveTool.SetActive(false);
		m_subtractiveTool.SetActive(false);
		m_massPreservingTool.SetActive(false);
		triangleToolFullObject.SetActive(false);
		squareToolFullObject.SetActive(false);
	}

	void OnApplicationQuit()
	{
		// Always destroy the instance on application quit to avoid surprises, especially when
		// running the DLL in the editor ...
		raycastInterval = 0;
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
		// Optional animation.
		/*
		if (animate)
		{
			m_angleStep = Mathf.Min(m_angleStep + 0.01f, 10.0f);
			m_angle += m_angleStep;

			m_solid.transform.localRotation = Quaternion.AngleAxis(m_angle, new Vector3(0, 1, 0));
		}
		*/
		if (Input.GetMouseButton(0))
		{
			if (Time.time >= nextRaycastTime)
			{
				OnMouseClick();
				nextRaycastTime = Time.time + raycastInterval;
			}
		}

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


		// Apply the key shortcuts used for this demo.
		processKeyboard();

		// Notify the generator that the solid's and tools' transformations might have changed. In
		// this demo we assume that, in order to do the "carving", you change these transformations
		// with your mouse (or some other controller) from within Unity's editor.
		m_generator.setSolidWorldToLocalMatrix(m_solid.GetComponent<Renderer>().worldToLocalMatrix);
		m_generator.setToolLocalToWorldMatrix(1, m_subtractiveTool.GetComponent<Renderer>().localToWorldMatrix);
		m_generator.setToolLocalToWorldMatrix(2, m_additiveTool.GetComponent<Renderer>().localToWorldMatrix);
		m_generator.setToolLocalToWorldMatrix(3, m_massPreservingTool.GetComponent<Renderer>().localToWorldMatrix);

		if (m_triangleTool != null)
		{
			m_generator.setToolLocalToWorldMatrix(m_triangleTool1Id, m_triangleTool.transform.localToWorldMatrix);
		}

		if (m_squareTool != null)
		{
			m_generator.setToolLocalToWorldMatrix(m_squareToolId, m_squareTool.transform.localToWorldMatrix);
		}
		toolIndex = toolUIConnector.GetComponent<PotterySimulatorToolUIConnector>().toolIndex;
		// Regenerate the solid's mesh.
		updateMesh();
	}

	void updateMesh()
	{
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

		// Replace the m_solid's mesh. There is probably a more optimal way to do this in Unity...

		Mesh mesh = m_solid.GetComponent<MeshFilter>().mesh;

		mesh.Clear();

		mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.triangles = indices;

		//Update mesh collider to fit the deformed gameobject
		if (meshCollider == null)
		{
			meshCollider = (MeshCollider)m_solid.AddComponent(typeof(MeshCollider));
		}

		else
		{
			MeshFilter mf = m_solid.GetComponent<MeshFilter>();
			Mesh sculptMesh = mf.mesh;
			meshCollider.sharedMesh = null;
			meshCollider.sharedMesh = sculptMesh;
		}

		Debug.Log("Mesh updated: " + vertices.Length + " vertices, " + indices.Length / 3 + " triangles");

	}

	public void updateTexturingShader()
	{
		// Currently the DLL does not generate texture coordinates. Instead we rely on a tri-planar
		// shader to generate texture coordinates and texture the object. The provided shader is
		// only basic, you can write a more advanced tri-planar shader if you prefer. The provided
		// shader requires the solid's bounding volume to scale the texture coordinates so that
		// one texture wraps exactly once. If your texture is seamless (the provided is not) you
		// can modify the shader and skip this method call to let the texture wrap.
		// Note that the shader was created with ShaderGraph, so you should install ShaderGraph
		// from Unity's package manager (the shader was created with ShaderGraph version 14.0.10).

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
		//else if (Input.GetKeyDown(KeyCode.Z))
		//{
			// Restore the last pushed geometry. Nothing happens if the undo stack is empty.
			//m_generator.popUndo();
		//}
		else if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			// Activate or deactivate the tool. Intended use of the active/inactive state of tools
			// is to prevent accidental modification of the solid as you move the tools or solid.

			if (m_generator.isToolActive(1))
			{
				m_generator.activateTool(1, false);
				m_subtractiveTool.GetComponent<Renderer>().material.color = new Color(0.25f, 0, 0);
			}
			else
			{
				m_generator.activateTool(1, true);
				m_subtractiveTool.GetComponent<Renderer>().material.color = new Color(1, 0, 0);
			}
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			// Activate or deactivate the tool. Intended use of the active/inactive state of tools
			// is to prevent accidental modification of the solid as you move the tools or solid.

			if (m_generator.isToolActive(2))
			{
				m_generator.activateTool(2, false);
				m_additiveTool.GetComponent<Renderer>().material.color = new Color(0, 0.25f, 0);
			}
			else
			{
				m_generator.activateTool(2, true);
				m_additiveTool.GetComponent<Renderer>().material.color = new Color(0, 1, 0);
			}
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			// Activate or deactivate the tool. Intended use of the active/inactive state of tools
			// is to prevent accidental modification of the solid as you move the tools or solid.

			if (m_generator.isToolActive(3))
			{
				m_generator.activateTool(3, false);
				m_massPreservingTool.GetComponent<Renderer>().material.color = new Color(0.25f, 0.25f, 0);
			}
			else
			{
				m_generator.activateTool(3, true);
				m_massPreservingTool.GetComponent<Renderer>().material.color = new Color(1, 1, 0);
			}
		}
		else if (Input.GetKeyDown(KeyCode.Alpha8))
		{
			//m_generator.setTemplate(Application.streamingAssetsPath + "/template.png");
			updateTexturingShader();
		}
		else if (Input.GetKeyDown(KeyCode.Alpha9))
		{
			//m_generator.setTemplate(Application.streamingAssetsPath + "/customTemplate.png");
			updateTexturingShader();
		}
		else if (Input.GetKeyDown(KeyCode.T))
		{
			// Save the current geometry as a file. Intended use is to create different "starting
			// points" (during development), for the user to select from (in release build). The
			// saved template can be loaded with Generator.setTemplate().
			//m_generator.saveAsTemplate(Application.streamingAssetsPath + "/customTemplate.png");
		}
		else if (Input.GetKeyDown(KeyCode.V))
		{
			// Show/hide the debug visualization window.
			m_generator.enableVisualization(!m_generator.isVisualizationEnabled());
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

	void OnMouseClick()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Debug.DrawRay(ray.origin, ray.direction * 20f, Color.red, 15f);
		Vector3 objectToCenterSolidDirection;
		raycastHit = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity);
		if (!raycastHit)
		{
			return;
		}

		if (hit.collider.gameObject != m_solid)
		{
			return;
		}

		Debug.Log(hit.transform.gameObject.name);

		Vector3 solidXYVector = new Vector3(m_solid.transform.position.x, hit.point.y, m_solid.transform.position.y);
		objectToCenterSolidDirection = solidXYVector - hit.point;
		if (camera.GetComponent<OrbitCamera>().enableCameraMode == false)
		{
			if (toolIndex == 1)
			{
				m_subtractiveTool.transform.position = hit.point - objectToCenterSolidDirection * subtractiveToolStrength;
				m_subtractiveTool.SetActive(true);
				m_additiveTool.SetActive(false);
				m_massPreservingTool.SetActive(false);
			}
			else if (toolIndex == 2)
			{
				m_additiveTool.transform.position = hit.point;
				m_additiveTool.SetActive(true);
				m_massPreservingTool.SetActive(false);
				m_subtractiveTool.SetActive(false);
			}
			else if (toolIndex == 3)
			{
				m_massPreservingTool.transform.position = hit.point;
				m_additiveTool.SetActive(false);
				m_massPreservingTool.SetActive(true);
				m_subtractiveTool.SetActive(false);
			}
			else if (toolIndex == 4)
			{
				/*
				m_triangleTool.transform.position = hit.point;
				m_triangleTool.transform.rotation = Quaternion.LookRotation(objectToCenterSolidDirection);
				*/
				m_additiveTool.SetActive(false);
				m_massPreservingTool.SetActive(false);
				m_subtractiveTool.SetActive(false);
				m_triangleTool.SetActive(true);
				m_squareTool.SetActive(false);
				
				
			}
			else if (toolIndex == 5)
			{
				/*
				m_squareTool.transform.position = hit.point;
				m_squareTool.transform.rotation = Quaternion.LookRotation(objectToCenterSolidDirection);
				*/
				m_additiveTool.SetActive(false);
				m_massPreservingTool.SetActive(false);
				m_subtractiveTool.SetActive(false);
				m_triangleTool.SetActive(false);
				m_squareTool.SetActive(true);
			}
			AddToUndoStack();
		}

	}


	//Add state into the undo stack. Right now we set standard number of 50 undos in the list but this may change with trial and error
	private void AddToUndoStack()
	{
		if (undoStackCounter <= undo_stack_index)
		{
			m_generator.pushUndo();
		}
		else
		{
			m_generator.clearUndoStack();
			m_generator.pushUndo();
		}
	}

	public void EnableToolControls(GameObject tool)
	{
		tool.GetComponent<ToolController>().controlsEnabled = true;
	}

	public void DisableToolControl(GameObject tool)
	{
		tool.GetComponent<ToolController>().controlsEnabled = false;
	}

    
}
