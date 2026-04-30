using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Makes this object always face a camera in a camera hierarchy.
/// </summary>
[ExecuteAlways]
public class AlwaysFaceCamera : MonoBehaviour
{
    private enum LocalFacingAxis
    {
        ForwardZ,
        BackwardZ,
        RightX,
        LeftX,
        UpY,
        DownY
    }

    [Header("Target")]
    [Tooltip("Root camera. This camera and all of its child cameras are considered.")]
    [SerializeField] private Camera targetCamera;

    [Header("Options")]
    [Tooltip("If enabled, only rotates around Y axis when aligning to screen center.")]
    [SerializeField] private bool onlyRotateY = false;

    [Tooltip("Rotate an extra 180 degrees to flip forward direction.")]
    [SerializeField] private bool flip = false;

    [Tooltip("Which local axis of this object should face the camera.")]
    [SerializeField] private LocalFacingAxis localFacingAxis = LocalFacingAxis.BackwardZ;

    [Tooltip("When multiple cameras are available, use the nearest one.")]
    [SerializeField] private bool useNearestCamera = true;

    [Tooltip("Prefer the camera currently rendering this object (Camera.current).")]
    [SerializeField] private bool preferRenderingCamera = true;

    private readonly List<Camera> cameraCache = new List<Camera>(8);

    private void LateUpdate()
    {
        Camera cam = ResolveCamera();
        if (cam == null)
        {
            return;
        }

        // Screen-facing billboard: use inverse camera forward,
        // so the object front is always visible on screen.
        Vector3 direction = -cam.transform.forward;

        if (onlyRotateY)
        {
            direction = Vector3.ProjectOnPlane(direction, Vector3.up);
        }

        if (direction.sqrMagnitude < 0.000001f)
        {
            return;
        }

        Vector3 forwardAxis = GetLocalFacingVector(localFacingAxis);
        Quaternion look = Quaternion.FromToRotation(forwardAxis, direction.normalized);

        if (flip)
        {
            look *= Quaternion.Euler(0f, 180f, 0f);
        }

        transform.rotation = look;
    }

    private Camera ResolveCamera()
    {
        Camera root = targetCamera != null ? targetCamera : Camera.main;
        if (root == null)
        {
            return null;
        }

        cameraCache.Clear();

        // Include root camera itself.
        if (root.isActiveAndEnabled && root.gameObject.activeInHierarchy)
        {
            cameraCache.Add(root);
        }

        // Include all child cameras under the root camera hierarchy.
        Camera[] childCameras = root.GetComponentsInChildren<Camera>(true);
        for (int i = 0; i < childCameras.Length; i++)
        {
            Camera child = childCameras[i];
            if (child == null || child == root)
            {
                continue;
            }

            if (!child.isActiveAndEnabled || !child.gameObject.activeInHierarchy)
            {
                continue;
            }

            cameraCache.Add(child);
        }

        if (cameraCache.Count == 0)
        {
            return null;
        }

        if (preferRenderingCamera)
        {
            Camera renderingCamera = Camera.current;
            if (renderingCamera != null && IsCameraInTargetHierarchy(renderingCamera, root))
            {
                if (renderingCamera.isActiveAndEnabled && renderingCamera.gameObject.activeInHierarchy)
                {
                    return renderingCamera;
                }
            }
        }

        if (!useNearestCamera)
        {
            return cameraCache[0];
        }

        Camera best = cameraCache[0];
        float bestSqrDistance = (best.transform.position - transform.position).sqrMagnitude;

        for (int i = 1; i < cameraCache.Count; i++)
        {
            Camera candidate = cameraCache[i];
            float sqrDistance = (candidate.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance < bestSqrDistance)
            {
                best = candidate;
                bestSqrDistance = sqrDistance;
            }
        }

        return best;
    }

    private static bool IsCameraInTargetHierarchy(Camera cam, Camera root)
    {
        if (cam == null || root == null)
        {
            return false;
        }

        Transform current = cam.transform;
        Transform rootTransform = root.transform;
        while (current != null)
        {
            if (current == rootTransform)
            {
                return true;
            }
            current = current.parent;
        }

        return false;
    }

    private static Vector3 GetLocalFacingVector(LocalFacingAxis axis)
    {
        switch (axis)
        {
            case LocalFacingAxis.ForwardZ:
                return Vector3.forward;
            case LocalFacingAxis.BackwardZ:
                return Vector3.back;
            case LocalFacingAxis.RightX:
                return Vector3.right;
            case LocalFacingAxis.LeftX:
                return Vector3.left;
            case LocalFacingAxis.UpY:
                return Vector3.up;
            case LocalFacingAxis.DownY:
                return Vector3.down;
            default:
                return Vector3.forward;
        }
    }
}