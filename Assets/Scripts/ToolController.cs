using UnityEngine;

public class ToolController : MonoBehaviour
{

    public bool controlsEnabled = false;
    public float moveSpeed = 2f;
    public float rotationSpeed = 100f;

    private float yMin = -5f;
    private float yMax = 5f;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private float zRotation = 0f;

    private bool isControlling = false;
    private bool isRightMouseHeld = false;
    private Vector2 lastMousePosition;
    private bool hasStartedDragging = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void Update()
    {
        if (!controlsEnabled) return;

        Vector3 pos = transform.position;

        // W/S → move along Y axis
        if (Input.GetKey(KeyCode.W))
        {
            pos.y += moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            pos.y -= moveSpeed * Time.deltaTime;
        }

        // A/D → move along X axis
        if (Input.GetKey(KeyCode.A))
        {
            pos.x -= moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            pos.x += moveSpeed * Time.deltaTime;
        }

        // Clamp Y position to stay within the specified range
        pos.y = Mathf.Clamp(pos.y, yMin, yMax);

        transform.position = pos;

        if (Input.GetKey(KeyCode.Q))
        {
            zRotation += rotationSpeed * Time.deltaTime;
            this.transform.rotation = Quaternion.Euler(xRotation, yRotation, zRotation);
        }
        if (Input.GetKey(KeyCode.E))
        {
            zRotation -= rotationSpeed * Time.deltaTime;
            this.transform.rotation = Quaternion.Euler(xRotation, yRotation, zRotation);
        }

        // Optional: Clamp Z rotation if needed
        zRotation = Mathf.Clamp(zRotation, -90f, 90f);
        


        HandleRotation();
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isRightMouseHeld = true;
            lastMousePosition = Input.mousePosition;
            hasStartedDragging = false;

            // Capture current tool rotation (in case it's not at 0)
            Vector3 currentEuler = this.transform.rotation.eulerAngles;

            // Normalize to -180 to 180 range
            xRotation = NormalizeAngle(currentEuler.x);
            yRotation = NormalizeAngle(currentEuler.y);
        }

        if (Input.GetMouseButtonUp(1))
        {
            isRightMouseHeld = false;
            hasStartedDragging = false;
        }

        if (isRightMouseHeld)
        {
            Vector2 currentMouse = Input.mousePosition;
            Vector2 delta = currentMouse - lastMousePosition;

            if (!hasStartedDragging)
            {
                // Ignore first delta to prevent jump
                hasStartedDragging = true;
            }
            else
            {
                // Only apply rotation on subsequent moves
                yRotation += delta.x * Time.deltaTime * (rotationSpeed * 0.1f);
                xRotation -= delta.y * Time.deltaTime * (rotationSpeed * 0.1f);

                yRotation = Mathf.Clamp(yRotation, 0.0f, 180f);
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);

                this.transform.rotation = Quaternion.Euler(xRotation, yRotation, zRotation);
            }

            lastMousePosition = currentMouse;
        }
    }
    
    float NormalizeAngle(float angle)
    {
        angle %= 360;
        if (angle > 180) angle -= 360;
        return angle;
    }
}
