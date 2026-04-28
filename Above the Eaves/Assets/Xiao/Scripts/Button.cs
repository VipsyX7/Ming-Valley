using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    [Header("Tag")]
    public string playerTag = "Player";          

    [Header("³õÄ©Î»ÖÃ")]
    public Transform objectToMove;              
    public Transform targetPositionObject;      

    [Header("ËÙ¶È")]
    public float moveSpeed = 5f;            

    private bool triggered = false;         

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
