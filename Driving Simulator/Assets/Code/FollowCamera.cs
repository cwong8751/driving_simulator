using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;           // Your car (CarRoot)
    public Vector3 offset = new Vector3(0, 5, -10);
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        transform.LookAt(target);
    }
}
