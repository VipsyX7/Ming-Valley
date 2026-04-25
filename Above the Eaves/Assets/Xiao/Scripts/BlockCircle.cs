using UnityEngine;

public class BlockCircle : MonoBehaviour
{
    public Transform rotationAxisObject; // 空物体确定旋转轴
    public Vector3 rotationAxis = Vector3.up; // 默认Y轴
    public float snapAngle = 45f; // 每次点击旋转角度
    public float smoothSpeed = 10f; // 平滑旋转速度

    private Quaternion targetRotation;

    void Start()
    {
        targetRotation = transform.rotation;
    }

    void OnMouseDown()
    {
        // 点击时增加旋转角度
        Quaternion rotationStep = Quaternion.AngleAxis(snapAngle, rotationAxis);
        targetRotation = rotationStep * targetRotation;
    }

    void Update()
    {
        // 平滑旋转到目标旋转
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
    }
}