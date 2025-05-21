using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerInventory : MonoBehaviour
{
    public PlayerInputs inventoryInputs;
    [Header("Inventory Objects")]
    [SerializeField] Camera inventoryCamera;
    [SerializeField] Camera mainCamera;
    [SerializeField] GameObject currentItem;
    [SerializeField] GameObject currentItemContainer;
    public GameObject leftItemContainer;
    public GameObject leftItem;
    public GameObject rightItemContainer;
    public GameObject rightItem;
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
    public int currentItemIndex = 1;
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
        inventoryInputs = new PlayerInputs();
        inventoryInputs.UI.Use.performed += ctx => UseItem();
        inventoryInputs.UI.ShiftLeft.performed += ctx => StartCoroutine(ShiftLeft());
        inventoryInputs.UI.ShiftRight.performed += ctx => StartCoroutine(ShiftRight());
        inventoryInputs.UI.Exit.performed += ctx => DisplayInventory();
        inventoryInputs.UI.Scroll.performed += ctx => ShiftInventory(ctx.ReadValue<Vector2>());
    }

    private void OnDisable()
    {
        PlayerManager.Instance.PlayerPressedInventoryButton -= DisplayInventory;
        inventoryInputs.UI.Scroll.performed -= ctx => ShiftInventory(ctx.ReadValue<Vector2>());
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
        if (UIHandler.Instance.textDisplayed)
            return;

        if (!inventoryDisplayed)
        {
            inventoryInputs.UI.Enable();
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
            inventoryInputs.UI.Disable();
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
            foreach (PlayerManager.InventoryItems ammo in PlayerManager.Instance.interactableItems)
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
            foreach (PlayerManager.InventoryItems inventory in PlayerManager.Instance.interactableItems)
            {
                if (inventory.item == selectedItem)
                {
                    selectedItem.itemDetails.GetComponent<IUsable>().Use();
                    inventory.quantity -= 1;
                    if (inventory.quantity <= 0)
                    {
                        PlayerManager.Instance.interactableItems.Remove(inventory);
                        Destroy(displayedItem);
                    }
                    SortInventory();
                    break;
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
        inventorySwitch.SwitchItemsLeft();
        inventoryInputs.UI.Disable();
        yield return new WaitForSeconds(0.3f);
        inventoryInputs.UI.Enable();
        currentItemIndex--;
        currentItem = leftItem;
        currentItem.layer = 5;
        displayedItem = leftItem;
        leftItem = null;
        SortInventory();
    }

    public IEnumerator ShiftRight()
    {

        if (currentItemIndex >= PlayerManager.Instance.interactableItems.Count - 1)
            yield return null;
        
        rightItem = Instantiate(PlayerManager.Instance.interactableItems[currentItemIndex+1].item.interactable, rightItemContainer.transform);
        inventorySwitch.SwitchItemsRight();
        inventoryInputs.UI.Disable();
        yield return new WaitForSeconds(0.3f);
        inventoryInputs.UI.Enable();
        currentItemIndex++;
        displayedItem = rightItem;
        rightItem = null;
        SortInventory();
    }
    #endregion
    
    // SortInventory is called after a major change, re-establishing the order of items and what should be displayed on the screen.
    void SortInventory()
    {
        selectedItem = PlayerManager.Instance.interactableItems[currentItemIndex].item;
        currentItem = selectedItem.interactable;
        Destroy(displayedItem);
        displayedItem = Instantiate(currentItem, currentItemContainer.transform);
        displayedItem.layer = 5;
        itemName.text = PlayerManager.Instance.interactableItems[currentItemIndex].item.itemName;
        itemText.text = PlayerManager.Instance.interactableItems[currentItemIndex].item.itemDesc;

        if (selectedItem is WeaponBP)
        {
            useText.text = "Press 'E' to reload";
            itemText.text += "<br>Ammo count " + PlayerCombat.Instance.activeWeapon.currentAmmo + "/" + PlayerCombat.Instance.activeWeapon.maxAmmoCount + ".";
        }
        else if (selectedItem.interactable.name.Contains("Ammo"))
        {
            useText.text = "";
            itemText.text += "<br>Remaining ammo: " + PlayerManager.Instance.interactableItems[currentItemIndex].quantity;
        }
        else if (selectedItem.itemDetails != null)
            useText.text = "Press 'E' to use";
    }
}
