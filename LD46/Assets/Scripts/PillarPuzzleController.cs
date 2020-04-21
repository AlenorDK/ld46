using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PillarPuzzleController : MonoBehaviour
{
    public GameObject pillarA, pillarB, pillarC, pillarD;
    public int pillarAOrigPos, pillarBOrigPos, pillarCOrigPos, pillarDOrigPos;
    public int pillarAPos, pillarBPos, pillarCPos, pillarDPos;
    public float pillarOrigY;
    public float offset = 4f;
    public bool canActivateB = true;
    public float lerpingSpeed = 5f;
    
    
    void Start()
    {
        pillarOrigY = pillarA.transform.position.y;
        originalSetup();
    }

    public void originalSetup()
    {
        pillarA.transform.position = new Vector3(
            pillarA.transform.position.x,
            pillarOrigY + pillarAOrigPos * offset,
            pillarA.transform.position.z);

        pillarB.transform.position = new Vector3(
            pillarB.transform.position.x,
            pillarOrigY + pillarBOrigPos * offset,
            pillarB.transform.position.z);

        pillarC.transform.position = new Vector3(
            pillarC.transform.position.x,
            pillarOrigY + pillarCOrigPos * offset,
            pillarC.transform.position.z);

        pillarD.transform.position = new Vector3(
            pillarD.transform.position.x,
            pillarOrigY + pillarDOrigPos * offset,
            pillarD.transform.position.z);
    }

    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Alpha5))
            Reset();
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
            ActivateA();
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
            ActivateB();

        if (Input.GetKeyDown(KeyCode.Alpha3))
            ActivateC();
        ;
        if (Input.GetKeyDown(KeyCode.Alpha4))
            ActivateD();*/

        pillarA.transform.position = Vector3.Lerp(pillarA.transform.position,
            new Vector3(pillarA.transform.position.x,
                pillarOrigY + pillarAPos * offset,
                pillarA.transform.position.z), lerpingSpeed * Time.deltaTime);
        
        pillarB.transform.position = Vector3.Lerp(pillarB.transform.position,
            new Vector3(pillarB.transform.position.x,
                pillarOrigY + pillarBPos * offset,
                pillarB.transform.position.z), lerpingSpeed * Time.deltaTime);
        
        pillarC.transform.position = Vector3.Lerp(pillarC.transform.position,
            new Vector3(pillarC.transform.position.x,
                pillarOrigY + pillarCPos * offset,
                pillarC.transform.position.z), lerpingSpeed * Time.deltaTime);
        
        pillarD.transform.position = Vector3.Lerp(pillarD.transform.position,
            new Vector3(pillarD.transform.position.x,
                pillarOrigY + pillarDPos * offset,
                pillarD.transform.position.z), lerpingSpeed * Time.deltaTime);

    }

    public void ActivateA()
    {
        pillarAPos = Mathf.Clamp(pillarCPos - 2, 0, 6);
        
        pillarBPos = Mathf.Clamp(pillarCPos - 1, 0, 6);
        
        pillarDPos = Mathf.Clamp(pillarCPos + 2, 0, 6);

        canActivateB = true;
    }

    public void ActivateB()
    {
        if (canActivateB)
        {
            pillarCPos = Mathf.Clamp(pillarCPos + 2, 0, 6);
            
            pillarDPos = Mathf.Clamp(pillarDPos - 2, 0, 6);

            canActivateB = false;
        }
    }

    public void ActivateC()
    {
        pillarCPos = Mathf.Clamp(pillarCPos - 1, 0, 6);
    }

    public void ActivateD()
    {
        pillarAPos = Mathf.Clamp(pillarAPos + 1, 0, 6);
    }

    public void Reset()
    {
        pillarAPos = pillarAOrigPos;
        pillarBPos = pillarBOrigPos;
        pillarCPos = pillarCOrigPos;
        pillarDPos = pillarDOrigPos;
    }
}
