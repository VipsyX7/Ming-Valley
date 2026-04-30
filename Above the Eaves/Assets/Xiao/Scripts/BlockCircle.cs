using UnityEngine;

public class BlockCircleAutoAxis : MonoBehaviour
{
    public enum AxisType
    {
        X, Y, Z
    }

    [Header("?????u")]
    public AxisType axisType = AxisType.Y; // ???????
    public float snapAngle = 45f;
    public float smoothSpeed = 10f;

    [Header("Rotate SFX")]
    [SerializeField] private bool playRotateSfx = true;
    [SerializeField] private AudioClip rotateSfx;
    [SerializeField] private AudioSource rotateAudioSource;
    [SerializeField] private float rotateSfxCooldown = 0.08f;
    private float lastRotateSfxTime = -Mathf.Infinity;

    private Vector3 pivotPoint;      // ?{?????S
    private Vector3 rotationAxis;    // ??????

    private float currentAngle = 0f;
    private float targetAngle = 0f;
    private float lastAngle = 0f;

    void Start()
    {
        CalculateBoundsCenter();
        UpdateAxis();

        if (rotateAudioSource == null)
        {
            rotateAudioSource = GetComponent<AudioSource>();
        }
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
                rotationAxis = Vector3.right; // ????YZ????
                break;
            case AxisType.Y:
                rotationAxis = Vector3.up;    // ????XZ????
                break;
            case AxisType.Z:
                rotationAxis = Vector3.forward; // ????XY????
                break;
        }
    }

    void OnMouseDown()
    {
        targetAngle += snapAngle;

        if (playRotateSfx && rotateSfx != null && rotateAudioSource != null)
        {
            if (Time.time - lastRotateSfxTime >= rotateSfxCooldown)
            {
                lastRotateSfxTime = Time.time;
                rotateAudioSource.PlayOneShot(rotateSfx);
            }
        }
    }

    

    void Update()
    {
        CalculateBoundsCenter();
        UpdateAxis();

        // 平滑角度
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * smoothSpeed);

        // 正确的 delta（只看我们自己记录的角度）
        float delta = currentAngle - lastAngle;

        transform.RotateAround(pivotPoint, rotationAxis, delta);

        lastAngle = currentAngle;
    }
}