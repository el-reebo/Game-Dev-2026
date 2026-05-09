using UnityEngine;
using StarterAssets;
using System.Collections;

public class PlayerSound : MonoBehaviour
{
    public CharacterController _playerController;
    public FirstPersonController _fpsController;

    [Header("Action Audio Settings")]
    [SerializeField] private float WalkRadius = 10f;
    [SerializeField] private float WalkPriority = 50f;
    [SerializeField] private float RunRadius = 20f;
    [SerializeField] private float RunPriority = 60f;
    [SerializeField] private float MaxRadius = 200f;

    [Header("Microphone Settings")]
    public int SampleWindow = 128;
    public float Threshold = 0f;
    public float SensitivityMultiplier = 1f;

    // Gun sound handled in gun script

    private float PlayerSpeed;
    private float WalkSpeed;
    private float RunSpeed;

    private float currentSoundRadius = 0f;
    private float SoundPriority = 0f;
    private AudioClip MicrophoneClip;
    private string MicName;

    void Awake()
    {
        _fpsController = GetComponent<FirstPersonController>();
        // Debug.Log("PlayerSound script");

        SetMicAudioClip(0);

        WalkSpeed = _fpsController.MoveSpeed;
        RunSpeed = _fpsController.SprintSpeed;
    }

// --- Microphone Functions ---
    private void SetMicAudioClip(int chosenMic)
    {
        MicName = Microphone.devices[chosenMic];
        MicrophoneClip = Microphone.Start(MicName, true, 10, AudioSettings.outputSampleRate);
    }

    public float GetAudioClipVolume(int clipPos, AudioClip clip)
    {
        int startPos = clipPos - SampleWindow;
        if (startPos < 0) return 0;

        float[] waveData = new float[SampleWindow];
        clip.GetData(waveData, startPos);

        float totalVol = 0;
        foreach (var sample in waveData)
        {
            totalVol += Mathf.Abs(sample);
        }

        return totalVol / SampleWindow;
    }

    public float GetVolumeFromMic()
    {
        return GetAudioClipVolume(Microphone.GetPosition(MicName), MicrophoneClip);
    }


    private void OnDrawGizmos()
    {
        if (currentSoundRadius > 0f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, currentSoundRadius);
        }
        
    }

    void Update()
    {
        currentSoundRadius = 0f;
        PlayerSpeed = _playerController.velocity.magnitude;
        // Debug.Log($"Player Speed: {PlayerSpeed}");

    // Movement noise
        if (PlayerSpeed > RunSpeed - 0.5f)
        {
            currentSoundRadius += RunRadius;
            SoundPriority += RunPriority;
        }
        else if (PlayerSpeed > WalkSpeed - 0.5f)
        {
            currentSoundRadius += WalkRadius;
            SoundPriority += WalkPriority;
        }
        else
        {
            currentSoundRadius = 0f;
        }

        float micVolume = GetVolumeFromMic() * SensitivityMultiplier;
        if (micVolume < Threshold) micVolume = 0;
        // Debug.Log($"Mic Volume: {micVolume}");

        if (micVolume > currentSoundRadius)
            currentSoundRadius = Mathf.Min(MaxRadius, micVolume);

        if (currentSoundRadius > 0f)
        {
            // Debug.Log($"Sound Radius: {currentSoundRadius}");
            Sounds.MakeSound(new Sound(transform.position, currentSoundRadius, SoundPriority));
        }
    }


}
