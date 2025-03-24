using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerInventory : MonoBehaviour
{
    PlayerInputs playerInputs;
    [SerializeField] Camera inventoryCamera, mainCamera;
    [SerializeField] GameObject currentItem, currentItemContainer, displayedItem;
    [SerializeField] TextMeshProUGUI itemText, itemNameTop, itemNameMiddle, itemNameBottom, useText;
    [SerializeField] InteractableItem item;
    bool inventoryDisplayed = false;
    float mouseX, mouseY, sens = 3f;
    [SerializeField] int currentItemIndex = 1;


    private void Start()
    {
        PlayerManager.Instance.PlayerPressedInventoryButton += DisplayInventory;
    }

    private void OnEnable()
    {
        playerInputs = new PlayerInputs();
        playerInputs.UI.Scroll.performed += ctx => UpdateInventory(ctx.ReadValue<Vector2>());
    }

    private void OnDisable()
    {
        PlayerManager.Instance.PlayerPressedInventoryButton -= DisplayInventory;
        playerInputs.UI.Scroll.performed -= ctx => UpdateInventory(ctx.ReadValue<Vector2>());
    }

    private void Update()
    {
        if (inventoryDisplayed)
        {
            if (Input.GetMouseButton(0))
            {
                mouseX = Input.GetAxisRaw("Mouse X");
                mouseY = Input.GetAxisRaw("Mouse Y");
                displayedItem.transform.Rotate(mouseY * sens, mouseX * sens, 0);
                Debug.Log("Item is being rotated");
            }
        }

    }

    private void DisplayInventory()
    {
        if (!inventoryDisplayed)
        {
            playerInputs.UI.Enable();
            playerInputs.Player.Disable();
            inventoryDisplayed = true;
            PlayerManager.Instance.UpdateCamera(inventoryCamera);
            currentItem = PlayerManager.Instance.interactableItems[currentItemIndex].item.interactable;
            item = PlayerManager.Instance.interactableItems[currentItemIndex].item;
            itemText.text = PlayerManager.Instance.interactableItems[currentItemIndex].item.itemDesc;
            displayedItem = Instantiate(currentItem, currentItemContainer.transform);
            displayedItem.layer = 5;
            itemNameTop.text = PlayerManager.Instance.interactableItems[currentItemIndex].item.itemName;
            if (item is WeaponBP)
                useText.text = "RELOAD";
            else
                useText.text = "USE";
            if (PlayerManager.Instance.interactableItems.Count > 1)
                itemNameMiddle.text = PlayerManager.Instance.interactableItems[currentItemIndex + 1].item.itemName;
            if (PlayerManager.Instance.interactableItems.Count > 2)
                itemNameBottom.text = PlayerManager.Instance.interactableItems[currentItemIndex + 2].item.itemName;
        }
        else
        {
            PlayerManager.Instance.UpdateCamera(mainCamera);
            playerInputs.UI.Disable();
            playerInputs.Player.Enable();
            inventoryDisplayed = false;
        }
    }

    public void UseItem()
    {
        if (item is WeaponBP)
        {
            WeaponBP weapon = (WeaponBP)item;
            foreach(PlayerManager.InventoryItems ammo in  PlayerManager.Instance.interactableItems)
            {
                if (ammo.item == weapon.ammoType)
                {
                    Debug.Log(ammo.quantity);
                    ammo.quantity = PlayerCombat.Instance.activeWeapon.Reload(ammo.quantity);
                    Debug.Log(ammo.quantity);
                }
                if (item.quantity == 0)
                {
                    PlayerManager.Instance.interactableItems.Remove(ammo);
                }
            }
        }
    }

    private void UpdateInventory(Vector2 scrollMovement)
    {
        Debug.Log(scrollMovement);
        if (scrollMovement.y == 1 && currentItemIndex < (PlayerManager.Instance.interactableItems.Count - 1))
            ShiftUp();
        if (scrollMovement.y == -1 && currentItemIndex > 0)
            ShiftDown();
        SortInventory();
    }

    public void ShiftDown()
    {
        if (currentItemIndex < (PlayerManager.Instance.interactableItems.Count - 1) && PlayerManager.Instance.interactableItems.Count >= 3)
            currentItemIndex++;
        SortInventory();
    }

    public void ShiftUp()
    {
        if (currentItemIndex > 0)
            currentItemIndex--;
        SortInventory();
    }

    public void ShiftMiddle()
    {
        for (int x = 0; x >= PlayerManager.Instance.interactableItems.Count; x++)
        {
            if (item == PlayerManager.Instance.interactableItems[x].item)
            {
                if (currentItemIndex > x)
                    ShiftUp();
                else if (currentItemIndex < x)
                    ShiftDown();
            }
        }
    }

    void SortInventory()
    {
        currentItem = PlayerManager.Instance.interactableItems[currentItemIndex].item.interactable;
        itemText.text = PlayerManager.Instance.interactableItems[currentItemIndex].item.itemDesc;
        Destroy(displayedItem);
        displayedItem = Instantiate(currentItem, currentItemContainer.transform);
        displayedItem.transform.rotation = new Quaternion(0, 0, 0, 0);
        displayedItem.layer = 5;
        item = PlayerManager.Instance.interactableItems[currentItemIndex].item;
        if (currentItemIndex == 0 || (PlayerManager.Instance.interactableItems.Count < 3))
        {
            itemNameTop.text = PlayerManager.Instance.interactableItems[currentItemIndex].item.itemName;
            if (PlayerManager.Instance.interactableItems.Count > 1)
                itemNameMiddle.text = PlayerManager.Instance.interactableItems[currentItemIndex + 1].item.itemName;
            if (PlayerManager.Instance.interactableItems.Count > 2)
                itemNameBottom.text = PlayerManager.Instance.interactableItems[currentItemIndex + 2].item.itemName;
        }
        else if (currentItemIndex == PlayerManager.Instance.interactableItems.Count - 1)
        {
            itemNameTop.text = PlayerManager.Instance.interactableItems[currentItemIndex - 2].item.itemName;
            itemNameMiddle.text = PlayerManager.Instance.interactableItems[currentItemIndex - 1].item.itemName;
            itemNameBottom.text = PlayerManager.Instance.interactableItems[currentItemIndex].item.itemName;
        }
        else
        {
            itemNameTop.text = PlayerManager.Instance.interactableItems[currentItemIndex - 1].item.itemName;
            itemNameMiddle.text = PlayerManager.Instance.interactableItems[currentItemIndex].item.itemName;
            itemNameBottom.text = PlayerManager.Instance.interactableItems[currentItemIndex + 1].item.itemName;
        }
    }
}
