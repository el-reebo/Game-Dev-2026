using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private InputActionAsset playerControls;

    [Header("Action Map Name Reference")]
    [SerializeField] private string actionMapName = "Player";

    [Header("Action Name References")]
    [SerializeField] private string movement = "Movement";
    [SerializeField] private string rotation = "Rotation";
    [SerializeField] private string jump = "Jump";
    [SerializeField] private string sprint = "Sprint";
    [SerializeField] private string shoot = "Shoot";

    private InputAction movementAction;
    private InputAction rotationAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction shootAction;

    // Holds inputs passed in
    public Vector2 MovementInput { get; private set; }
    public Vector2 RotationInput { get; private set; }
    public bool JumpInput { get; private set; }
    public bool SprintInput { get; private set; }
    public bool ShootInput { get; private set; }

    void Awake()
    {
        // Get map references + individual actions
        InputActionMap mapReference = playerControls.FindActionMap(actionMapName);
        movementAction = mapReference.FindAction(movement);
        rotationAction = mapReference.FindAction(rotation);
        jumpAction = mapReference.FindAction(jump);
        sprintAction = mapReference.FindAction(sprint);
        shootAction = mapReference.FindAction(shoot);

        SubActionToInput();
    }

    void SubActionToInput()
    {
        // If movememnt performed, pass vector into MovementInput
        movementAction.performed += inputInfo => MovementInput = inputInfo.ReadValue<Vector2>();
        movementAction.canceled += inputInfo => MovementInput = Vector2.zero;

        rotationAction.performed += inputInfo => RotationInput = inputInfo.ReadValue<Vector2>();
        rotationAction.canceled += inputinfo => RotationInput = Vector2.zero;

        jumpAction.performed += inputInfo => JumpInput = true;
        jumpAction.canceled += inputInfo => JumpInput = false;

        sprintAction.performed += inputInfo => SprintInput = true;
        sprintAction.canceled += inputInfo => SprintInput = false;

        shootAction.performed += inputInfo => ShootInput = true;
        shootAction.canceled += inputInfo => ShootInput = false;
    }

    // Ensure player controls don't work if script is disabled
    private void OnEnable()
    {
        playerControls.FindActionMap(actionMapName).Enable();
    }
    private void OnDisable()
    {
        playerControls.FindActionMap(actionMapName).Disable();
    }

}
