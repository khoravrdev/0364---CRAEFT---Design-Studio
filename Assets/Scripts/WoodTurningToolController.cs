using UnityEngine;
using UnityEngine.UIElements;

public class WoodTurningToolController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float rotationSpeed = 100f;

    // Movement boundaries
    public Vector3 minPosition = new Vector3(-5f, 0f, -5f);
    public Vector3 maxPosition = new Vector3(5f, 5f, 5f);

    private enum Axis { None, X, Y, Z }
    private Axis selectedAxis = Axis.None;

    private Vector3 lastMousePosition;
    private Vector3 currentRotation = Vector3.zero; // Tracks cumulative rotation

    public GameObject solidGameObject;

    void Start()
    {
        if (solidGameObject == null)
        {
            solidGameObject = GameObject.Find("Solid");
        }
    }
    void Update()
    {
        HandleMovement();
        HandleAxisSelection();
        HandleRotation();
    }

    private void HandleMovement()
    {
        Vector3 movement = Vector3.zero;

        // Move along Z-axis with W/S
        if (Input.GetKey(KeyCode.W))
            movement += Vector3.forward;
        if (Input.GetKey(KeyCode.S))
            movement += Vector3.back;

        // Move along X-axis with A/D
        if (Input.GetKey(KeyCode.A))
            movement += Vector3.left;
        if (Input.GetKey(KeyCode.D))
            movement += Vector3.right;

        // Move along Y-axis with Q/E
        if (Input.GetKey(KeyCode.Q))
            movement += Vector3.up;
        if (Input.GetKey(KeyCode.E))
            movement += Vector3.down;

        Vector3 newPosition = transform.position + movement * moveSpeed * Time.deltaTime;

        // Apply constraints
        newPosition.x = Mathf.Clamp(newPosition.x, minPosition.x, maxPosition.x);
        newPosition.y = Mathf.Clamp(newPosition.y, minPosition.y, maxPosition.y);
        newPosition.z = Mathf.Clamp(newPosition.z, minPosition.z, maxPosition.z);

        transform.position = newPosition;
    }

    private void HandleAxisSelection()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            selectedAxis = Axis.X;
            solidGameObject.GetComponent<WoodTurningSimulationUIConnector>().xAxis.style.backgroundColor = new StyleColor(Color.grey);
            solidGameObject.GetComponent<WoodTurningSimulationUIConnector>().yAxis.style.backgroundColor = new StyleColor(Color.white);
            solidGameObject.GetComponent<WoodTurningSimulationUIConnector>().zAxis.style.backgroundColor = new StyleColor(Color.white);
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            selectedAxis = Axis.Y;
            solidGameObject.GetComponent<WoodTurningSimulationUIConnector>().yAxis.style.backgroundColor = new StyleColor(Color.grey);
            solidGameObject.GetComponent<WoodTurningSimulationUIConnector>().xAxis.style.backgroundColor = new StyleColor(Color.white);
            solidGameObject.GetComponent<WoodTurningSimulationUIConnector>().zAxis.style.backgroundColor = new StyleColor(Color.white);
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            selectedAxis = Axis.Z;
            solidGameObject.GetComponent<WoodTurningSimulationUIConnector>().zAxis.style.backgroundColor = new StyleColor(Color.grey);
            solidGameObject.GetComponent<WoodTurningSimulationUIConnector>().xAxis.style.backgroundColor = new StyleColor(Color.white);
            solidGameObject.GetComponent<WoodTurningSimulationUIConnector>().yAxis.style.backgroundColor = new StyleColor(Color.white);
        }
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1) && selectedAxis != Axis.None)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            float rotationAmount = 0f;

            switch (selectedAxis)
            {
                case Axis.X:
                    rotationAmount = -delta.y * rotationSpeed * Time.deltaTime;
                    currentRotation.x = Mathf.Clamp(currentRotation.x + rotationAmount, -90f, 90f);
                    break;
                case Axis.Y:
                    rotationAmount = delta.x * rotationSpeed * Time.deltaTime;
                    currentRotation.y = Mathf.Clamp(currentRotation.y + rotationAmount, -180f, 180f);
                    break;
                case Axis.Z:
                    rotationAmount = delta.x * rotationSpeed * Time.deltaTime;
                    currentRotation.z = Mathf.Clamp(currentRotation.z + rotationAmount, -90f, 90f);
                    break;
            }

            transform.localRotation = Quaternion.Euler(currentRotation);
            lastMousePosition = Input.mousePosition;
        }
    }
}
