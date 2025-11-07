using UnityEngine;

public class MenuCameraOrbit : MonoBehaviour
{
    public Transform target;
    public float rotationAmplitude = 20f; 
    public float rotationSpeed = 0.5f;    
    public float distance = 6f;         
    public float height = 2f;
    public float heightOffset;

    private float angleOffset;

    void Start()
    {
        if (target == null)
            enabled = false;
        angleOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void LateUpdate()
    {
        float angle = Mathf.Sin(Time.time * rotationSpeed + angleOffset) * rotationAmplitude;
        Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
        Vector3 offset = rotation * Vector3.back * distance + Vector3.up * height;

        transform.position = target.position + offset;
        transform.LookAt(target.position + Vector3.up * 1.5f + new Vector3(0, heightOffset,0));
    }
}
