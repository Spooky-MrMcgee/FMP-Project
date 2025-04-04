using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item")]
public class InteractableItem : ScriptableObject
{
    public string itemName;
    public string itemDesc;
    public int quantity;
    public GameObject interactable;
    public GameObject itemDetails;
}
