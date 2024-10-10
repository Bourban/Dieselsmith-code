using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForgeStation : WorkStation
{
    [SerializeField] 
    private Transform SwordObject;
    
    public override void SetStationComponentsActive(bool bStationEnabled)
    {
        base.SetStationComponentsActive(bStationEnabled);
        
        
    }
}
