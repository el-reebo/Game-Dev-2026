using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    [Header("Sensitivity")]
    public float mouseSens = 0.1f;

    [Header("Movement Speed")]
    public float walkSpeed = 2.0f;
    public float sprintSpeed = 4.0f;

    [Header("Jump Settings")]
    public float jumpPower = 2.0f;
    public float gravityMultiplier = 1.0f;

    [Header("References")]
    [SerializeField] private CharacterController charController;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerInputHandler pih;

    private Vector3 currentMovement;
    private float verticalRotation;
    // Doesn't store variable. Recalculates current speed whenever called in functions
    private float currentSpeed => pih.SprintInput ? sprintSpeed : walkSpeed;

    private void Start()
    {
        // Lock cursor in centre and make invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        MovementHandler();
        RotationHandler();
    }

    private Vector3 CalcWorldDirection()
    {
        Vector3 inputDirection = new Vector3(pih.MovementInput.x, 0f,  pih.MovementInput.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        return worldDirection.normalized;
    }

    private void JumpHandler()
    {
        if (charController.isGrounded)
        {
            currentMovement.y = -0.5f;
            if (pih.JumpInput)
            {
                currentMovement.y = jumpPower;
            }
        }
        else
        {
            currentMovement.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
    }

    private void MovementHandler()
    {
        Vector3 worldDirection = CalcWorldDirection();
        
        // Forward/backward movement
        currentMovement.x = worldDirection.x * currentSpeed;
        // Strafe movement
        currentMovement.z = worldDirection.z * currentSpeed;   

        JumpHandler();
        charController.Move(currentMovement * Time.deltaTime);
    }

    private void HorizontalRotation(float rotationValue)
    {
        transform.Rotate(0, rotationValue, 0);
    }

    private void VerticalRotation(float rotationValue) {
        verticalRotation = Mathf.Clamp(verticalRotation - rotationValue, -90f, 90f);
        // only apply vert rotation to camera not player
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void RotationHandler()
    {
        float mouseRotationX = pih.RotationInput.x * mouseSens;
        float mouseRotationY = pih.RotationInput.y * mouseSens;

        HorizontalRotation(mouseRotationX);
        VerticalRotation(mouseRotationY);
    }
}
