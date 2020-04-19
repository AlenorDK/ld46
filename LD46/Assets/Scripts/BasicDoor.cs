using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicDoor : InteractableObject
{
    public bool CanBeActivated = false;
    public bool isOpened = false;
    public bool oneWay = false;

    public Transform openedPosition;
    public Transform closedPosition;
    public Vector3 targetPosition;
    public Quaternion targetRotation;

    public GameObject door;

    public float lerpingSpeed;
    void Start()
    {
        if (isOpened)
        {
            door.transform.position = openedPosition.position;
            door.transform.rotation = openedPosition.rotation;
        }
        else
        {
            door.transform.position = closedPosition.position;
            door.transform.rotation = closedPosition.rotation;
        }

        targetPosition = isOpened ? openedPosition.position : closedPosition.position;
        targetRotation = isOpened ? openedPosition.rotation : closedPosition.rotation;
    }

    public override void Activate(bool forced)
    {
        if ((CanBeActivated || forced) && !(oneWay && isOpened))
            isOpened = !isOpened;

        targetPosition = isOpened ? openedPosition.position : closedPosition.position;
        targetRotation = isOpened ? openedPosition.rotation : closedPosition.rotation;
    }

    public override void SetState(bool state)
    {
        isOpened = state;
    }

    void Update()
    {
        targetPosition = isOpened ? openedPosition.position : closedPosition.position;
        targetRotation = isOpened ? openedPosition.rotation : closedPosition.rotation;
        door.transform.position = Vector3.Lerp(door.transform.position, targetPosition, lerpingSpeed * Time.deltaTime);
        door.transform.rotation = Quaternion.Lerp(door.transform.rotation, targetRotation, lerpingSpeed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(openedPosition.position, closedPosition.position);
    }
}
