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

    public float Energy = 100f;
    public float Frost = 100f;
    public float Heat = 100f;

    public GameObject EnergyBar;
    public GameObject FrostBar;
    public GameObject HeatBar;

    public float lerpSpeed = 2f;
    
    void Start()
    {
        rg = GetComponentInChildren<Rigidbody>();
        col = GetComponentInChildren<Collider>();
        origRotation = transform.rotation;
    }

    void Update()
    {
        EnergyBar.transform.localScale = new Vector3(
            EnergyBar.transform.localScale.x,
            Mathf.Lerp(EnergyBar.transform.localScale.y, Energy / 100f, lerpSpeed * Time.deltaTime),
            EnergyBar.transform.localScale.z);
        
        FrostBar.transform.localScale = new Vector3(
            FrostBar.transform.localScale.x, 
            Mathf.Lerp(FrostBar.transform.localScale.y, Frost / 100f, lerpSpeed * Time.deltaTime),
            FrostBar.transform.localScale.z);
        
        HeatBar.transform.localScale = new Vector3(
            HeatBar.transform.localScale.x, 
            Mathf.Lerp(HeatBar.transform.localScale.y, Heat / 100f, lerpSpeed * Time.deltaTime),
            HeatBar.transform.localScale.z);
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
