using UnityEngine;

public class BlockMove : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float snapStep = 0.25f;
    public float smoothSpeed = 15f;

    [Header("Drag Select SFX")]
    [SerializeField] private bool playSelectSfx = true;
    [SerializeField] private AudioClip selectSfx;
    [SerializeField] private AudioSource selectAudioSource;

    private bool dragging = false;
    private Vector3 offset;
    private Vector3 targetPos;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;

        if (selectAudioSource == null)
            selectAudioSource = GetComponent<AudioSource>();

        // ⭐ 关键：初始化时就对齐
        targetPos = GetSnappedPosition(transform.position);
        transform.position = targetPos;
    }

    void OnMouseDown()
    {
        PlayerMove.SuppressClickSfxNextClick();

        if (playSelectSfx && selectSfx != null && selectAudioSource != null)
        {
            selectAudioSource.PlayOneShot(selectSfx);
        }

        dragging = true;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = cam.WorldToScreenPoint(transform.position).z;
        offset = transform.position - cam.ScreenToWorldPoint(mousePos);

        // ⭐ 防止点击瞬间跳
        targetPos = transform.position;
    }

    void OnMouseUp()
    {
        dragging = false;

        // ⭐ 松开时再精确吸附一次
        targetPos = GetSnappedPosition(transform.position);
    }

    void Update()
    {
        if (dragging)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = cam.WorldToScreenPoint(transform.position).z;

            Vector3 worldPos = cam.ScreenToWorldPoint(mousePos) + offset;

            targetPos = GetSnappedPosition(worldPos);
        }

        // ⭐ 平滑移动（永远往 targetPos）
        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * smoothSpeed
        );
    }

    // ⭐ 核心：统一吸附函数（稳定关键）
    Vector3 GetSnappedPosition(Vector3 inputPos)
    {
        Vector3 dir = pointB.position - pointA.position;

        float length = dir.magnitude;
        if (length < 0.0001f) return pointA.position;

        Vector3 dirNormalized = dir / length;

        float t = Vector3.Dot(inputPos - pointA.position, dirNormalized) / length;
        t = Mathf.Clamp01(t);

        float snapT = Mathf.Round(t / snapStep) * snapStep;

        return pointA.position + dir * snapT;
    }
}