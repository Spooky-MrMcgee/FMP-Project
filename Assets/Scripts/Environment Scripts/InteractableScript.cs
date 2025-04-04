using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableScript : MonoBehaviour, IInteractable
{
    [Header("Base Variables")]
    [SerializeField] public List<string> interactText;
    [SerializeField] public bool collectable;
    [SerializeField] public InteractableItem interactable;
    [HideInInspector] public bool itemDespawn;
    [HideInInspector] public bool beingInteracted;
    public bool canDespawn;
    [HideInInspector] public bool interacted;

    public abstract void Interact();
}
