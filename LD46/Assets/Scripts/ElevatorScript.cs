using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorScript : InteractableObject
{
    public bool CanBeActivated = false;
    public bool isDown = false;
    public bool oneWay = true;

    public Transform downPosition;
    public Transform upPosition;
    public Vector3 targetPosition;
    public Quaternion targetRotation;

    public GameObject elevator;

    public float lerpingSpeed;
    void Start()
    {
        if (isDown)
        {
            elevator.transform.position = downPosition.position;
            elevator.transform.rotation = downPosition.rotation;
        }
        else
        {
            elevator.transform.position = upPosition.position;
            elevator.transform.rotation = upPosition.rotation;
        }

        targetPosition = isDown ? downPosition.position : upPosition.position;
        targetRotation = isDown ? downPosition.rotation : upPosition.rotation;
    }

    public override void Activate(bool forced)
    {
        if ((CanBeActivated || forced) && !(oneWay && isDown))
            isDown = !isDown;

        targetPosition = isDown ? downPosition.position : upPosition.position;
        targetRotation = isDown ? downPosition.rotation : upPosition.rotation;
    }

    void FixedUpdate()
    {
        targetPosition = isDown ? downPosition.position : upPosition.position;
        targetRotation = isDown ? downPosition.rotation : upPosition.rotation;

        elevator.transform.position = Vector3.Lerp(elevator.transform.position, targetPosition, lerpingSpeed * Time.deltaTime);
        elevator.transform.rotation = Quaternion.Lerp(elevator.transform.rotation, targetRotation, lerpingSpeed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(downPosition.position, upPosition.position);
    }
}
