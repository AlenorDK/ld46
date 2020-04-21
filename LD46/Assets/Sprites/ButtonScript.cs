using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScript : InteractableObject
{
    public GameObject[] connectedObjects;
    public bool isPressed = false;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    public Transform openedPosition, closedPosition;
    public GameObject button;
    public float lerpingSpeed;

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }
    
    public override void Activate(bool forced)
    {
        foreach (var obj in connectedObjects)
        {
            obj.GetComponent<InteractableObject>().Activate(true);
        }
        isPressed = !isPressed;
    }

    void Update()
    {
        if (button != null)
        {
            targetPosition = isPressed ? openedPosition.position : closedPosition.position;
            targetRotation = isPressed ? openedPosition.rotation : closedPosition.rotation;
            button.transform.position =
                Vector3.Lerp(button.transform.position, targetPosition, lerpingSpeed * Time.deltaTime);
            button.transform.rotation =
                Quaternion.Lerp(button.transform.rotation, targetRotation, lerpingSpeed * Time.deltaTime);
        }
    }
}
