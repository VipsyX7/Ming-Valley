using UnityEngine;

public class BlockMove : MonoBehaviour
{
    //轨道起点
    public Transform startPoint;

    //轨道终点
    public Transform endPoint;

    //当前位置
    [Range(0f, 1f)]
    public float t = 0f;

    //滑动速度
    public float moveSpeed = 2f;

    //吸附点
    public float[] snapPoints;

    public float snapThreshold = 0.05f; //触发吸附
    public float snapSpeed = 10f;

    private bool isDragging = false;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        UpdatePosition();
    }

    void Update()
    {
        MouseCheck();

        if (isDragging)
        {
            Drag();
        }

        Snap();
        UpdatePosition();
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

   
        t += mouseX * moveSpeed * Time.deltaTime;

    
        t = Mathf.Clamp01(t);
    }


    void UpdatePosition()
    {
        transform.position = Vector3.Lerp(
            startPoint.position,
            endPoint.position,
            t
        );
    }


    void Snap()
    {
        if (snapPoints == null || snapPoints.Length == 0) return;

        foreach (float point in snapPoints)
        {
            if (Mathf.Abs(t - point) < snapThreshold)
            {
                t = Mathf.Lerp(t, point, snapSpeed * Time.deltaTime);
            }
        }
    }
}