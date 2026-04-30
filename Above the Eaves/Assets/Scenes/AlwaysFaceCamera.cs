using UnityEngine;

/// <summary>
/// ?物体始?面向正交?像机（?用于2D游?、UI、血条等）
/// </summary>
public class FaceOrthographicCamera : MonoBehaviour
{
    [Header("?像机?置")]
    [Tooltip("要面向的正交?像机，不填?自?找主?像机")]
    public Camera targetCamera;

    [Header("旋?模式")]
    [Tooltip("true=只在水平面旋?（保持直立），false=完全面向?像机")]
    public bool rotateOnlyOnY = true;

    [Tooltip("是否翻?（背面朝向?像机）")]
    public bool flip = false;

    [Tooltip("是否保持原始Z?旋?不?（如2D Sprite通常不需要旋?Z）")]
    public bool preserveZRotation = true;

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError("未找到主?像机，?手?? " + gameObject.name + " 指定 targetCamera。");
            }
        }

        // ???像机是否?正交模式
        if (targetCamera != null && !targetCamera.orthographic)
        {
            Debug.LogWarning("警告：" + targetCamera.name + " 不是正交?像机，脚本可能表??常。");
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        Vector3 cameraPos = targetCamera.transform.position;
        Vector3 myPos = transform.position;

        if (rotateOnlyOnY)
        {
            // 只在Y?旋?：?算水平方向指向?像机
            Vector3 directionToCamera = cameraPos - myPos;
            directionToCamera.y = 0;  // 忽略高度差，保持直立

            if (directionToCamera != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
                if (flip) targetRotation *= Quaternion.Euler(0, 180, 0);

                if (preserveZRotation)
                {
                    // 保留原始Z旋?（常用于2D Sprite）
                    targetRotation = Quaternion.Euler(targetRotation.eulerAngles.x, targetRotation.eulerAngles.y, transform.eulerAngles.z);
                }
                transform.rotation = targetRotation;
            }
        }
        else
        {
            // 完全面向?像机（包括俯仰）
            Vector3 direction = cameraPos - myPos;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                if (flip) targetRotation *= Quaternion.Euler(0, 180, 0);
                transform.rotation = targetRotation;
            }
        }
    }

    // 可?：在??器模式下也??更新（方便??）
    private void OnDrawGizmosSelected()
    {
        if (targetCamera != null && Application.isPlaying == false)
        {
            LateUpdate();
        }
    }
}