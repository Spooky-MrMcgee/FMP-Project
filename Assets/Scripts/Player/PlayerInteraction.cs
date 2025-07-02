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
    public bool canInteractAgain = true;
    public InteractableScript nearestInteractable;
    
    public event Action InteractionButtonPressed;
    public event Action<GameObject> InteractableInRange;
    public event Action<InteractableItem> InteractableInteracted;
    public event Action<List<string>> TextInteractable;
    public event Action<GameObject> InteractableNoLongerInRange; 

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
    private void InteractableNearby(GameObject interactable)
    {
        InteractableInRange?.Invoke(interactable);
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
        if (itemBeingInteracted == null)
            return;

        float interactDelay = 0.5f;

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
            PlayerManager.Instance.playerPuzzleFinished = true;
            if (puzzleInteractable.fpTransition != null)
            {
                interactDelay = 2f;
                // If the puzzle then moves to a FirstPerson transition it then handles that here.
                puzzleInteractable.fpTransition.Interact();
            }
        }
        StartCoroutine(InteractDelay(interactDelay));
        currentlyInteracting = false;
    }

    IEnumerator InteractDelay(float interactDelay)
    {
        canInteractAgain = false;
        yield return new WaitForSeconds(interactDelay);
        canInteractAgain = true;
    }

    // All of the following scripts here handle the player's interactions based on Triggers that overlap when the player attempts to use the Interact button.
    #region Trigger Interaction Handling
    void OnTriggerEnter(Collider other)
    {
        if (PlayerManager.Instance.firstPerson != null || !canInteractAgain || PlayerInventory.Instance.inventoryDisplayed)
            return;

        if (other.gameObject.GetComponent<InteractableScript>())
        {
            if (!other.gameObject.GetComponent<InteractableScript>().enabled)
                return;
                
            if (!currentlyInteracting)
            {
                if (other.gameObject.transform.Find("PopUpPlacement") != null)
                    InteractableNearby(other.gameObject.transform.Find("PopUpPlacement").gameObject);
                else
                    InteractableNearby(other.gameObject);
            }
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
                    InteractableNoLongerInRange?.Invoke(other.gameObject);
                    interactableObject.Interact();
                }
            }
        }
    }


    void OnTriggerStay(Collider other)
    {
        if (PlayerManager.Instance.firstPerson != null || !canInteractAgain || PlayerInventory.Instance.inventoryDisplayed)
            return;

        if (PlayerManager.Instance.firstPerson != null)
        {
            InteractableNoLongerInRange?.Invoke(other.gameObject);
            return;
        }

        if (other.gameObject.GetComponent<InteractableScript>())
        {
            if (!other.gameObject.GetComponent<InteractableScript>().enabled)
                return;
                
            if (!currentlyInteracting)
            {
                if (other.gameObject.transform.Find("PopUpPlacement") != null)
                    InteractableNearby(other.gameObject.transform.Find("PopUpPlacement").gameObject);
                else
                    InteractableNearby(other.gameObject);
            }
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
                    InteractableNoLongerInRange?.Invoke(other.gameObject);
                    interactableObject.Interact();
                    if (interactableObject.collectable)
                        PlayerManager.Instance.playerState = PlayerManager.PlayerStates.Interacting;
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<InteractableScript>())
            InteractableNoLongerInRange?.Invoke(other.gameObject);
    }
    #endregion
}
