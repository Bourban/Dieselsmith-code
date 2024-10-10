using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CarryableGoods : Interactable
{
    private bool bIsBeingCarried;

    public override void Interact(Character InteractingCharacter)
    {
        base.Interact(InteractingCharacter);

        if (!bIsBeingCarried)
        {
            InteractableManager.Instance.StartSustainedInteraction(this, EInteractableState.Carrying);
            InteractingCharacter.PickupItem(this);
        }
        else
        {InteractableManager.Instance.EndSustainedInteraction();
            InteractingCharacter.DropItem(this);
        }

        bIsBeingCarried = !bIsBeingCarried;
    }

   
}
