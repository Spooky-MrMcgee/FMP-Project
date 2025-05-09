using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerInventory : MonoBehaviour
{
    PlayerInputs playerInputs;
    [Header("Inventory Objects")]
    [SerializeField] Camera inventoryCamera;
    [SerializeField] Camera mainCamera;
    [SerializeField] GameObject currentItem;
    [SerializeField] GameObject currentItemContainer;
    [SerializeField] GameObject displayedItem;
    [SerializeField] GameObject selectBox;
    [SerializeField] GameObject panel;

    [Header("Text Objects")]
    [SerializeField] TextMeshProUGUI itemText;
    [SerializeField] TextMeshProUGUI itemNameTop;
    [SerializeField] TextMeshProUGUI itemNameMiddle;
    [SerializeField] TextMeshProUGUI itemNameBottom;
    [SerializeField] TextMeshProUGUI useText;

    [Header("Inventory Items")]
    [SerializeField] InteractableItem topItem;
    [SerializeField] InteractableItem middleItem;
    [SerializeField] InteractableItem bottomItem;
    [SerializeField] InteractableItem selectedItem;

    [Header("Inventory Checks")]
    bool inventoryDisplayed = false;
    bool topSelected;
    bool middleSelected;
    bool bottomSelected;
    float mouseX, mouseY, sens = 3f;
    [SerializeField] int currentItemIndex = 1;
    public static PlayerInventory Instance;

    // Subscribes to events and player inputs
    #region Event Subscriptions
    private void Start()
    {
        Instance = this;
        PlayerManager.Instance.PlayerPressedInventoryButton += DisplayInventory;
    }

    private void OnEnable()
    {
        playerInputs = new PlayerInputs();
        playerInputs.UI.Scroll.performed += ctx => ShiftInventory(ctx.ReadValue<Vector2>());
    }

    private void OnDisable()
    {
        PlayerManager.Instance.PlayerPressedInventoryButton -= DisplayInventory;
        playerInputs.UI.Scroll.performed -= ctx => ShiftInventory(ctx.ReadValue<Vector2>());
    }
    #endregion


    private void Update()
    {
        // Handles item rotation based on mouse input.
        if (inventoryDisplayed)
        {
            UpdateInventory();
            if (Input.GetMouseButton(0))
            {
                mouseX = Input.GetAxisRaw("Mouse X");
                mouseY = Input.GetAxisRaw("Mouse Y");
                displayedItem.transform.Rotate(mouseY * sens, mouseX * sens, 0);
            }
        }

    }

    // Displays and hides the inventory based on whether it is currently on the screen, also assigns all the initial variables to get the inventory to load.
    public void DisplayInventory()
    {
        if (!inventoryDisplayed)
        {
            playerInputs.UI.Enable();
            playerInputs.Player.Disable();
            PlayerManager.Instance.playerCanvas.enabled = false;
            PlayerManager.Instance.inventory = true;
            inventoryDisplayed = true;
            panel.SetActive(true);
            PlayerManager.Instance.UpdateInventoryCamera(inventoryCamera);
            selectedItem = PlayerManager.Instance.interactableItems[currentItemIndex].item;
            currentItem = selectedItem.interactable;

            topItem = selectedItem;
            if ((currentItemIndex + 1) <= PlayerManager.Instance.interactableItems.Count)
                middleItem = PlayerManager.Instance.interactableItems[currentItemIndex + 1].item;
            if ((currentItemIndex + 2) < PlayerManager.Instance.interactableItems.Count)
                bottomItem = PlayerManager.Instance.interactableItems[currentItemIndex + 2].item;
            
            itemText.text = PlayerManager.Instance.interactableItems[currentItemIndex].item.itemDesc;
            displayedItem = Instantiate(currentItem, currentItemContainer.transform);
            displayedItem.layer = 5;
            itemNameTop.text = PlayerManager.Instance.interactableItems[currentItemIndex].item.itemName;
            
            // Assigns proper text tags based on whether an object is usable or has a different feature like reloading.
            if (selectedItem is WeaponBP)
                useText.text = "RELOAD";
            else
                useText.text = "USE";
            if ((currentItemIndex + 1) <= PlayerManager.Instance.interactableItems.Count)
                itemNameMiddle.text = PlayerManager.Instance.interactableItems[currentItemIndex + 1].item.itemName;
            if ((currentItemIndex + 2) < PlayerManager.Instance.interactableItems.Count)
                itemNameBottom.text = PlayerManager.Instance.interactableItems[currentItemIndex + 2].item.itemName;
        }
        else
        {
            // Hides Player Inventory
            PlayerManager.Instance.UpdateInventoryCamera(mainCamera);
            PlayerManager.Instance.inventory = false;
            PlayerManager.Instance.playerCanvas.enabled = true;
            playerInputs.UI.Disable();
            panel.SetActive(false);
            playerInputs.Player.Enable();
            Destroy(displayedItem);
            inventoryDisplayed = false;
        }
    }
    
    public void UpdateInventory()
    {
        // Makes sure to consistently update inventory so that all information on the screen is accurate.
        currentItem = selectedItem.interactable;
        itemText.text = selectedItem.itemDesc;
        itemNameTop.text = topItem.itemName;
        if (selectedItem is WeaponBP)
        {
            useText.text = "RELOAD";
            itemText.text = "Ammo count " + PlayerCombat.Instance.activeWeapon.currentAmmo + "/" + PlayerCombat.Instance.activeWeapon.maxAmmoCount + ".";
        }
        else
            useText.text = "USE";
        if ((currentItemIndex + 1) <= PlayerManager.Instance.interactableItems.Count)
            itemNameMiddle.text = middleItem.itemName;
        if ((currentItemIndex + 2) < PlayerManager.Instance.interactableItems.Count)
            itemNameBottom.text = PlayerManager.Instance.interactableItems[currentItemIndex + 2].item.itemName;
    }

    public void UseItem()
    {
        // Occurs when the player uses an object. Grabs the IUsable Interface if the object is not a weapon and uses its respective feature, otherwise it simply reloads based on current ammo count.
        if (selectedItem is WeaponBP)
        {
            WeaponBP weapon = (WeaponBP)selectedItem;
            foreach(PlayerManager.InventoryItems ammo in  PlayerManager.Instance.interactableItems)
            {
                if (ammo.item == weapon.ammoType)
                {
                    ammo.quantity = PlayerCombat.Instance.activeWeapon.Reload(ammo.quantity);
                }
                if (selectedItem.quantity == 0)
                {
                    PlayerManager.Instance.interactableItems.Remove(ammo);
                    Destroy(displayedItem);
                    displayedItem = Instantiate(currentItem, currentItemContainer.transform);
                }
            }
        }
        else if (selectedItem.itemDetails.GetComponent<IUsable>() != null)
        {
            selectedItem.itemDetails.GetComponent<IUsable>().Use();
            selectedItem.quantity -= 1;
            if (selectedItem.quantity <= 0)
            {
                foreach (PlayerManager.InventoryItems inventory in PlayerManager.Instance.interactableItems)
                {
                    if (inventory.item == selectedItem)
                    {
                        PlayerManager.Instance.interactableItems.Remove(inventory);
                        break;
                    }
                }
            }    
        }
        SortInventory();
    }

    // Inventory Scrolling handles all the button inputs from the player, displaying the respective items and their positions based on the players input.
    #region Inventory Scrolling
    private void ShiftInventory(Vector2 scrollMovement)
    {
        // Shifts the players inventory up or down based on the scroll wheel.
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

    public void MiddleItem()
    {
        if (middleItem == null)
            return;
        selectBox.transform.position = itemNameMiddle.transform.position;
        bottomSelected = false;
        topSelected = false;
        middleSelected = true;
        SortInventory();
    }

    public void TopItem()
    {
        selectBox.transform.position = itemNameTop.transform.position;
        bottomSelected = false;
        topSelected = true;
        middleSelected = false;
        SortInventory();
    }

    public void BottomItem()
    {
        if (bottomItem == null)
            return;
        selectBox.transform.position = itemNameBottom.transform.position;
        bottomSelected = true;
        topSelected = false;
        middleSelected = false;
        SortInventory();
    }
    #endregion

    // SortInventory is called after a major change, re-establishing the order of items and what should be displayed on the screen.
    void SortInventory()
    {
        if (currentItemIndex == 0 || (PlayerManager.Instance.interactableItems.Count < 3))
        {
            itemNameTop.text = PlayerManager.Instance.interactableItems[currentItemIndex].item.itemName;
            topItem = PlayerManager.Instance.interactableItems[currentItemIndex].item;
            if (PlayerManager.Instance.interactableItems.Count > 1)
            {
                itemNameMiddle.text = PlayerManager.Instance.interactableItems[currentItemIndex + 1].item.itemName;
                middleItem = PlayerManager.Instance.interactableItems[currentItemIndex + 1].item;
            }
            if (PlayerManager.Instance.interactableItems.Count > 2)
            {
                itemNameBottom.text = PlayerManager.Instance.interactableItems[currentItemIndex + 2].item.itemName;
                bottomItem = PlayerManager.Instance.interactableItems[currentItemIndex + 2].item;
            }
        }
        else if (currentItemIndex == PlayerManager.Instance.interactableItems.Count - 1)
        {
            itemNameTop.text = PlayerManager.Instance.interactableItems[currentItemIndex - 2].item.itemName;
            topItem = PlayerManager.Instance.interactableItems[currentItemIndex - 2].item;
            itemNameMiddle.text = PlayerManager.Instance.interactableItems[currentItemIndex - 1].item.itemName;
            middleItem = PlayerManager.Instance.interactableItems[currentItemIndex - 1].item;
            itemNameBottom.text = PlayerManager.Instance.interactableItems[currentItemIndex].item.itemName;
            bottomItem = PlayerManager.Instance.interactableItems[currentItemIndex].item;
        }
        else
        {
            itemNameTop.text = PlayerManager.Instance.interactableItems[currentItemIndex - 1].item.itemName;
            topItem = PlayerManager.Instance.interactableItems[currentItemIndex - 1].item;
            itemNameMiddle.text = PlayerManager.Instance.interactableItems[currentItemIndex].item.itemName;
            middleItem = PlayerManager.Instance.interactableItems[currentItemIndex].item;
            itemNameBottom.text = PlayerManager.Instance.interactableItems[currentItemIndex + 1].item.itemName;
            bottomItem = PlayerManager.Instance.interactableItems[currentItemIndex + 1].item;
        }

        if (topSelected)
            selectedItem = topItem;
        else if (middleSelected)
            selectedItem = middleItem;
        else if (bottomSelected)
            selectedItem = bottomItem;

        Debug.Log(selectedItem.name);

        currentItem = selectedItem.interactable;
        itemText.text = PlayerManager.Instance.interactableItems[currentItemIndex].item.itemDesc;
        Destroy(displayedItem);
        displayedItem = Instantiate(currentItem, currentItemContainer.transform);
        displayedItem.transform.rotation = new Quaternion(0, 0, 0, 0);
        displayedItem.layer = 5;
    }
}
