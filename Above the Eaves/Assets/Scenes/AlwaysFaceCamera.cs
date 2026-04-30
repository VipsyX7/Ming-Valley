using UnityEngine;

/// <summary>
/// Makes this object always face a target camera.
/// </summary>
[ExecuteAlways]
public class AlwaysFaceCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Camera to face. If null, Camera.main is used.")]
    [SerializeField] private Camera targetCamera;

    [Header("Options")]
    [Tooltip("If enabled, only rotates around Y axis.")]
    [SerializeField] private bool onlyRotateY = false;

    [Tooltip("Rotate an extra 180 degrees to flip forward direction.")]
    [SerializeField] private bool flip = false;

    private void LateUpdate()
    {
        Camera cam = ResolveCamera();
        if (cam == null)
        {
            return;
        }

        Vector3 direction = cam.transform.position - transform.position;

        if (onlyRotateY)
        {
            direction.y = 0f;
        }

        if (direction.sqrMagnitude < 0.000001f)
        {
            return;
        }

        Quaternion look = Quaternion.LookRotation(direction.normalized, Vector3.up);
        if (flip)
        {
            look *= Quaternion.Euler(0f, 180f, 0f);
        }

        transform.rotation = look;
    }

    private Camera ResolveCamera()
    {
        if (targetCamera != null)
        {
            return targetCamera;
        }

        return Camera.main;
    }
}