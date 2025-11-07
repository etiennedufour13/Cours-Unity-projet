using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    Vector3 currentVelocity;
    float rotationSmoothVelocity;
    public float rotationSmoothTime = 0.1f;


    public Transform target;
    public Vector3 offset = new Vector3(0f, 3f, -6f);
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        Vector3 targetPosition = target.position + target.rotation * offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, 1f / smoothSpeed);

        float targetYRotation = target.eulerAngles.y;
        float smoothYRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetYRotation, ref rotationSmoothVelocity, rotationSmoothTime);

        transform.rotation = Quaternion.Euler(0f, smoothYRotation, 0f);

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
