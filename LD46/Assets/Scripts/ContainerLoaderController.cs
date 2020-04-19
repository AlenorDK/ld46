using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class ContainerLoaderController : MonoBehaviour
{
    public bool hasContainer = false;

    public Transform BoxPosition;

    public GameObject loaderJack;

    public Vector3 jackRotationOff, jackRotationOn, targetRotation;

    public float lerpingSpeed;
    public bool isCharging = false;

    public float ChargingSpeed;
    void Start()
    {
        targetRotation = jackRotationOff;
    }
    
    public void Place(GameObject box)
    {
        box.transform.parent = BoxPosition;
        box.transform.localPosition = Vector3.zero;
        hasContainer = true;
        StartCoroutine(StartCharging());
    }

    IEnumerator StartCharging()
    {
        yield return new WaitForSeconds(1f);
        isCharging = true;
    }
    
    void Update()
    {
        targetRotation = hasContainer ? jackRotationOn : jackRotationOff;
        loaderJack.transform.localRotation = Quaternion.Lerp(loaderJack.transform.localRotation, Quaternion.Euler(targetRotation), lerpingSpeed * Time.deltaTime);

        if (hasContainer && isCharging)
            GetComponentInChildren<ContainerController>().Charge(ChargingSpeed * Time.deltaTime);
    }
}
