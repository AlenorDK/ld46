using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarPuzzleButton : InteractableObject
{
    public string methodName;
    
    public override void Activate(bool forced)
    {
        GetComponentInParent<PillarPuzzleController>().Invoke(methodName, 0f);
    }
}
