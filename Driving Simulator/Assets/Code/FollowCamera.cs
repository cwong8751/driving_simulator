using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -10);
    public float smoothSpeed = 5f;

    public float rotationSpeed = 100f;
    public float zoomSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 10f;

    private float currentZoom = 5f;
    private float yaw = 0f;
    private float pitch = 20f;

    private bool manualRotation = false;

    void Update()
    {
        if (!target) return;

        // Mouse input for manual rotation
        if (Input.GetMouseButton(1)) // Right mouse button held
        {
            manualRotation = true;
            yaw += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, 10f, 80f); // Limit pitch to avoid flipping
        }
        else
        {
            manualRotation = false;
        }

        // Scroll input for zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentZoom -= scroll * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
    }

    void LateUpdate()
    {
        if (!target) return;

        if (!manualRotation)
        {
            // Automatically align yaw to target's Y rotation (smoothly)
            float targetYaw = target.eulerAngles.y;
            yaw = Mathf.LerpAngle(yaw, targetYaw, smoothSpeed * Time.deltaTime);
        }

        // Calculate desired camera position
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredPosition = target.position + rotation * new Vector3(0, 0, -currentZoom) + offset;

        // Smooth movement
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Always look at the target
        transform.LookAt(target.position + offset);
    }
}
