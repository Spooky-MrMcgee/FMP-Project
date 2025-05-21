using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;

public class PlayerManager : MonoBehaviour, IDamageable
{
    public static PlayerManager Instance;
    public PlayerInputs PlayerActions;

    [Header("Player Stats")]
    public float health;
    [SerializeField] float maxPlayerHealth;
    public float speed;

    [Header("Player Objects")]
    public GameObject playerMesh; 
    public GameObject flashlight;
    public Animator animator;
    public Canvas playerCanvas;
    public Camera currentCamera;
    public GameObject virtualCamera;
    public FPTransition firstPerson;
    public RoomDetails currentRoom;
    public RoomDetails previousRoom;
    float camX, camZ;

    [Header("Player Checks")]
    public bool playerInteract;
    public bool playerPuzzle;
    public bool playerPuzzleFinished;
    public bool firstPersonFinished;
    bool hurt;
    public bool inventory;


    [Header("Player Inventory")]
    [SerializeField] List<InteractableItem> startingItems;
    public List<InventoryItems> interactableItems = new List<InventoryItems>();
    public class InventoryItems
    {
        public InteractableItem item;
        public int quantity;
    }

    public event Action PlayerPressedInventoryButton;


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
        Interacting,
        Aiming,
        Dead,
    }

    [Header("Player States")]
    public PlayerStates playerState;
    public PlayerHealthStates playerHealthState;

    private void Awake()
    {
        Instance = this;
        currentCamera = Camera.main;
    }

    private void Start()
    {
        // Adding player starting items and initialising the inventory system.
        PlayerInteraction.Instance.InteractableInteracted += UpdateInventory;
        health = maxPlayerHealth;
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
        // Hooking up Player Inputs
        PlayerActions = new PlayerInputs();
        PlayerActions.Player.PlayerInteract.performed += ctx => InteractCheck(ctx);
        PlayerActions.Player.PlayerInteract.canceled += ctx => InteractCheck(ctx);
        PlayerActions.Enable();
    }

    private void OnDisable()
    {
        // Unsubscribing from Player Inputs
        PlayerActions.Player.PlayerInteract.performed -= ctx => InteractCheck(ctx);
        PlayerActions.Player.PlayerInteract.canceled -= ctx => InteractCheck(ctx);
        PlayerInteraction.Instance.InteractableInteracted -= UpdateInventory;
    }


    private void Update()
    {
        #region Player Inputs & Camera

        if (PlayerInteraction.Instance.itemBeingInteracted == null)
            UpdateCameraPosition(null);
        else
            UpdateCameraPosition(PlayerInteraction.Instance.itemBeingInteracted.GetComponent<PuzzleInteractable>());

        if (PlayerActions.Player.PlayerInventory.WasPerformedThisFrame())
        {
            PlayerPressedInventoryButton?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && firstPerson != null)
            StartCoroutine(firstPerson.ExitFirstPersonTransition());

        #endregion

        #region First Person Handling
        if (firstPerson != null && PlayerInteraction.Instance.currentlyInteracting == false)
        {
            playerCanvas.enabled = false;
            if (firstPerson.flashlight)
            {
                // Activating flashlight and having it move according to the player's mouse.
                float singleStep = 1 * Time.deltaTime;
                flashlight.SetActive(true);
                Vector3 mousePosition = Input.mousePosition;
                Vector3 viewDir = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 30));
                flashlight.transform.LookAt(viewDir);
            }
            else
            {
                flashlight.SetActive(false);
            }
        }
        else if (firstPerson == null && !inventory)
        {
            flashlight.transform.localRotation = new Quaternion(0, 0, 0, 0);
            flashlight.SetActive(false);
            playerCanvas.enabled = true;
        }
        #endregion
  
        if (PlayerInteraction.Instance.currentlyInteracting)
            playerCanvas.enabled = true;

        #region Player State Handling

        if (playerState == PlayerStates.Dead)
        {
            HandleAnimationStates("isDead");
            return;
        }
        // Animation handler takes the names based on states and then checks the bools for them
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
            case PlayerStates.Interacting:
                HandleAnimationStates("isInteracting");
                break;
        }
        #endregion
    }

    private void HandleAnimationStates(string animationTrigger)
    {
        // Handles the animation states for the player based on current state, takes a different string for each state that correlates with an animation boolean that switches on that boolean and all others off.
        for (int i = 0; i < animator.parameterCount; i++)
        {
            AnimatorControllerParameter animController;
            animController = animator.GetParameter(i);
            if (animController.name.ToString() == animationTrigger)
            {
                animator.SetBool(animationTrigger, true);
            }
            else
                animator.SetBool(animController.name, false);
        }
    }

    public void UpdateCameraPosition(PuzzleInteractable puzzleInteractable)
    {
        // Camera position is influenced by three major aspects, if the player is in firstperson, if the player is interacting with a puzzle, or if the player is in a general state the camera changes to match that.
        if (firstPerson != null)
        {
            virtualCamera.transform.position = firstPerson.cameraPerspective.transform.position;
            virtualCamera.transform.rotation = firstPerson.cameraPerspective.transform.rotation;
            if (!firstPerson.isOrthographic)
                currentCamera.orthographic = false;
            else
                virtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize = firstPerson.orthographicPerspective;
            playerMesh.SetActive(false);
            return;
        }

        currentCamera.orthographic = true;

        if (!playerPuzzle)
        {
            if (currentRoom != previousRoom)
            {
                virtualCamera.transform.position = currentRoom.cameraPoint.transform.position;
                virtualCamera.transform.rotation = currentRoom.cameraPoint.transform.rotation;
                previousRoom = currentRoom;
            }

            if (playerPuzzleFinished)
            {
                virtualCamera.transform.position = currentRoom.cameraPoint.transform.position;
                virtualCamera.transform.rotation = currentRoom.cameraPoint.transform.rotation;
                playerPuzzleFinished = false;
            }

            if (firstPersonFinished)
            {
                virtualCamera.transform.position = currentRoom.cameraPoint.transform.position;
                virtualCamera.transform.rotation = currentRoom.cameraPoint.transform.rotation;
                firstPersonFinished = false;
            }

            #region Clamp Handling
            if (currentRoom.lockX)
                camX = virtualCamera.transform.position.x;
            else
                camX = virtualCamera.transform.position.x + playerMesh.transform.position.x;

            if (currentRoom.lockZ)
                camZ = virtualCamera.transform.position.z;
            else
                camZ = playerMesh.transform.position.z;
            
            if (currentRoom.clampX.x != 0 && currentRoom.clampX.y != 0)
            {
                if (playerMesh.transform.position.x >= currentRoom.clampX.x || playerMesh.transform.position.x <= currentRoom.clampX.y)
                    currentRoom.followPlayer = false;
                else
                    currentRoom.followPlayer = true;
            }

            if (currentRoom.clampZ.x != 0 && currentRoom.clampZ.y != 0)
            {
                if (playerMesh.transform.position.z >= currentRoom.clampZ.x || playerMesh.transform.position.z <= currentRoom.clampZ.y)
                    currentRoom.followPlayer = false;
                else
                    currentRoom.followPlayer = true;
            }

            if (currentRoom.followPlayer)
                virtualCamera.transform.position = new Vector3(camX, virtualCamera.transform.position.y, camZ);

            virtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize = currentRoom.orthographicSize;
            playerMesh.SetActive(true);
            #endregion
        }
        else
        {
            virtualCamera.transform.position = puzzleInteractable.cameraPerspective.transform.position;
            virtualCamera.transform.rotation = puzzleInteractable.cameraPerspective.transform.rotation;
            virtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize = puzzleInteractable.cameraOrthographic;
            playerMesh.SetActive(false);
        }
    }

    #region Inventory Handling
    public void UpdateInventoryCamera(Camera camera)
    {
        // Triggers the inventory camera on/off by enabling/disabling the other camera in the scene.
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
        // Whenever an item gets added to the players inventory it's sorted through here and added.
        InventoryItems invItem = new InventoryItems();
        foreach (InventoryItems inventoryItem in interactableItems)
        {
            if (inventoryItem.item == itemType)
            {
                inventoryItem.quantity += itemType.quantity;
                return;
            }
        }
        invItem.item = itemType;
        invItem.quantity = itemType.quantity;
        interactableItems.Add(invItem);
    }

    public bool SearchInventory(InteractableItem itemType)
    {
        // For when a puzzle object or key needs to be found in the players inventory for the sake of progression
        foreach (InventoryItems inventoryItem in interactableItems)
        {
            if (inventoryItem.item == itemType)
                return true;
        }
        return false;
    }
    #endregion

    public void TakeDamage(float damageAmount)
    {
        // Takes from the IDamagable interface and assigns the players health states as necessary.
        hurt = true;
        health -= damageAmount;
        if (health <= (maxPlayerHealth / 1.5) && health > maxPlayerHealth / 3)
            playerHealthState = PlayerHealthStates.Wounded;
        else if (health <= maxPlayerHealth / 3 && health > 0)
            playerHealthState = PlayerHealthStates.Critical;
        else if (health <= 0)
            playerState = PlayerStates.Dead;
        else
            playerHealthState = PlayerHealthStates.Healthy;
        
        if (playerState == PlayerStates.Dead)
        {
            StartCoroutine(SceneReload());
            return;
        }
        
        if (hurt)
        {
            HandleAnimationStates("isHurt");
            StartCoroutine("AnimationCooldown");
        }
    }

    IEnumerator SceneReload()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator AnimationCooldown()
    {
        // Coolsdown the players hurt animation so it's not constantly looped.
        hurt = false;
        yield return new WaitForSeconds(1f);
        hurt = true;
    }

    public void InteractCheck(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            playerInteract = true;
        else
            playerInteract = false;
    }
}
