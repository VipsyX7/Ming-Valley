using UnityEngine;

public class BlockMove : MonoBehaviour
{
    public Transform pointA; // 轨道起点
    public Transform pointB; // 轨道终点
    public float snapStep = 0.25f; // 吸附步长
    public float smoothSpeed = 10f; // 平滑速度

    private bool dragging = false;
    private Vector3 offset;
    private Vector3 targetPos;

    void OnMouseDown()
    {
        dragging = true;
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        offset = transform.position - Camera.main.ScreenToWorldPoint(mousePos);
    }

    void OnMouseUp()
    {
        dragging = false;
        CalculateTargetPosition();
    }

    void Update()
    {
        if (dragging)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos) + offset;

            // 计算沿轨道的投影位置
            Vector3 dir = pointB.position - pointA.position;
            float t = Vector3.Dot(worldPos - pointA.position, dir.normalized) / dir.magnitude;
            t = Mathf.Clamp01(t);

            // 吸附目标位置
            float snapT = Mathf.Round(t / snapStep) * snapStep;
            targetPos = pointA.position + dir * snapT;
        }

        // 平滑移动
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
    }

    void CalculateTargetPosition()
    {
        Vector3 dir = pointB.position - pointA.position;
        float t = Vector3.Dot(transform.position - pointA.position, dir.normalized) / dir.magnitude;
        t = Mathf.Clamp01(t);
        float snapT = Mathf.Round(t / snapStep) * snapStep;
        targetPos = pointA.position + dir * snapT;
    }
}