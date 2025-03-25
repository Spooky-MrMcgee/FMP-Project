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
    [SerializeField] CharacterController characterController;
    [SerializeField] float speed = 6f, smoothTurnTime = 0.1f, gravity = 9.81f;
    [SerializeField] public GameObject mousePos, currentCamera;
    Vector3 forward, right, dir;
    float playerHorizontalInput, playerVerticalInput, turnSmoothVelocity;
    RaycastHit[] boxHit = null;
    Vector3 gizmoHit;
    bool canTraverseRooms = true;
    public static PlayerMovement PlayerMove;
    public bool isMoving;
    public bool nextToDoor;

    private void Awake()
    {
        PlayerMove = this;
    }


    private void OnEnable()
    {
        PlayerActions = new PlayerInputs();
        PlayerActions.Player.PlayerMove.performed += ctx => Move(ctx.ReadValue<Vector2>());
        PlayerActions.Player.PlayerMove.canceled += ctx => Move(ctx.ReadValue<Vector2>());
        PlayerActions.Enable();
    }

    void Update()
    {
        // Grabs player inputs and movements from Move() and then applies that to movement.
        dir = (playerHorizontalInput * forward) + (playerVerticalInput * right);
        if (dir.magnitude != 0 && !PlayerInteraction.Instance.currentlyInteracting)
        {
            float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, smoothTurnTime);
            if (!PlayerCombat.Instance.currentlyAiming)
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            characterController.Move(dir * (speed) * Time.deltaTime);
            
            characterController.Move(new Vector3(0, -gravity, 0) * Time.deltaTime);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(gizmoHit, new Vector3(5, 5, 5));
    }

    private void Move(Vector2 direction)
    {
        if (direction.magnitude != 0 && !PlayerCombat.Instance.currentlyAiming)
            PlayerManager.Instance.playerState = PlayerManager.PlayerStates.Walking;
        else if (direction.magnitude == 0 && !PlayerCombat.Instance.currentlyAiming)
            PlayerManager.Instance.playerState = PlayerManager.PlayerStates.Idle;

        playerHorizontalInput = direction.y;
        playerVerticalInput = direction.x;
        forward = currentCamera.transform.forward;
        right = currentCamera.transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward = forward.normalized;
        right = right.normalized;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponentInParent<CameraSwitch>())
        {
            other.gameObject.GetComponentInParent<CameraSwitch>().isColliding = true;
            currentCamera = other.gameObject.transform.parent.gameObject;
        }

        if (other.transform.tag == "RoomExit" && canTraverseRooms)
        {
            nextToDoor = true;
            if (PlayerManager.Instance.playerInteract)
            {
                Debug.Log("Player is interacting");
                characterController.enabled = false;
                Vector3 newPos = other.transform.GetComponent<RoomDoor>().connectingSpawn.transform.position;
                transform.position = new Vector3(newPos.x, newPos.y + 6, newPos.z);
                characterController.enabled = true;
                StartCoroutine(RoomCooldown());
            }
            if (!canTraverseRooms)
                nextToDoor = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {

        if (other.gameObject.GetComponentInParent<CameraSwitch>())
            currentCamera = other.gameObject.transform.parent.gameObject;

        if (other.transform.tag == "RoomExit")
        {
            nextToDoor = true;
            if (PlayerManager.Instance.playerInteract && canTraverseRooms)
            {
                Debug.Log("Player is interacting");
                characterController.enabled = false;
                Vector3 newPos = other.transform.GetComponent<RoomDoor>().connectingSpawn.transform.position;
                transform.position = new Vector3(newPos.x, newPos.y + 6, newPos.z);
                characterController.enabled = true;
                StartCoroutine(RoomCooldown());
            }
            if (!canTraverseRooms)
                nextToDoor = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponentInParent<CameraSwitch>())
            other.gameObject.GetComponentInParent<CameraSwitch>().isColliding = false;

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
}
