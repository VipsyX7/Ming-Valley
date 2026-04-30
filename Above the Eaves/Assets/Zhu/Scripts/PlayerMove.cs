using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMove : MonoBehaviour
{
    private static bool sfxSuppressNextClick = false;

    // BlockMove 选中/开始拖拽时会调用，用于屏蔽同一帧的点击音效。
    public static void SuppressClickSfxNextClick()
    {
        sfxSuppressNextClick = true;
    }

    [Header("Checkpoint Detect")]
    [SerializeField] private LayerMask checkpointLayer;
    [SerializeField] private float clickDetectRadius = 1.2f;
    [SerializeField] private float checkpointNeighborRadius = 2.5f;
    [SerializeField] private bool use2DPhysics = true;
    [SerializeField] private bool showDebugLog = true;

    [Header("Snap to Checkpoint")]
    [SerializeField] private bool enableSnap = true;              // 是否启用吸附
    [SerializeField] private float snapDistance = 0.1f;          // 吸附距离阈值
    [SerializeField] private bool snapOnStart = true;            // 开始时吸附到最近检查点
    [SerializeField] private bool snapOnArrive = true;           // 到达检查点时吸附
    [SerializeField] private bool followMovingCheckpoint = true; // 跟随移动的检查点

    [Header("Click SFX")]
    [SerializeField] private bool playClickSfx = true;
    [SerializeField] private AudioClip clickSfx;
    [SerializeField] private AudioSource clickAudioSource;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float arriveDistance = 0.02f;

    private Camera cachedCamera;
    private Coroutine moveCoroutine;

    // 记录当前吸附的检查点
    private Transform currentSnappedCheckpoint;
    // 记录检查点上一帧的位置（用于跟随移动）
    private Vector3 lastCheckpointPosition;

    private void Awake()
    {
        cachedCamera = Camera.main;

        if (clickAudioSource == null)
        {
            clickAudioSource = GetComponent<AudioSource>();
        }

        if (clickAudioSource == null && clickSfx != null)
        {
            Debug.LogWarning("PlayerMove: 已配置 clickSfx，但当前物体没有 AudioSource。", this);
        }
    }

    private void Start()
    {
        // 游戏开始时吸附到最近的检查点
        if (enableSnap && snapOnStart)
        {
            SnapToNearestCheckpoint();
        }
    }

    private void Update()
    {
        // 如果启用了跟随移动的检查点，且当前吸附在某个检查点上
        if (followMovingCheckpoint && currentSnappedCheckpoint != null)
        {
            // 检测检查点是否移动了
            if (currentSnappedCheckpoint.position != lastCheckpointPosition)
            {
                // 跟随检查点移动
                transform.position = currentSnappedCheckpoint.position;
                lastCheckpointPosition = currentSnappedCheckpoint.position;
                Log($"跟随检查点 {currentSnappedCheckpoint.name} 移动到: {transform.position}");
            }
            else
            {
                // 更新记录的位置
                lastCheckpointPosition = currentSnappedCheckpoint.position;
            }
        }

        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        HandleMouseClick();
    }

    /// <summary>
    /// 将人物吸附到最近的检查点
    /// </summary>
    /// <returns>是否成功吸附</returns>
    public bool SnapToNearestCheckpoint()
    {
        Transform nearestCheckpoint = FindNearestCheckpoint(transform.position, snapDistance, null);

        if (nearestCheckpoint != null)
        {
            transform.position = nearestCheckpoint.position;
            SetSnappedCheckpoint(nearestCheckpoint);
            Log($"吸附到检查点: {nearestCheckpoint.name}");
            return true;
        }

        // 如果阈值内没有检查点，尝试用更大的搜索半径
        Transform distantCheckpoint = FindNearestCheckpoint(transform.position, checkpointNeighborRadius, null);
        if (distantCheckpoint != null)
        {
            transform.position = distantCheckpoint.position;
            SetSnappedCheckpoint(distantCheckpoint);
            Log($"吸附到检查点（扩展搜索）: {distantCheckpoint.name}");
            return true;
        }

        Log("没有找到可吸附的检查点");
        return false;
    }

    /// <summary>
    /// 将人物吸附到指定的检查点
    /// </summary>
    public void SnapToCheckpoint(Transform checkpoint)
    {
        if (checkpoint == null) return;

        transform.position = checkpoint.position;
        SetSnappedCheckpoint(checkpoint);
        Log($"吸附到指定检查点: {checkpoint.name}");
    }

    /// <summary>
    /// 设置当前吸附的检查点
    /// </summary>
    private void SetSnappedCheckpoint(Transform checkpoint)
    {
        currentSnappedCheckpoint = checkpoint;
        if (checkpoint != null)
        {
            lastCheckpointPosition = checkpoint.position;
        }
    }

    /// <summary>
    /// 清除当前吸附的检查点
    /// </summary>
    public void ClearSnappedCheckpoint()
    {
        currentSnappedCheckpoint = null;
        Log("清除检查点吸附");
    }

    /// <summary>
    /// 获取当前吸附的检查点
    /// </summary>
    public Transform GetCurrentSnappedCheckpoint()
    {
        return currentSnappedCheckpoint;
    }

    private bool ShouldPlayClickSfx()
    {
        if (sfxSuppressNextClick)
        {
            sfxSuppressNextClick = false;
            return false;
        }

        // 若点击的是可拖拽物体（BlockMove），则不播放本脚本的点击音效。
        if (IsClickOnBlockMoveObject())
        {
            return false;
        }

        return true;
    }

    private bool IsClickOnBlockMoveObject()
    {
        Camera cam = cachedCamera != null ? cachedCamera : Camera.main;
        if (cam == null)
        {
            return false;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        const float maxDistance = 1000f;

        // BlockMove 使用 OnMouseDown，因此通常是 3D Collider；这里优先做 3D 检测。
        if (Physics.Raycast(ray, out RaycastHit hit3D, maxDistance))
        {
            if (hit3D.collider != null && hit3D.collider.GetComponentInParent<BlockMove>() != null)
            {
                return true;
            }
        }

        // 兜底：如果场景里是 2D Collider，也尝试一次。
        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray, maxDistance);
        if (hit2D.collider != null && hit2D.collider.GetComponentInParent<BlockMove>() != null)
        {
            return true;
        }

        return false;
    }

    private void PlayClickSfx()
    {
        if (!playClickSfx || clickSfx == null)
        {
            return;
        }

        if (clickAudioSource == null)
        {
            // 兜底：如果没有手动指定 AudioSource，就尝试从挂载物体获取一次。
            clickAudioSource = GetComponent<AudioSource>();
            if (clickAudioSource == null)
            {
                return;
            }
        }

        clickAudioSource.PlayOneShot(clickSfx);
    }

    private void HandleMouseClick()
    {
        if (cachedCamera == null)
        {
            cachedCamera = Camera.main;
        }

        if (cachedCamera == null)
        {
            Debug.LogWarning("PlayerMove: 未找到 Main Camera。");
            return;
        }

        Vector3 clickWorldPos = GetMouseWorldPosition(cachedCamera);

        Transform targetCheckpointB = FindNearestCheckpoint(clickWorldPos, clickDetectRadius, null);
        if (targetCheckpointB == null)
        {
            Log($"点击位置附近没有检查点，点击世界坐标: {clickWorldPos}");
            return;
        }

        Transform currentCheckpointA = FindNearestCheckpoint(transform.position, checkpointNeighborRadius, null);
        if (currentCheckpointA == null)
        {
            Log("玩家附近没有起始检查点A，无法开始移动。");
            return;
        }

        Log($"找到目标检查点B: {targetCheckpointB.name}，起始检查点A: {currentCheckpointA.name}");

        // 只有当点击能够命中"可移动目标"（检查点B）并能找到起始检查点A时，才播放点击音效。
        if (ShouldPlayClickSfx())
        {
            PlayClickSfx();
        }

        StartMove(currentCheckpointA, targetCheckpointB);
    }

    private void StartMove(Transform startCheckpoint, Transform targetCheckpoint)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = StartCoroutine(MoveThroughCheckpointsRoutine(startCheckpoint, targetCheckpoint));
    }

    private IEnumerator MoveThroughCheckpointsRoutine(Transform startCheckpoint, Transform targetCheckpoint)
    {
        Transform current = startCheckpoint;
        Transform previous = null;
        HashSet<Transform> visited = new HashSet<Transform>();

        int maxSteps = 256;
        int step = 0;

        while (current != null && step < maxSteps)
        {
            yield return MoveToPosition(current.position);

            // 到达检查点后，吸附到该检查点
            if (enableSnap && snapOnArrive)
            {
                SnapToCheckpoint(current);
            }

            visited.Add(current);
            Log($"Step {step}: 移动到 {current.name}");

            if (current == targetCheckpoint)
            {
                Log("已到达目标检查点B。");
                moveCoroutine = null;
                yield break;
            }

            Transform next = FindBestNeighbor(
                current.position,
                checkpointNeighborRadius,
                previous,
                current,
                targetCheckpoint,
                visited);

            if (next == null)
            {
                Log("当前检查点附近没有可前进的下一个检查点，移动结束。");
                moveCoroutine = null;
                yield break;
            }

            previous = current;
            current = next;
            step++;
        }

        if (step >= maxSteps)
        {
            Debug.LogWarning("PlayerMove: 超过最大步数，已停止以避免死循环。");
        }

        moveCoroutine = null;
    }

    private IEnumerator MoveToPosition(Vector3 targetPos)
    {
        if (moveSpeed <= 0f)
        {
            transform.position = targetPos;
            yield break;
        }

        float stopDistSqr = arriveDistance * arriveDistance;
        while ((transform.position - targetPos).sqrMagnitude > stopDistSqr)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
    }

    private Transform FindBestNeighbor(
        Vector3 center,
        float radius,
        Transform excludePrevious,
        Transform excludeCurrent,
        Transform targetCheckpoint,
        HashSet<Transform> visited)
    {
        Transform[] checkpoints = GetCheckpointsInRadius(center, radius);

        Transform best = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < checkpoints.Length; i++)
        {
            Transform candidate = checkpoints[i];
            if (candidate == null ||
                candidate == excludePrevious ||
                candidate == excludeCurrent ||
                visited.Contains(candidate))
            {
                continue;
            }

            float score = (candidate.position - targetCheckpoint.position).sqrMagnitude;
            if (score < bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }

        return best;
    }

    private Transform FindNearestCheckpoint(Vector3 center, float radius, Transform exclude)
    {
        Transform[] checkpoints = GetCheckpointsInRadius(center, radius);

        Transform nearest = null;
        float minDistSqr = float.MaxValue;

        for (int i = 0; i < checkpoints.Length; i++)
        {
            Transform t = checkpoints[i];
            if (t == null || t == exclude)
            {
                continue;
            }

            float distSqr = (t.position - center).sqrMagnitude;
            if (distSqr < minDistSqr)
            {
                minDistSqr = distSqr;
                nearest = t;
            }
        }

        return nearest;
    }

    private Vector3 GetMouseWorldPosition(Camera cam)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (use2DPhysics)
        {
            Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, transform.position.z));
            if (plane.Raycast(ray, out float enter2D))
            {
                return ray.GetPoint(enter2D);
            }
        }
        else
        {
            Plane plane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
            if (plane.Raycast(ray, out float enter3D))
            {
                return ray.GetPoint(enter3D);
            }
        }

        return transform.position;
    }

    private Transform[] GetCheckpointsInRadius(Vector3 center, float radius)
    {
        if (use2DPhysics)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, checkpointLayer);
            Transform[] result = new Transform[hits.Length];
            for (int i = 0; i < hits.Length; i++)
            {
                result[i] = hits[i].transform;
            }
            return result;
        }
        else
        {
            Collider[] hits = Physics.OverlapSphere(center, radius, checkpointLayer);
            Transform[] result = new Transform[hits.Length];
            for (int i = 0; i < hits.Length; i++)
            {
                result[i] = hits[i].transform;
            }
            return result;
        }
    }

    private void Log(string msg)
    {
        if (showDebugLog)
        {
            Debug.Log($"PlayerMove: {msg}");
        }
    }
}