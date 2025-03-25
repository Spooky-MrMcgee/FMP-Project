using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour, IDamageable
{
    public static PlayerManager Instance;
    public GameObject playerMesh;
    public Camera currentCamera { get; private set; }
    public float health { get; private set; }
    [SerializeField] float maxPlayerHealth;
    public float speed { get; private set; }
    public bool playerInteract { get; private set; }
    public bool playerPuzzle;    
    
    public RoomDetails currentRoom { get; private set; }

    public Animator animator;

    [SerializeField] List<InteractableItem> startingItems;
    public List<InventoryItems> interactableItems { get; private set; } = new List<InventoryItems>();

    public PlayerInputs PlayerActions;
    public class InventoryItems
    {
        public InteractableItem item;
        public int quantity;
    }

    public enum PlayerHealthStates
    {
        Healthy,
        Wounded,
        Critical,
    }

    public enum PlayerStates
    {
        Idle,
        Walking,
        Sprinting,
        Aiming,
    }

    public PlayerStates playerState;
    public PlayerHealthStates playerHealthState;

    private void Awake()
    {
        Instance = this;
        currentCamera = Camera.main;
    }

    private void Start()
    {
        PlayerInteraction.Instance.InteractableInteracted += UpdateInventory;
        foreach (var item in startingItems)
        {
            InventoryItems items = new InventoryItems();
            items.item = item;
            items.quantity = item.quantity;
            interactableItems.Add(items);
        }
    }

    private void OnEnable()
    {
        PlayerActions = new PlayerInputs();
        PlayerActions.Player.PlayerInteract.performed += ctx => InteractCheck(ctx);
        PlayerActions.Player.PlayerInteract.canceled += ctx => InteractCheck(ctx);
        PlayerActions.Enable();
    }

    private void OnDisable()
    {
        PlayerActions.Player.PlayerInteract.performed -= ctx => InteractCheck(ctx);
        PlayerActions.Player.PlayerInteract.canceled -= ctx => InteractCheck(ctx);
        PlayerInteraction.Instance.InteractableInteracted -= UpdateInventory;
    }

    public event Action PlayerPressedInventoryButton;

    private void Update()
    {
        if (!playerPuzzle)
        {
            currentCamera.transform.position = currentRoom.cameraPoint.transform.position;
            currentCamera.transform.rotation = currentRoom.cameraPoint.transform.rotation;
            currentCamera.orthographicSize = currentRoom.orthographicSize;
            playerMesh.SetActive(true);
        }
        else
        {
            if (PlayerInteraction.Instance.itemBeingInteracted.GetComponent<PuzzleInteractable>())
            {
                currentCamera.transform.position = PlayerInteraction.Instance.itemBeingInteracted.GetComponent<PuzzleInteractable>().cameraPerspective.transform.position;
                currentCamera.transform.rotation = PlayerInteraction.Instance.itemBeingInteracted.GetComponent<PuzzleInteractable>().cameraPerspective.transform.rotation;
                currentCamera.orthographicSize = PlayerInteraction.Instance.itemBeingInteracted.GetComponent<PuzzleInteractable>().cameraOrthographic;
            }
            playerMesh.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            PlayerPressedInventoryButton?.Invoke();
        }

        switch (playerState)
        {
            case PlayerStates.Idle:
                HandleAnimationStates("isIdle");
                break;

            case PlayerStates.Walking:
                HandleAnimationStates("isWalking");
                break;

            case PlayerStates.Aiming:
                HandleAnimationStates("isAiming");
                break;

            case PlayerStates.Sprinting:
                HandleAnimationStates("isSprinting");
                break;
        }
    }
    
    private void HandleAnimationStates(string animationTrigger)
    {
        Debug.Log(animationTrigger);
        for (int i = 0; i < animator.parameterCount; i++)
        {
            AnimatorControllerParameter animController;
            animController = animator.GetParameter(i);
            if (animController.name.ToString() == animationTrigger)
            {
                Debug.Log(animController.name.ToString() + ", " + animationTrigger);
                animator.SetBool(animationTrigger, true);
            }
            else
                animator.SetBool(animController.name, false);
        }
    }

    public void UpdateCamera(Camera camera)
    {
        Camera[] allCameras = Camera.allCameras;
        currentCamera = camera;
        currentCamera.enabled = true;
        foreach (Camera cam in allCameras)
        {
            if (cam.enabled && cam != currentCamera)
                cam.enabled = false;
        }
    }

    private void UpdateInventory(InteractableItem itemType)
    {
        InventoryItems invItem = new InventoryItems();
        foreach (InventoryItems inventoryItem in interactableItems)
        {
            if (inventoryItem.item == itemType)
            {
                inventoryItem.quantity += itemType.quantity;
                Debug.Log(inventoryItem.quantity);
                return;
            }
        }
        invItem.item = itemType;
        invItem.quantity = itemType.quantity;
        interactableItems.Add(invItem);
    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        if (health >= (maxPlayerHealth / 1.5))
            playerHealthState = PlayerHealthStates.Wounded;
        else if (health >= maxPlayerHealth / 3)
            playerHealthState = PlayerHealthStates.Critical;
        else
            playerHealthState = PlayerHealthStates.Healthy;
        HandleAnimationStates("isDamaged");
    }

    public void InteractCheck(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            playerInteract = true;
        else
            playerInteract = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.tag == "Room")
            currentRoom = other.transform.root.GetComponent<RoomDetails>();
    }
}
