using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableScript : MonoBehaviour, IInteractable
{
    [SerializeField] protected List<string> interactText;
    [SerializeField] public bool collectable;
    [SerializeField] public InteractableItem interactable;
    public bool itemDespawn;
    public bool beingInteracted;
    public bool canDespawn;
    [SerializeField] public bool interacted;

    public abstract void Interact();
}
