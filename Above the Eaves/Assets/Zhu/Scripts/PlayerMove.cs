using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMove : MonoBehaviour
{
    [Header("Checkpoint Detect")]
    [SerializeField] private LayerMask checkpointLayer;
    [SerializeField] private float clickDetectRadius = 1.2f;
    [SerializeField] private float checkpointNeighborRadius = 2.5f;
    [SerializeField] private bool use2DPhysics = true;
    [SerializeField] private bool showDebugLog = true;

    [Header("Click SFX")]
    [SerializeField] private bool playClickSfx = true;
    [SerializeField] private AudioClip clickSfx;
    [SerializeField] private AudioSource clickAudioSource;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float arriveDistance = 0.02f;

    private Camera cachedCamera;
    private Coroutine moveCoroutine;

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

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        PlayClickSfx();
        HandleMouseClick();
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
