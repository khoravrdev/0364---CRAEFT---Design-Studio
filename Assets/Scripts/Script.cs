using UnityEngine;
using RevolutionSolid;
using System;

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
	GameObject m_subtractiveTool = null;
	GameObject m_additiveTool = null;
	GameObject m_massPreservingTool = null;

	// The generator instance. Created in Start(), destroyed in OnApplicationQuit().
	Generator m_generator = null;

	// If true, apply rotation to the solid about its Y axis to emulate the solid being upon a
	// turntable.
	public bool animate = true;

	// These are used for the solid animation, if enabled.
	float m_angle = 0.0f;
	float m_angleStep = 0.0f;
	
	public bool raycastHit;
	public bool enableOrbitCameraMode;
	MeshCollider meshCollider;
	void Start()
	{
		m_solid = GameObject.Find("Solid");
		m_subtractiveTool = GameObject.Find("SubtractiveTool");
		m_additiveTool = GameObject.Find("AdditiveTool");
		m_massPreservingTool = GameObject.Find("MassPreservingTool");

		// Set up debug logging for the DLL. Messages will be printed if something goes wrong (for
		// example invalid argument passed to method). You should comment out this in release build.
		Generator.setLoggingCallback(onMessage);

		// Create the generator.
		m_generator = new Generator();

		// Set a template to be used as a starting point for carving. The provided template is
		// a white rectangle, which generates a cylinder. You can use other templates, either created
		// with a painting application, or one that was saved with a call to Generator.saveAsTemplate().
		// Many Generator methods will produce an error if a template is not set before calling. You
		// can change the template at any time, even during runtime.
		m_generator.setTemplate(Application.streamingAssetsPath + "/template.png");
		//m_generator.setTemplate(Application.streamingAssetsPath + "/customTemplate.png");

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

		meshCollider = m_solid.GetComponent<MeshCollider>();
		enableOrbitCameraMode = true;

	}

	void OnApplicationQuit()
	{
		// Always destroy the instance on application quit to avoid surprises, especially when
		// running the DLL in the editor ...
		m_generator.destroy();
	}

	void Update()
	{
		// Optional animation.

		if (animate)
		{
			m_angleStep = Mathf.Min(m_angleStep + 0.01f, 10.0f);
			m_angle += m_angleStep;

			m_solid.transform.localRotation = Quaternion.AngleAxis(m_angle, new Vector3(0, 1, 0));
		}

		if(Input.GetMouseButton(0))
		{	
			OnMouseClick();
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

		// Regenerate the solid's mesh.
		updateMesh();

		Debug.Log(enableOrbitCameraMode + " MESA S|TO SCRIPT");
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
		if(meshCollider == null)
		{
			meshCollider = (MeshCollider) m_solid.AddComponent(typeof(MeshCollider));
		}
		
		else
		{	
			MeshFilter mf = m_solid.GetComponent<MeshFilter>();
			Mesh sculptMesh = mf.mesh;
			meshCollider.sharedMesh =  null;
			meshCollider.sharedMesh = sculptMesh;
		}	
		
		Debug.Log("Mesh updated: " + vertices.Length + " vertices, " + indices.Length / 3 + " triangles");

	}

	void updateTexturingShader()
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
		else if (Input.GetKeyDown(KeyCode.Z))
		{
			// Restore the last pushed geometry. Nothing happens if the undo stack is empty.
			m_generator.popUndo();
		}
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
			m_generator.setTemplate(Application.streamingAssetsPath + "/template.png");
			updateTexturingShader();
		}
		else if (Input.GetKeyDown(KeyCode.Alpha9))
		{
			m_generator.setTemplate(Application.streamingAssetsPath + "/customTemplate.png");
			updateTexturingShader();
		}
		else if (Input.GetKeyDown(KeyCode.T))
		{
			// Save the current geometry as a file. Intended use is to create different "starting
			// points" (during development), for the user to select from (in release build). The
			// saved template can be loaded with Generator.setTemplate().
			m_generator.saveAsTemplate(Application.streamingAssetsPath + "/customTemplate.png");
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
		Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 15f);
		raycastHit = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity);
		if(!raycastHit)
		{
			enableOrbitCameraMode = true;
			Debug.Log("KENO");
			return;
		}

		if(hit.collider.gameObject != m_solid)
		{
			Debug.Log("Oxi SOLID");
			return;
		}			
		
		enableOrbitCameraMode = false;
		Debug.Log(hit.transform.gameObject.name);
		m_generator.activateTool(1, true);
		m_subtractiveTool.transform.position = hit.point;		
    } 
}
