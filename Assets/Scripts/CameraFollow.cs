using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    Vector3 currentVelocity; // À ajouter en haut de la classe
    float rotationSmoothVelocity; // À ajouter aussi
    public float rotationSmoothTime = 0.1f; // Réglable dans l’inspector


    public Transform target;
    public Vector3 offset = new Vector3(0f, 3f, -6f);
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        // --- Position lissée (SmoothDamp au lieu de Lerp) ---
        Vector3 targetPosition = target.position + target.rotation * offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, 1f / smoothSpeed);

        // --- Rotation de la caméra lissée autour du joueur ---
        float targetYRotation = target.eulerAngles.y;
        float smoothYRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetYRotation, ref rotationSmoothVelocity, rotationSmoothTime);

        // Appliquer la rotation uniquement sur Y (horizontal), laisser la hauteur indépendante
        transform.rotation = Quaternion.Euler(0f, smoothYRotation, 0f);

        // --- Regarder le joueur (mais de manière stable, avec un léger offset vertical) ---
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
