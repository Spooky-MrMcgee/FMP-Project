using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseMedkit : InventoryUse
{
    [SerializeField] InteractableItem bandages, hydroflouricAcid, medkit;
    public override void Use()
    {
        PlayerManager.InventoryItems inventoryItemsBandage = new PlayerManager.InventoryItems();
        PlayerManager.InventoryItems inventoryItemsAcid = new PlayerManager.InventoryItems();
        inventoryItemsAcid.item = hydroflouricAcid;
        inventoryItemsAcid.quantity = hydroflouricAcid.quantity;
        inventoryItemsBandage.item = bandages;
        inventoryItemsBandage.quantity = bandages.quantity;
        PlayerManager.Instance.interactableItems.Add(inventoryItemsBandage);
        PlayerManager.Instance.interactableItems.Add(inventoryItemsAcid);
        PlayerManager.Instance.RemoveInventory(medkit);
    }

    public override void Combine()
    {
        throw new System.NotImplementedException();
    }
}
