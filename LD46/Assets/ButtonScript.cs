using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScript : InteractableObject
{
    public GameObject connectedObject;
    public bool isPressed = false;
    private Vector3 targetPosition;
    public Transform openedPosition, closedPosition;
    public GameObject button;
    public float lerpingSpeed;

    void Start()
    {
        targetPosition = transform.position;
    }
    
    public override void Activate(bool forced)
    {
        connectedObject.GetComponent<InteractableObject>().Activate(true);
        isPressed = !isPressed;
    }
    
    void Update()
    {
        targetPosition = isPressed ? openedPosition.position : closedPosition.position;
        button.transform.position = Vector3.Lerp(button.transform.position, targetPosition, lerpingSpeed * Time.deltaTime);
    }
}
