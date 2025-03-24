using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleInteractable : InteractableScript
{
    public GameObject cameraPerspective;
    public float cameraOrthographic;
    public override void Interact()
    {
        if (interacted)
            return;
        PlayerManager.Instance.playerPuzzle = true;
        PlayerInteraction.Instance.TriggerTextInteractable(interactText);
        interacted = true;
    }

}
