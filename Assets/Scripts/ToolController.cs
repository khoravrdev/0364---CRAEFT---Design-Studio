using UnityEngine;
using UnityEngine.UIElements;

public class ToolController : MonoBehaviour
{
    public GameObject tool;
    public float rotationSpeed = 100f;
    public float moveSpeed = 2f;
    public bool controlsEnabled = false;
    private enum RotationAxis { None, X, Y, Z }
    private RotationAxis selectedAxis = RotationAxis.None;

    private bool isRightMouseHeld = false;
    private Vector2 lastMousePosition;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private float zRotation = 0f;

    private float minY = -5f;
    private float maxY = 5f;
    private float minX = -5f;
    private float maxX = 5f;

    public GameObject toolUIConnector;
    void Start()
    {
        tool = this.gameObject;
    }
    void Update()
    {
        if (!controlsEnabled) return;
        HandleAxisSelection();
        HandleRotation();
        HandleMovement();
    }

    void HandleAxisSelection()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            selectedAxis = RotationAxis.X;
            toolUIConnector.GetComponent<PotterySimulatorToolUIConnector>().zAxis.style.backgroundColor = new StyleColor(Color.grey);
            toolUIConnector.GetComponent<PotterySimulatorToolUIConnector>().xAxis.style.backgroundColor = new StyleColor(Color.white);
            toolUIConnector.GetComponent<PotterySimulatorToolUIConnector>().yAxis.style.backgroundColor = new StyleColor(Color.white);
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            selectedAxis = RotationAxis.Y;
            toolUIConnector.GetComponent<PotterySimulatorToolUIConnector>().yAxis.style.backgroundColor = new StyleColor(Color.grey);
            toolUIConnector.GetComponent<PotterySimulatorToolUIConnector>().zAxis.style.backgroundColor = new StyleColor(Color.white);
            toolUIConnector.GetComponent<PotterySimulatorToolUIConnector>().xAxis.style.backgroundColor = new StyleColor(Color.white);

        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            selectedAxis = RotationAxis.Z;
            toolUIConnector.GetComponent<PotterySimulatorToolUIConnector>().xAxis.style.backgroundColor = new StyleColor(Color.grey);
            toolUIConnector.GetComponent<PotterySimulatorToolUIConnector>().zAxis.style.backgroundColor = new StyleColor(Color.white);
            toolUIConnector.GetComponent<PotterySimulatorToolUIConnector>().yAxis.style.backgroundColor = new StyleColor(Color.white);
        }
            
    }

    void HandleRotation()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isRightMouseHeld = true;
            lastMousePosition = Input.mousePosition;

            Vector3 currentEuler = tool.transform.rotation.eulerAngles;
            xRotation = NormalizeAngle(currentEuler.x);
            yRotation = NormalizeAngle(currentEuler.y);
            zRotation = NormalizeAngle(currentEuler.z);
        }

        if (Input.GetMouseButtonUp(1))
        {
            isRightMouseHeld = false;
        }

        if (isRightMouseHeld && selectedAxis != RotationAxis.None)
        {
            Vector2 currentMouse = Input.mousePosition;
            Vector2 delta = currentMouse - lastMousePosition;
            lastMousePosition = currentMouse;

            float deltaRotation = 0f;

            switch (selectedAxis)
            {
                case RotationAxis.X:
                    deltaRotation = -delta.y * Time.deltaTime * rotationSpeed;
                    xRotation = Mathf.Clamp(xRotation + deltaRotation, -90f, 90f);
                    break;
                case RotationAxis.Y:
                    deltaRotation = delta.x * Time.deltaTime * rotationSpeed;
                    yRotation = Mathf.Clamp(yRotation + deltaRotation,0f, 180f);
                    break;
                case RotationAxis.Z:
                    deltaRotation = delta.x * Time.deltaTime * rotationSpeed;
                    zRotation = Mathf.Clamp(zRotation + deltaRotation, -90f, 90f);
                    break;
            }

            tool.transform.rotation = Quaternion.Euler(xRotation, yRotation, zRotation);
        }
    }

    void HandleMovement()
    {
        Vector3 position = tool.transform.position;

        if (Input.GetKey(KeyCode.W))
        {
            position.y += moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            position.y -= moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            position.x -= moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            position.x += moveSpeed * Time.deltaTime;
        }

        position.y = Mathf.Clamp(position.y, minY, maxY);
        position.x = Mathf.Clamp(position.x, minX, maxX);

        tool.transform.position = position;
    }

    float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
