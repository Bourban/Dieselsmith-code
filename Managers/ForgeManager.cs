using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ForgeManager : MonoBehaviour
{
    // Singleton instance
    public static ForgeManager Instance;

    [SerializeField] 
    private Character PlayerCharacter;

    private EControlState CurrentControlState = EControlState.Default;
    
    //Very temp, UI manager needed.
    [SerializeField] 
    private TMP_Text tmpUseText;
    
    // Ref to WorkStation currently selected for interacting.
    private WorkStation InteractableWorkStation;
    
    void Start()
    {
        if (Instance)
        {
            Debug.LogError("Trying to instantiate a second ForgeManager.");
            Destroy(this);
        }
        Instance = this;

        tmpUseText.enabled = false;
    }

    public void SetInteractiveWorkStation(WorkStation station)
    {
        InteractableWorkStation = station;

        //TODO: Update UI appropriately.
        tmpUseText.enabled = InteractableWorkStation != null;
    }

    public void StartUsingCurrentWorkStation()
    {
        if (CurrentControlState == EControlState.Default && InteractableWorkStation != null)
        {
            CurrentControlState = EControlState.Crafting;
            InteractableWorkStation.SetStationComponentsActive(true);
            return;
        }
        
        //If we're not near a valid station, or we're already interacting with a station, go back to Overview.
        CurrentControlState = EControlState.Default;

        if (InteractableWorkStation != null)
        {
            InteractableWorkStation.SetStationComponentsActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartUsingCurrentWorkStation();
        }
    }
}

enum EControlState
{
    Default,
    Crafting,
    InSetPiece,
    Invalid
}