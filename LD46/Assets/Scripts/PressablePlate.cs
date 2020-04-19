using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor.Timeline;
using UnityEngine;

public class PressablePlate : InteractableObject
{
    public bool isActivated = false;
    public bool hasContainer = false;
    public GameObject plate;
    public Transform BoxPosition;
    public Transform UpPosition, DownPosition;
    private Vector3 targetPosition;
    public float lerpingSpeed;

    public GameObject[] _gameObjects;

    public override void Activate(bool forced)
    {
        if (hasContainer)
        {
            ContainerController controller = GetComponentInChildren<ContainerController>();
            controller.PickUp();
            hasContainer = false;
            isActivated = false;
        }
    }

    void Update()
    {
        targetPosition = isActivated ? DownPosition.position : UpPosition.position;
        plate.transform.position = Vector3.Lerp(plate.transform.position, targetPosition, lerpingSpeed * Time.deltaTime);

        foreach (var obj in _gameObjects)
        {
            obj.GetComponent<InteractableObject>().SetState(isActivated);
        }
    }
    
    public void Place(GameObject box)
    {
        box.transform.parent = BoxPosition;
        box.transform.localPosition = Vector3.zero;
        box.transform.localRotation = box.GetComponent<ContainerController>().origRotation;
        hasContainer = true;
        isActivated = true;
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log(other.collider.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>())
            isActivated = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>() && !hasContainer)
            isActivated = false;
    }
}
