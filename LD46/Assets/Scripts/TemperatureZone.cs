using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

public class TemperatureZone : MonoBehaviour
{
    public float TemperatureMod;

    private void OnDrawGizmos()
    {
        if (TemperatureMod > 0)
            Gizmos.color = Color.red;
        
        else if (TemperatureMod < 0)
            Gizmos.color = Color.blue;
        
        else Gizmos.color = Color.gray;
        
        Gizmos.DrawWireCube(GetComponent<BoxCollider>().center, GetComponent<BoxCollider>().size);
    }

   
}
