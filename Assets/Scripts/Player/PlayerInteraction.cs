using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Instance;
    PlayerInputs playerInputs;
    [Header("Interaction Checks")]
    public GameObject itemBeingInteracted = null;
    public bool currentlyInteracting;
    public InteractableItem currentItem;
    public bool doneInteracting;
    bool isUsingInteract;
    bool canInteractAgain = true;
    public InteractableScript nearestInteractable;
    
    public event Action InteractionButtonPressed;
    public event Action<Vector3> InteractableInRange;
    public event Action<InteractableItem> InteractableInteracted;
    public event Action<List<string>> TextInteractable;

    private void Awake()
    {
        playerInputs = new PlayerInputs();
        Instance = this;
        playerInputs.Enable();
    }

    private void Start()
    {
        UIHandler.Instance.FinishInteracted += DoneInteracting;
    }

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
        StartCoroutine(InteractDelay());
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

    IEnumerator InteractDelay()
    {
        canInteractAgain = false;
        yield return new WaitForSeconds(0.5f);
        canInteractAgain = true;
    }

    // All of the following scripts here handle the player's interactions based on Triggers that overlap when the player attempts to use the Interact button.
    #region Trigger Interaction Handling
    void OnTriggerEnter(Collider other)
    {
        if (PlayerManager.Instance.firstPerson != null || !canInteractAgain)
            return;

        if (other.gameObject.GetComponent<InteractableScript>())
        {   
            InteractableNearby(other.transform.position);
            nearestInteractable = other.gameObject.GetComponent<InteractableScript>();
            InteractableScript interactableObject = other.GetComponent<InteractableScript>();
            if (playerInputs.Player.PlayerInteract.WasPerformedThisFrame())
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
        if (PlayerManager.Instance.firstPerson != null || !canInteractAgain)
            return;

        if (other.gameObject.GetComponent<InteractableScript>())
        {
            InteractableNearby(other.transform.position);
            InteractableScript interactableObject = other.GetComponent<InteractableScript>();
            if (playerInputs.Player.PlayerInteract.WasPerformedThisFrame())
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
        if (PlayerManager.Instance.firstPerson != null || !canInteractAgain)
            return;

        if (other.gameObject.GetComponent<InteractableScript>())
        {
            InteractableNearby(other.transform.position);
            InteractableScript interactableObject = other.GetComponent<InteractableScript>();
            if (playerInputs.Player.PlayerInteract.WasPerformedThisFrame())
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
