using UnityEngine;

public class BlockMove : MonoBehaviour
{
    public Transform pointA; // ??????
    public Transform pointB; // ??????
    public float snapStep = 0.25f; // ????????
    public float smoothSpeed = 10f; // ??????

    [Header("Drag Select SFX")]
    [SerializeField] private bool playSelectSfx = true;
    [SerializeField] private AudioClip selectSfx;
    [SerializeField] private AudioSource selectAudioSource;

    private bool dragging = false;
    private Vector3 offset;
    private Vector3 targetPos;

    private void Awake()
    {
        if (selectAudioSource == null)
        {
            selectAudioSource = GetComponent<AudioSource>();
        }
    }

    void OnMouseDown()
    {
        // When selecting/starting to drag, suppress PlayerMove click SFX once.
        PlayerMove.SuppressClickSfxNextClick();

        if (playSelectSfx && selectSfx != null && selectAudioSource != null)
        {
            selectAudioSource.PlayOneShot(selectSfx);
        }

        dragging = true;
        Vector3 mousePos = Input.mousePosition;
        if (Camera.main != null)
        {
            mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
            offset = transform.position - Camera.main.ScreenToWorldPoint(mousePos);
        }
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

            // ????????????????
            Vector3 dir = pointB.position - pointA.position;
            float t = Vector3.Dot(worldPos - pointA.position, dir.normalized) / dir.magnitude;
            t = Mathf.Clamp01(t);

            // ???????????
            float snapT = Mathf.Round(t / snapStep) * snapStep;
            targetPos = pointA.position + dir * snapT;
        }

        // ??????
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