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

    public GameObject door;

    public float lerpingSpeed;
    void Start()
    {
        if (isOpened)
            door.transform.position = openedPosition.position;
        else
            door.transform.position = closedPosition.position;
        
        targetPosition = isOpened ? openedPosition.position : closedPosition.position;
    }

    public override void Activate(bool forced)
    {
        if ((CanBeActivated || forced) && !(oneWay && isOpened))
            isOpened = !isOpened;

        targetPosition = isOpened ? openedPosition.position : closedPosition.position;
    }

    void Update()
    {
        targetPosition = isOpened ? openedPosition.position : closedPosition.position;
        door.transform.position = Vector3.Lerp(door.transform.position, targetPosition, lerpingSpeed * Time.deltaTime);
    }
}
