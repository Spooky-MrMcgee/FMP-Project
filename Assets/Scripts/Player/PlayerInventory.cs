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
    [SerializeField] GameObject leftItemContainer;
    [SerializeField] GameObject leftItem;
    [SerializeField] GameObject rightItemContainer;
    [SerializeField] GameObject rightItem;
    [SerializeField] GameObject displayedItem;
    [SerializeField] GameObject selectBox;
    [SerializeField] GameObject panel;

    [Header("Text Objects")]
    [SerializeField] TextMeshProUGUI itemText;
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] TextMeshProUGUI useText;

    [Header("Inventory Items")]
    [SerializeField] InteractableItem selectedItem;

    [Header("Inventory Checks")]
    bool inventoryDisplayed = false;
    bool topSelected;
    bool middleSelected;
    bool bottomSelected;
    float mouseX, mouseY, sens = 3f;
    [SerializeField] int currentItemIndex = 1;
    [SerializeField] Animator inventoryAnimator;
    [SerializeField] InventorySwitch inventorySwitch;
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
        playerInputs.UI.Use.performed += ctx => UseItem();
        playerInputs.UI.ShiftLeft.performed += ctx => StartCoroutine(ShiftLeft());
        playerInputs.UI.ShiftRight.performed += ctx => StartCoroutine(ShiftRight());
        playerInputs.UI.Exit.performed += ctx => DisplayInventory();
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
            PlayerManager.Instance.PlayerActions.Player.Disable();
            PlayerManager.Instance.playerCanvas.enabled = false;
            PlayerManager.Instance.inventory = true;
            inventoryDisplayed = true;
            panel.SetActive(true);
            selectedItem = PlayerManager.Instance.interactableItems[currentItemIndex].item;
            currentItem = selectedItem.interactable;


            itemName.text = PlayerManager.Instance.interactableItems[currentItemIndex].item.itemName;
            itemText.text = PlayerManager.Instance.interactableItems[currentItemIndex].item.itemDesc;
            displayedItem = Instantiate(currentItem, currentItemContainer.transform);
            displayedItem.layer = 5;
            
            // Assigns proper text tags based on whether an object is usable or has a different feature like reloading.
            if (selectedItem is WeaponBP)
            {
                useText.text = "Press 'E' to reload";
                itemText.text += "<br>Ammo count " + PlayerCombat.Instance.activeWeapon.currentAmmo + "/" + PlayerCombat.Instance.activeWeapon.maxAmmoCount + ".";
            }    
            else
                useText.text = "Press 'E' to use";
        }
        else
        {
            // Hides Player Inventory
            PlayerManager.Instance.inventory = false;
            PlayerManager.Instance.playerCanvas.enabled = true;
            playerInputs.UI.Disable();
            panel.SetActive(false);
            PlayerManager.Instance.PlayerActions.Player.Enable();
            Destroy(displayedItem);
            inventoryDisplayed = false;
        }
    }
    
    public void UpdateInventory()
    {
        // Makes sure to consistently update inventory so that all information on the screen is accurate.
        currentItem = selectedItem.interactable;
        itemText.text = selectedItem.itemDesc;

        if (selectedItem is WeaponBP)
        {
            useText.text = "Press 'E' to reload";
            itemText.text += "<br>Ammo count " + PlayerCombat.Instance.activeWeapon.currentAmmo + "/" + PlayerCombat.Instance.activeWeapon.maxAmmoCount + ".";
        }
        else
            useText.text = "Press 'E' to use";
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

    }

    public IEnumerator ShiftLeft()
    {
        if (currentItemIndex == 0)
            yield return null;

        leftItem = Instantiate(PlayerManager.Instance.interactableItems[currentItemIndex-1].item.interactable, leftItemContainer.transform);
        inventoryAnimator.SetBool("shiftLeft", true);
        yield return new WaitForSeconds(0.25f);
        inventoryAnimator.SetBool("shiftLeft", false);
        inventorySwitch.SwitchItemsLeft();
        currentItemIndex--;
        //currentItem = leftItem;
        //currentItem.layer = 5;
        //displayedItem = leftItem;
        //leftItem = null;
        SortInventory();
    }

    public IEnumerator ShiftRight()
    {

        if (currentItemIndex >= PlayerManager.Instance.interactableItems.Count - 1)
            yield return null;
        
        rightItem = Instantiate(PlayerManager.Instance.interactableItems[currentItemIndex+1].item.interactable, rightItemContainer.transform);
        inventoryAnimator.SetBool("shiftRight", true);
        yield return new WaitForSeconds(0.25f);
        inventoryAnimator.SetBool("shiftRight", false);
        currentItemIndex++;
        //inventorySwitch.SwitchItemsRight();
        //displayedItem = rightItem;
        //rightItem = null;
        SortInventory();
    }
    #endregion
    
    // SortInventory is called after a major change, re-establishing the order of items and what should be displayed on the screen.
    void SortInventory()
    {
        selectedItem = PlayerManager.Instance.interactableItems[currentItemIndex].item;
        currentItem = selectedItem.interactable;
        currentItem.layer = 5;
        itemName.text = PlayerManager.Instance.interactableItems[currentItemIndex].item.itemName;
        itemText.text = PlayerManager.Instance.interactableItems[currentItemIndex].item.itemDesc;

        if (selectedItem is WeaponBP)
        {
            useText.text = "Press 'E' to reload";
            itemText.text += "<br>Ammo count " + PlayerCombat.Instance.activeWeapon.currentAmmo + "/" + PlayerCombat.Instance.activeWeapon.maxAmmoCount + ".";
        }
        else
            useText.text = "Press 'E' to use";
    }
}
