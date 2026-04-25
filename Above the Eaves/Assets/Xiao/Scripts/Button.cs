using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    [Header("玩家Tag")]
    public string playerTag = "Player";          // 玩家Tag

    [Header("移动物体和目标")]
    public Transform objectToMove;               // 要移动的物体
    public Transform targetPositionObject;       // 终点物体

    [Header("移动设置")]
    public float moveSpeed = 5f;                 // 移动速度

    private bool triggered = false;              // 是否触发

    void Update()
    {
        if (triggered && objectToMove != null && targetPositionObject != null)
        {
           
            objectToMove.position = Vector3.MoveTowards(objectToMove.position, targetPositionObject.position, moveSpeed * Time.deltaTime);

          
            if (Vector3.Distance(objectToMove.position, targetPositionObject.position) < 0.01f)
            {
                triggered = false;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag(playerTag))
        {
            triggered = true;
        }
    }
}
