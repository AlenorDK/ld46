using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
#if UNITY_EDITOR 
using UnityEditor;
#endif 
using UnityEngine;

public class ContainerController : InteractableObject
{
    public bool isPickedUp = false;

    public Vector3 pickedUpScale;
    public Vector3 origScale;
    public Quaternion origRotation;

    public float Energy = 100f;
    public float Temperature = 100f;
    public float Heat = 100f;

    public float minTemperature = -50f;
    public float maxTemperature = 50f;
    
    public Gradient TemperatureGradient;
    
    public GameObject EnergyBar;
    public GameObject TemperatureBar;

    public GameObject parentLoaderObject;
    
    public float lerpSpeed = 2f;
    
    public bool isUncharging = true;
    public float UnchargingSpeed = 4f;
    public float UnsuitableTemperatureUnchargingSpeed = 6f;
    public float criticalThreshold = 30f;

    public ParticleSystem stealingBox1;
    public ParticleSystem stealingBox2;

    public TextMeshPro tempText;

    public AudioSource src;
    public AudioClip boxPut, alert;

    private bool isPlayingAlert = false;
    
    void Start()
    {
        origRotation = transform.rotation;
        origScale = transform.localScale;
    }

    void Update()
    {
        if (isUncharging)
        {
            Energy = Mathf.Abs(Temperature) > criticalThreshold ? Energy - UnsuitableTemperatureUnchargingSpeed * Time.deltaTime : Energy - UnchargingSpeed * Time.deltaTime;
            Energy = Mathf.Clamp(Energy, 0f, 100f);
        }
        
        EnergyBar.transform.localScale = new Vector3(
            EnergyBar.transform.localScale.x,
            Mathf.Lerp(EnergyBar.transform.localScale.y, Energy / 100f, lerpSpeed * Time.deltaTime),
            EnergyBar.transform.localScale.z);

        Temperature = Mathf.Clamp(Temperature, minTemperature, maxTemperature);
        TemperatureBar.GetComponent<MeshRenderer>().material.color = TemperatureGradient.Evaluate((Temperature + 50f) / 100f);
        tempText.text = ((int) Temperature).ToString();

        if (Energy > 0f && Energy < 30f && !isPlayingAlert)
        {
            isPlayingAlert = true;
            StartCoroutine(PlayAlert());
        }
    }

    IEnumerator PlayAlert()
    {
        do
        {
            src.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            src.volume = 0.3f;
            src.PlayOneShot(alert);
            yield return new WaitForSeconds(2f);
        } while (Energy > 0f && Energy < 30f);

        isPlayingAlert = false;
    }
    
    public void PickUp()
    {
        if (parentLoaderObject != null)
        {
            parentLoaderObject.GetComponent<ContainerLoaderController>().hasContainer = false;
            parentLoaderObject.GetComponent<ContainerLoaderController>().isCharging = false;
            isUncharging = true;
            parentLoaderObject = null;
        }
        gameObject.layer = 8;
        foreach (Transform child in GetComponentInChildren<Transform>())
        {
            child.gameObject.layer = 8;
        }
        isPickedUp = true;
        transform.localScale = pickedUpScale;
    }

    public void Place()
    {
        src.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        src.volume = 0.4f;
        src.PlayOneShot(boxPut);
        StartCoroutine(TurnOffCollider());
    }

    public void PlaceInCharger()
    {
        StartCoroutine(TurnOffColliderInCharger());
    }
    
    IEnumerator TurnOffCollider()
    {
        yield return new WaitForFixedUpdate();
        gameObject.layer = 0;
        foreach (Transform child in GetComponentInChildren<Transform>())
        {
            child.gameObject.layer = 0;
        }
        transform.localScale = origScale;
        isPickedUp = false;
    }
    
    IEnumerator TurnOffColliderInCharger()
    {
        parentLoaderObject = GetComponentInParent<ContainerLoaderController>().gameObject;
        yield return new WaitForFixedUpdate();
        gameObject.layer = 0;
        transform.rotation = origRotation;
        foreach (Transform child in GetComponentInChildren<Transform>())
        {
            child.gameObject.layer = 0;
        }
        isPickedUp = false;
    }

    public void Charge(float chargingSpeed)
    {
        isUncharging = false;
        Energy = Mathf.Clamp(Energy + chargingSpeed, 0f, 100f);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<TemperatureZone>() != null)
        {
            Temperature += other.GetComponent<TemperatureZone>().TemperatureMod * Time.deltaTime;
        }
        
        if (other.gameObject.tag == "MovingPlatform" && !isPickedUp)
        {
            transform.parent = other.gameObject.transform.parent;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "MovingPlatform" && !isPickedUp)
        {
            transform.parent = null;
        }
    }
}
