using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CameraMoving : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private Transform triggerObjectA;

    [Header("Camera Settings")]
    [SerializeField] private Transform targetCamera;
    [SerializeField] private Transform cameraTargetPoint;
    [SerializeField] private Vector3 cameraTargetEulerAngles;
    [SerializeField] private float smoothSpeed = 4f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isTriggered;

    private void Start()
    {
        Collider selfCollider = GetComponent<Collider>();
        if (selfCollider != null && !selfCollider.isTrigger)
        {
            Debug.LogWarning("CameraMoving requires this object's Collider to be set as Trigger.", this);
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main != null ? Camera.main.transform : null;
        }

        if (targetCamera != null)
        {
            originalPosition = targetCamera.position;
            originalRotation = targetCamera.rotation;
        }
    }

    private void Update()
    {
        if (targetCamera == null || cameraTargetPoint == null)
        {
            return;
        }

        Vector3 desiredPosition = isTriggered ? cameraTargetPoint.position : originalPosition;
        Quaternion desiredRotation = isTriggered
            ? Quaternion.Euler(cameraTargetEulerAngles)
            : originalRotation;

        float t = Mathf.Clamp01(smoothSpeed * Time.deltaTime);
        targetCamera.position = Vector3.Lerp(targetCamera.position, desiredPosition, t);
        targetCamera.rotation = Quaternion.Slerp(targetCamera.rotation, desiredRotation, t);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerObjectA != null && other.transform != triggerObjectA)
        {
            return;
        }

        isTriggered = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (triggerObjectA != null && other.transform != triggerObjectA)
        {
            return;
        }

        isTriggered = false;
    }
}
