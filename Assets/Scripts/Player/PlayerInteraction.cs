using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Instance;
    LayerMask mask;
    public GameObject itemBeingInteracted;
    public bool currentlyInteracting;
    public bool doneInteracting;
    private void Awake()
    {
        Instance = this;
        mask = (2);
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
        Debug.Log("I'm being called from here!");
        TextInteractable?.Invoke(textInteract);
    }

    void DoneInteracting()
    {
        currentlyInteracting = false;
        if (itemBeingInteracted.GetComponent<InteractableScript>().canDespawn)
            itemBeingInteracted.SetActive(false);
        if (itemBeingInteracted.GetComponent<InteractableScript>().collectable)
            InteractablePickedUp(itemBeingInteracted.GetComponent<InteractableScript>().interactable);
        itemBeingInteracted.GetComponent<InteractableScript>().interacted = false;
    }

    void OnTriggerEnter(Collider other)
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
}
