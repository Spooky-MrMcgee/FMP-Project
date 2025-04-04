using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleInteractable : InteractableScript
{
    [Header("Puzzle Interactables")]
    public GameObject cameraPerspective;
    public float cameraOrthographic;
    public bool causeTransition;
    public FPTransition fpTransition;
    [HideInInspector] public bool finishedInteracting;
    public override void Interact()
    {
        if (interacted)
            return;
        PlayerManager.Instance.playerPuzzle = true;
        PlayerInteraction.Instance.TriggerTextInteractable(interactText);
        interacted = true;
    }

}
