using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

// This just mirrors the enum in InteractableManager - do we need it?
public enum EInteractableType
{
    WorkStation,
    Carryable,
    Generic
}

[RequireComponent(typeof(BoxCollider))]
public abstract class Interactable : MonoBehaviour
{
    [SerializeField] 
    public string Name;
    [SerializeField]
    private EInteractableType InteractableType;

    private bool bIsInteracting = false;
    
    protected BoxCollider InteractBox;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InteractableManager.Instance.OverlappingInteractables.Add(this);
        }
        InteractableManager.Instance.RefreshInteractables();
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InteractableManager.Instance.OverlappingInteractables.Remove(this);
        }
        InteractableManager.Instance.RefreshInteractables();
    }

    public virtual void Interact(Character InteractingCharacter)
    {
        //TODO - check if we're actually in range here
        
        if (bIsInteracting)
        {
            InteractableManager.Instance.EndSustainedInteraction();
            bIsInteracting = false;
            return;
        }
        
        switch (InteractableType)
        {
            case EInteractableType.Generic:
                break;
            case EInteractableType.Carryable:
                InteractableManager.Instance.StartSustainedInteraction(this, EInteractableState.Carrying);
                break;
            case EInteractableType.WorkStation:
                InteractableManager.Instance.StartSustainedInteraction(this, EInteractableState.WorkStation);
                break;
        }

        bIsInteracting = true;
    }

    protected void Awake()
    {
        InteractBox = gameObject.GetComponent<BoxCollider>();
    }
}
