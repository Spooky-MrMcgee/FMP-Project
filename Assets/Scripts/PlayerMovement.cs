using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // A quick note that currently PlayerMovement does not consider camera orientation, and I will need to eventually remap the way the inputs work to match the camera orientations.
    PlayerInputs PlayerActions;
    [SerializeField] CharacterController characterController;
    [SerializeField] float speed = 6f;
    [SerializeField] float smoothTurnTime = 0.1f;
    Vector3 forward, right, dir;
    float playerHorizontalInput, playerVerticalInput;
    [SerializeField] GameObject currentCamera;
    float turnSmoothVelocity;

    private void Awake()
    {
        PlayerActions = new PlayerInputs();
        PlayerActions.Movement.PlayerMove.performed += ctx => Move(ctx.ReadValue<Vector2>());
        PlayerActions.Movement.PlayerMove.canceled += ctx => Move(ctx.ReadValue<Vector2>());
    }

    private void OnEnable()
    {
        PlayerActions.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        //float horizontal = Input.GetAxisRaw("Horizontal");
        //float vertical = Input.GetAxisRaw("Vertical");
        //Vector3 dir = new Vector3(horizontal, 0f, vertical).normalized;

        print(dir.magnitude);
        dir = (playerHorizontalInput * forward) + (playerVerticalInput * right);
        if (dir.magnitude != 0)
        {
            Debug.Log("AAAAA");
            float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, smoothTurnTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            characterController.Move(dir * speed * Time.deltaTime);
        }
    }

    private void Move(Vector2 direction)
    {
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
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponentInParent<CameraSwitch>())
        {
            currentCamera = other.gameObject.transform.parent.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponentInParent<CameraSwitch>())
        {
            other.gameObject.GetComponentInParent<CameraSwitch>().isColliding = false;
        }
    }
}
