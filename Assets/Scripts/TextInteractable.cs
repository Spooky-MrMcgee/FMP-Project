using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextInteractable : InteractableScript
{
    // Text Interactable is a type of InteractableScript that specifically gives the player text information, this applies mostly to consumable objects and flavour objects found in the scenes.
    public override void Interact()
    {
        if (interacted)
            return;
        PlayerInteraction.Instance.TriggerTextInteractable(interactText);
        interacted = true;
    }
}
