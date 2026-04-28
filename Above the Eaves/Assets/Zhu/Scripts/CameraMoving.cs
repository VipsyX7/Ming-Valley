using UnityEngine;

public class CameraMoving : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private Transform triggerObjectA;
    [SerializeField] private bool showDebugLog = true;
    [SerializeField] private bool useManualCheck = true;
    [SerializeField] private float manualCheckRadius = 0.35f;

    [Header("Camera Settings")]
    [SerializeField] private Transform targetCamera;
    [SerializeField] private Transform cameraTargetPoint;
    [SerializeField] private Vector3 cameraTargetEulerAngles;
    [SerializeField] private float smoothSpeed = 4f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isTriggered;
    private Collider selfCollider3D;
    private Collider2D selfCollider2D;

    private void Start()
    {
        selfCollider3D = GetComponent<Collider>();
        selfCollider2D = GetComponent<Collider2D>();

        if (selfCollider3D == null && selfCollider2D == null)
        {
            Debug.LogWarning("CameraMoving requires a Collider or Collider2D on this object.", this);
        }

        if (selfCollider3D != null && !selfCollider3D.isTrigger)
        {
            Debug.LogWarning("CameraMoving requires this object's Collider to be set as Trigger.", this);
        }

        if (selfCollider2D != null && !selfCollider2D.isTrigger)
        {
            Debug.LogWarning("CameraMoving requires this object's Collider2D to be set as Trigger.", this);
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

        if (useManualCheck)
        {
            isTriggered = EvaluateManualTriggerState();
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
        if (!IsTargetObject(other.transform))
        {
            return;
        }

        isTriggered = true;
        Log($"3D Enter: {other.name}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsTargetObject(other.transform))
        {
            return;
        }

        isTriggered = false;
        Log($"3D Exit: {other.name}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsTargetObject(other.transform))
        {
            return;
        }

        isTriggered = true;
        Log($"2D Enter: {other.name}");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsTargetObject(other.transform))
        {
            return;
        }

        isTriggered = false;
        Log($"2D Exit: {other.name}");
    }

    private bool IsTargetObject(Transform other)
    {
        if (triggerObjectA == null)
        {
            return true;
        }

        if (other == triggerObjectA)
        {
            return true;
        }

        return other.IsChildOf(triggerObjectA);
    }

    private bool EvaluateManualTriggerState()
    {
        if (triggerObjectA == null)
        {
            return false;
        }

        Vector3 targetPos = triggerObjectA.position;
        float radiusSqr = manualCheckRadius * manualCheckRadius;

        if (selfCollider3D != null)
        {
            Vector3 closest3D = selfCollider3D.ClosestPoint(targetPos);
            return (closest3D - targetPos).sqrMagnitude <= radiusSqr;
        }

        if (selfCollider2D != null)
        {
            Vector2 closest2D = selfCollider2D.ClosestPoint(targetPos);
            Vector2 delta = (Vector2)targetPos - closest2D;
            return delta.sqrMagnitude <= radiusSqr;
        }

        // If no collider exists, fallback to distance from button object center.
        return (transform.position - targetPos).sqrMagnitude <= radiusSqr;
    }

    private void Log(string msg)
    {
        if (showDebugLog)
        {
            Debug.Log($"CameraMoving: {msg}", this);
        }
    }
}
