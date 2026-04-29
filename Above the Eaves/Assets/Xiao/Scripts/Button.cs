
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    [Header("���Tag")]
    public string playerTag = "Player";          // ���Tag

    [Header("�ƶ������Ŀ��")]
    public Transform objectToMove;               // Ҫ�ƶ�������
    public Transform targetPositionObject;       // �յ�����

    [Header("�ƶ�����")]
    public float moveSpeed = 5f;                 // �ƶ��ٶ�

    private bool triggered = false;              // �Ƿ񴥷�

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
