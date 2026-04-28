using UnityEngine;

public class BlockCircleAutoAxis : MonoBehaviour
{
    public enum AxisType
    {
        X, Y, Z
    }

    [Header("旋??置")]
    public AxisType axisType = AxisType.Y; // 勾?旋??
    public float snapAngle = 45f;
    public float smoothSpeed = 10f;

    private Vector3 pivotPoint;      // 几何中心
    private Vector3 rotationAxis;    // ??旋??

    private float currentAngle = 0f;
    private float targetAngle = 0f;

    void Start()
    {
        CalculateBoundsCenter();
        UpdateAxis();
    }

    void CalculateBoundsCenter()
    {
        Renderer[] renders = GetComponentsInChildren<Renderer>();

        if (renders.Length == 0)
        {
            pivotPoint = transform.position;
            return;
        }

        Bounds bounds = renders[0].bounds;

        foreach (Renderer r in renders)
        {
            bounds.Encapsulate(r.bounds);
        }

        pivotPoint = bounds.center;
    }

    void UpdateAxis()
    {
        switch (axisType)
        {
            case AxisType.X:
                rotationAxis = Vector3.right; // 垂直YZ平面
                break;
            case AxisType.Y:
                rotationAxis = Vector3.up;    // 垂直XZ平面
                break;
            case AxisType.Z:
                rotationAxis = Vector3.forward; // 垂直XY平面
                break;
        }
    }

    void OnMouseDown()
    {
        targetAngle += snapAngle;
    }

    void Update()
    {
        // 重新?算（防止物体?????化）
        CalculateBoundsCenter();
        UpdateAxis();

        // 平滑角度
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * smoothSpeed);

        // ??旋?差?
        float delta = currentAngle - transform.localEulerAngles.magnitude;

        // 正??中心旋?
        transform.RotateAround(pivotPoint, rotationAxis, delta);
    }
}