using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class PlayerMovement : MonoBehaviour
{
    
    PlayerInputs PlayerActions;
    public static PlayerMovement PlayerMove;
    [SerializeField] CharacterController characterController;
    [Header("Variables")]
    [SerializeField] float speed = 6f;
    [SerializeField] float sprintSpeed;
    [SerializeField] float walkSpeed;
    [SerializeField] float aimSpeed;
    [SerializeField] float smoothTurnTime = 0.1f;
    [SerializeField] float gravity = 9.81f;
    public bool isMoving;
    public bool nextToDoor;

    [Header("Game Objects")]
    public GameObject mousePos;
    public GameObject currentCamera;
    
    Vector3 forward, right, dir;
    float playerHorizontalInput, playerVerticalInput, turnSmoothVelocity;
    RaycastHit[] boxHit = null;
    Vector3 gizmoHit;
    bool canTraverseRooms = true;

    #region Event Subscriptions and Player Input
    private void Awake()
    {
        PlayerMove = this;
    }


    private void Start()
    {
        PlayerActions = PlayerManager.Instance.PlayerActions;
        PlayerActions.Player.PlayerMove.performed += ctx => Move(ctx.ReadValue<Vector2>());
        PlayerActions.Player.PlayerSprint.performed += ctx => Sprint(ctx);
        PlayerActions.Player.PlayerMove.canceled += ctx => Move(ctx.ReadValue<Vector2>());
        PlayerActions.Player.PlayerSprint.canceled += ctx => Sprint(ctx);
    }
    #endregion

    void Update()
    {
        // Grabs player inputs and movements from Move() and then applies that to movement.
        dir = (playerHorizontalInput * forward) + (playerVerticalInput * right);
        if (dir.magnitude != 0 && !PlayerInteraction.Instance.currentlyInteracting)
        {
            isMoving = true;
            float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, smoothTurnTime);
            if (!PlayerCombat.Instance.currentlyAiming)
            {
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
                if (PlayerManager.Instance.playerState == PlayerManager.PlayerStates.Sprinting)
                    speed = sprintSpeed;
                else
                    speed = walkSpeed;
            }
            else
                speed = aimSpeed;
            characterController.Move(dir * (speed) * Time.deltaTime);

            characterController.Move(new Vector3(0, -gravity, 0) * Time.deltaTime);
        }
        else
            isMoving = false;
    }

    private void Move(Vector2 direction)
    {
        // Move takes the players inputs and assigns them to directions for the player to move in, it also handles the players movement states.
        if (PlayerManager.Instance.firstPerson != null || PlayerManager.Instance.playerState == PlayerManager.PlayerStates.Dead) 
            return;

        playerHorizontalInput = direction.y;
        playerVerticalInput = direction.x;
        forward = currentCamera.transform.forward;
        right = currentCamera.transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward = forward.normalized;
        right = right.normalized;

        if (direction.magnitude == 0 && !PlayerCombat.Instance.currentlyAiming)
            PlayerManager.Instance.playerState = PlayerManager.PlayerStates.Idle;

        if (PlayerManager.Instance.playerState == PlayerManager.PlayerStates.Sprinting)
            return;

        if (direction.magnitude != 0 && !PlayerCombat.Instance.currentlyAiming && !PlayerInteraction.Instance.currentlyInteracting)
            PlayerManager.Instance.playerState = PlayerManager.PlayerStates.Walking;
    }

    private void Sprint(InputAction.CallbackContext ctx)
    {
        if (PlayerCombat.Instance.currentlyAiming)
            return;

        if (ctx.performed)
        {
            if (isMoving)
                PlayerManager.Instance.playerState = PlayerManager.PlayerStates.Sprinting;
        }
        else
        {
            if (isMoving)
                PlayerManager.Instance.playerState = PlayerManager.PlayerStates.Walking;
        }
    }

    // Room Handling takes care of making sure that the cameras and player positions are always up to date with the current room, making sure that there is a seamless transition between them.
    #region Room Handling
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "RoomExit" && canTraverseRooms)
        {
            nextToDoor = true;
            RoomDoor door = other.transform.GetComponent<RoomDoor>();
            if (PlayerManager.Instance.playerInteract)
            {
                if (door.locked)
                {
                    if (!PlayerInteraction.Instance.canInteractAgain)
                        return;

                    PlayerInteraction.Instance.currentlyInteracting = true;
                    if (!PlayerManager.Instance.SearchInventory(door.key))
                    {
                        List<string> doorLockText = new List<string>();
                        doorLockText.Add("The door is locked, I can't seem to get it open without a key.");
                        UIHandler.Instance.DisplayText(doorLockText);
                        return;
                    }

                    List<string> doorUnlockText = new List<string>();
                    doorUnlockText.Add("The door unlocked.");
                    UIHandler.Instance.DisplayText(doorUnlockText);
                    door.locked = false;
                    
                }
                characterController.enabled = false;
                Vector3 newPos = door.connectingSpawn.transform.position;
                transform.position = new Vector3(newPos.x, newPos.y + 6, newPos.z);
                characterController.enabled = true;
                AudioManager.Instance.PlaySFX("DoorClose");
                StartCoroutine(RoomCooldown());
            }
            if (!canTraverseRooms)
                nextToDoor = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {

        if (other.transform.tag == "RoomExit")
        {
            nextToDoor = true;
            if (PlayerManager.Instance.playerInteract && canTraverseRooms)
            {
                RoomDoor door = other.transform.GetComponent<RoomDoor>();
                if (door.locked)
                {
                    if (!PlayerInteraction.Instance.canInteractAgain)
                        return;

                    PlayerInteraction.Instance.currentlyInteracting = true;
                    if (!PlayerManager.Instance.SearchInventory(door.key))
                    {
                        List<string> doorLockText = new List<string>();
                        doorLockText.Add("The door is locked, I can't seem to get it open without a key.");
                        UIHandler.Instance.DisplayText(doorLockText);
                        return;
                    }

                    List<string> doorUnlockText = new List<string>();
                    doorUnlockText.Add("The door unlocked.");
                    UIHandler.Instance.DisplayText(doorUnlockText);
                    door.locked = false;
                }
                characterController.enabled = false;
                Vector3 newPos = other.transform.GetComponent<RoomDoor>().connectingSpawn.transform.position;
                transform.position = new Vector3(newPos.x, newPos.y + 6, newPos.z);
                characterController.enabled = true;
                AudioManager.Instance.PlaySFX("DoorClose");
                StartCoroutine(RoomCooldown());
            }
            if (!canTraverseRooms)
                nextToDoor = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "RoomExit")
        {
            nextToDoor = false;
        }
    }

    IEnumerator RoomCooldown()
    {
        canTraverseRooms = false;
        yield return new WaitForSeconds(0.5f);
        canTraverseRooms = true;
    }
    #endregion
}
