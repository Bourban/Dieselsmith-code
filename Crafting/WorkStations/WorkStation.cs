using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public abstract class WorkStation : MonoBehaviour
{
    [SerializeField] 
    public Transform CameraTransform;

    protected BoxCollider InteractBox;

    //TODO: Probably an enum for WorkType? 

    protected float WorkProgress;
    protected float MaxWork = 100.0f;

    protected bool bAutoCompleteOnMaxProgress = true;
    
    protected void Awake()
    {
        InteractBox = gameObject.GetComponent<BoxCollider>();
        
        SetStationComponentsActive(false);
    }

    protected virtual void StartWorkUnit()
    {
        WorkProgress = 0.0f;
    }

    protected virtual void ProgressWork(float progress)
    {
        WorkProgress += progress;

        if (WorkProgress >= MaxWork && bAutoCompleteOnMaxProgress == true)
        {
            FinishWorkUnit();
        }
    }

    protected virtual void FinishWorkUnit()
    {
        WorkProgress = 0.0f;
    }

    public virtual void SetStationComponentsActive(bool bStationEnabled)
    {
        //Trigger box's active state should be the opposite of the active components.
        InteractBox.enabled = !bStationEnabled;
        CameraTransform.gameObject.SetActive(bStationEnabled);
        
        // TODO: List of objects for the workstation to be toggled
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ForgeManager.Instance.SetInteractiveWorkStation(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // If stations overlap this will cause bugs
            ForgeManager.Instance.SetInteractiveWorkStation(null);
        }
    }
}