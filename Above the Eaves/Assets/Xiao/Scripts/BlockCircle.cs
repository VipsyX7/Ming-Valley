using UnityEngine;

public class BlockCircle : MonoBehaviour
{
    public Transform rotationAxisObject; // 空物体确定旋转轴
    public Vector3 rotationAxis = Vector3.up; // 默认Y轴
    public float snapAngle = 45f; // 吸附角度
    public float smoothSpeed = 10f; // 平滑旋转速度

    private bool dragging = false;
    private Vector3 lastMousePos;
    private Quaternion targetRotation;

    void Start()
    {
        targetRotation = transform.rotation;
    }

    void OnMouseDown()
    {
        dragging = true;
        lastMousePos = Input.mousePosition;
    }

    void OnMouseUp()
    {
        dragging = false;
        SnapRotation();
    }

    void Update()
    {
        if (dragging)
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePos;
            float rotateAmount = mouseDelta.x; // 鼠标左右控制旋转
            transform.RotateAround(rotationAxisObject.position, rotationAxis, rotateAmount * 0.5f);
            lastMousePos = Input.mousePosition;
            targetRotation = transform.rotation; // 拖拽中持续更新目标旋转
        }

        // 平滑旋转
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
    }

    void SnapRotation()
    {
        Vector3 angles = transform.eulerAngles;
        angles.x = Mathf.Round(angles.x / snapAngle) * snapAngle;
        angles.y = Mathf.Round(angles.y / snapAngle) * snapAngle;
        angles.z = Mathf.Round(angles.z / snapAngle) * snapAngle;
        targetRotation = Quaternion.Euler(angles);
    }
}