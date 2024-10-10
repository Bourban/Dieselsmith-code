using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EInteractableState
{
    Default,
    WorkStation,
    Carrying
}


//TODO - consider just moving the functionality of this class to Character
public class InteractableManager : MonoBehaviour
{
    // Singleton instance
    public static InteractableManager Instance;

    public List<Interactable> OverlappingInteractables;

    private EInteractableState CurrentInteractableState;

    [SerializeField]
    private Character PlayerChar;
    
    // Start is called before the first frame update
    void Start()
    {
        if (Instance)
        {
            Debug.LogError("Trying to instantiate a second InteractableManager.");
            Destroy(this);
        }
        Instance = this;
    }

    //Make sure that if we're in range of any Interactables, we set the most recent ones as the Focussed Interactable
    public void RefreshInteractables()
    {
        switch (CurrentInteractableState)
        {
            case EInteractableState.Default:
                if (OverlappingInteractables.Count == 0)
                {
                    //TODO further refine by interactable type/state
                    PlayerChar.FocussedInteractable = null;
                    return;
                }

                // Get the most recent Overlapping interactable and make it our focussed interactable
                PlayerChar.FocussedInteractable = OverlappingInteractables[^1];
                break;
            case EInteractableState.Carrying:
            case EInteractableState.WorkStation:
                // nothing to do here, FocussedInteractable is the thing we're currently doing.
                break;
        }
       
        if (PlayerChar.FocussedInteractable)
        {
            HudManager.Instance.ShowAndUpdateInteractText(PlayerChar.FocussedInteractable.Name, true);
        }
        else
        {
            HudManager.Instance.ShowInteractText(false);
        }
    }

    public EInteractableState GetInteractableState()
    {
        return CurrentInteractableState;
    }
    
    // A bit flimsy, maybe validate NewState
    public void StartSustainedInteraction(Interactable InteractingGameObject, EInteractableState NewState)
    {
        CurrentInteractableState = NewState;
        PlayerChar.FocussedInteractable = InteractingGameObject;
        
        RefreshInteractables();
    }

    public void EndSustainedInteraction()
    {
        CurrentInteractableState = EInteractableState.Default;
        RefreshInteractables();
    }
}
