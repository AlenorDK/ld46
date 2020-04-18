using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ContainerController : InteractableObject
{
    public bool isPickedUp = false;
    private Rigidbody rg;
    private Collider col;

    public Vector3 pickedUpScale;
    public Quaternion origRotation;
    
    void Start()
    {
        rg = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        origRotation = transform.rotation;
    }
    
    public void PickUp()
    {
        gameObject.layer = 8;
        isPickedUp = true;
        col.enabled = false;
        transform.localScale = pickedUpScale;
    }

    public void Place()
    {
        StartCoroutine(TurnOffCollider());
    }

    IEnumerator TurnOffCollider()
    {
        yield return new WaitForFixedUpdate();
        gameObject.layer = 0;
        transform.localScale = Vector3.one;
        isPickedUp = false;
        col.enabled = true;
        
    }
}
