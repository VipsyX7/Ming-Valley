using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCircle : MonoBehaviour
{
    //起始旋转
    public Transform rotateStart;

    //目标旋转
    public Transform rotateEnd;

    //旋转进度
    [Range(0f, 1f)]
    public float t = 0f;

    //旋转速度
    public float rotateSpeed = 2f;

    // 吸附点
    public float[] snapPoints;

    public float snapThreshold = 0.05f;
    public float snapSpeed = 10f;

    private bool isDragging = false;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        UpdateRotation();
    }

    void Update()
    {
        MouseCheck();

        if (isDragging)
        {
            Drag();
        }

        Snap();
        UpdateRotation();
    }

    void MouseCheck()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform)
                {
                    isDragging = true;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    void Drag()
    {
        float mouseX = Input.GetAxis("Mouse X");

        t += mouseX * rotateSpeed * Time.deltaTime;
        t = Mathf.Clamp01(t);
    }

    void UpdateRotation()
    {
        transform.rotation = Quaternion.Lerp(
            rotateStart.rotation,
            rotateEnd.rotation,
            t
        );
    }

    void Snap()
    {
        if (snapPoints == null) return;

        foreach (float point in snapPoints)
        {
            if (Mathf.Abs(t - point) < snapThreshold)
            {
                t = Mathf.Lerp(t, point, snapSpeed * Time.deltaTime);
            }
        }
    }
}
