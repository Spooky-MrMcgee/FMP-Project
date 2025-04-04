using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Instance;
    [Header("Interaction Checks")]
    public GameObject itemBeingInteracted = null;
    public bool currentlyInteracting;
    public InteractableItem currentItem;
    public bool doneInteracting;
    public InteractableScript nearestInteractable;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UIHandler.Instance.FinishInteracted += DoneInteracting;
    }
    public event Action<Vector3> InteractableInRange;
    public event Action<InteractableItem> InteractableInteracted;
    public event Action<List<string>> TextInteractable;

    // Event notifies if an item is within range for UI handlers to grab and activate
    private void InteractableNearby(Vector3 itemPosition)
    {
        InteractableInRange?.Invoke(itemPosition);
    }

    // Event notifies if an item has been interacted with and picked up.
    private void InteractablePickedUp(InteractableItem interactable)
    {
        InteractableInteracted?.Invoke(interactable);
    }

    public void TriggerTextInteractable(List<string> textInteract)
    {
        TextInteractable?.Invoke(textInteract);
    }

    void DoneInteracting()
    {
        // Removes all interaction data after an interaction is finished.
        currentlyInteracting = false;
        InteractableScript interactable = itemBeingInteracted.GetComponent<InteractableScript>();
        if (interactable.canDespawn)
            itemBeingInteracted.SetActive(false);
        if (interactable.collectable)
            InteractablePickedUp(itemBeingInteracted.GetComponent<InteractableScript>().interactable);

        interactable.interacted = false;

        if (interactable is PuzzleInteractable)
        {
            PuzzleInteractable puzzleInteractable = itemBeingInteracted.GetComponent<PuzzleInteractable>();
            PlayerManager.Instance.playerPuzzle = false;
            if (puzzleInteractable.fpTransition == null)
                return;
            // If the puzzle then moves to a FirstPerson transition it then handles that here.
            puzzleInteractable.fpTransition.Interact();
        }
    }

    // All of the following scripts here handle the player's interactions based on Triggers that overlap when the player attempts to use the Interact button.
    #region Trigger Interaction Handling
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<InteractableScript>())
        {   
            InteractableNearby(other.transform.position);
            nearestInteractable = other.gameObject.GetComponent<InteractableScript>();
            InteractableScript interactableObject = other.GetComponent<InteractableScript>();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!currentlyInteracting)
                {
                    PlayerManager.Instance.playerState = PlayerManager.PlayerStates.Idle;
                    itemBeingInteracted = other.gameObject;
                    currentlyInteracting = true;
                }
                if (currentlyInteracting && itemBeingInteracted == other.gameObject)
                {
                    interactableObject.Interact();
                }
            }
        }
    }


    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<InteractableScript>())
        {
            InteractableNearby(other.transform.position);
            InteractableScript interactableObject = other.GetComponent<InteractableScript>();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!currentlyInteracting)
                {
                    PlayerManager.Instance.playerState = PlayerManager.PlayerStates.Idle;
                    itemBeingInteracted = other.gameObject;
                    currentlyInteracting = true;
                }
                if (currentlyInteracting && itemBeingInteracted == other.gameObject)
                {
                    interactableObject.Interact();
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<InteractableScript>())
        {
            InteractableNearby(other.transform.position);
            InteractableScript interactableObject = other.GetComponent<InteractableScript>();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!currentlyInteracting)
                {
                    itemBeingInteracted = other.gameObject;
                    currentlyInteracting = true;
                }
                if (currentlyInteracting && itemBeingInteracted == other.gameObject)
                {
                    interactableObject.Interact();
                }
            }
        }
    }
    #endregion
}
