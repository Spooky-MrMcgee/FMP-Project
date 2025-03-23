using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextInteractable : InteractableScript
{
    public override void Interact()
    {
        if (interacted)
            return;
        PlayerInteraction.Instance.TriggerTextInteractable(interactText);
        interacted = true;
    }
}
