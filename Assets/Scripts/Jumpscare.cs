using UnityEngine;
using System.Collections;
using Cinemachine;
using StarterAssets;

public class Jumpscare : MonoBehaviour
{
    public MonsterAIController Monster;
    public Transform JumpscarePoint;
    public Transform MainCamera;
    public PlayerInputHandler _playerInputHandler;
    public Canvas _uiCanvas;
    public Camera _itemCamera;

    private CharacterController _characterController;
    private FirstPersonController _firstPersonController;
    private CinemachinePOV _pov;
    // private CinemachinePanTilt _panTilt;
    public CinemachineVirtualCamera VirtualCamera;

    void Awake()
    {
        Monster.Jumpscare += HandleJumpscare;
        _characterController = GetComponent<CharacterController>();
        _firstPersonController = GetComponent<FirstPersonController>();
        // _pov = VirtualCamera.GetCinemachineComponent<CinemachinePOV>();
    }

    private void HandleJumpscare()
    {
        _characterController.enabled = false;
        _firstPersonController.enabled = false;
        _playerInputHandler.enabled = false;
        _uiCanvas.enabled = false;
        _itemCamera.enabled = false;

        Debug.Log("HandleJumpscare called");
        // Set player to jumpscare position
        transform.position = JumpscarePoint.position;

        Debug.Log($"JumpscarePoint position: {JumpscarePoint.position}");
        Debug.Log($"Player position after teleport: {transform.position}");


        // Set base rotation
        Vector3 rotationDir = Monster.transform.position - transform.position;
        rotationDir.y = 0f;
        transform.rotation = Quaternion.LookRotation(rotationDir);

        // Set camera rotation and +23 angle
        Quaternion tiltUp = Quaternion.Euler(-10f, 0f, 0f);
        _firstPersonController.CinemachineCameraTarget.transform.rotation = Quaternion.LookRotation(rotationDir) * tiltUp;
        // _pov.m_VerticalAxis.Value = 23f;

        // _firstPersonController.SetPitch(23f);

        // MainCamera.localRotation *= tiltUp;
        StartCoroutine(ReEnableController());
    }


    private IEnumerator ReEnableController()
    {
        yield return new WaitForSeconds(10f);
        _characterController.enabled = true;
        _firstPersonController.enabled = true;
        _playerInputHandler.enabled = true;
        _uiCanvas.enabled = true;
        _itemCamera.enabled = true;
    }
}
