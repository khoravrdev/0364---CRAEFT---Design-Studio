using UnityEngine;
using UnityEngine.EventSystems;

public class OrbitCamera : MonoBehaviour
{
	[Range(0.1f, 10.0f)]
	public float m_orbitRadius = 3.0f;

	public float m_orbitAngle = 0.0f;

	[Range(-90.0f, 90.0f)]
	public float m_elevation = 30.0f;

	[Range(-10.0f, 10.0f)]
	public float m_height = 0.0f;

	[Range(0.1f, 0.5f)]
	public float m_sensitivityX = 0.25f;

	[Range(0.1f, 0.5f)]
	public float m_sensitivityY = 0.25f;

	[Range(0.05f, 0.2f)]
	public float m_sensitivityWheel = 0.1f;

	Vector3 m_lastMouse;
	
	public GameObject Solid;
	bool raycastHitFromScript;
	public bool enableCameraMode;
	void Start()
	{
		if (Solid.GetComponent<Script>() != null)
		{
			raycastHitFromScript = Solid.GetComponent<Script>().raycastHit;	
			enableCameraMode = Solid.GetComponent<Script>().enableOrbitCameraMode;
			enableCameraMode = false;
		}
		
    }
    void Update()
	{
		if (enableCameraMode == true)
		{
			if (Input.GetMouseButtonDown(0))
			{
				m_lastMouse = Input.mousePosition;
			}
			else if (Input.GetMouseButton(0))
			{

				float dx = Input.mousePosition.x - m_lastMouse.x;
				float dy = Input.mousePosition.y - m_lastMouse.y;

				m_lastMouse = Input.mousePosition;

				m_orbitAngle += dx * m_sensitivityX;

				m_elevation = Mathf.Clamp(m_elevation - dy * m_sensitivityY, -90.0f, 90.0f);
			}
		}
			if (Input.mouseScrollDelta.y < 0.0f)
			{
				m_orbitRadius = Mathf.Clamp(m_orbitRadius * (1.0f - m_sensitivityWheel), 0.1f, 10.0f);
			}
			else if (Input.mouseScrollDelta.y > 0.0f)
			{
				m_orbitRadius = Mathf.Clamp(m_orbitRadius * (1.0f + m_sensitivityWheel), 0.1f, 10.0f);
			}

			Matrix4x4 m = Matrix4x4.identity;

			m *= Matrix4x4.Rotate(Quaternion.AngleAxis(m_orbitAngle, new Vector3(0, 1, 0)));
			m *= Matrix4x4.Translate(new Vector3(0, m_height, 0));
			m *= Matrix4x4.Rotate(Quaternion.AngleAxis(m_elevation, new Vector3(1, 0, 0)));
			m *= Matrix4x4.Translate(new Vector3(0, 0, -m_orbitRadius));

			transform.position = m.GetColumn(3);
			transform.rotation = m.rotation;

		
	}

}
