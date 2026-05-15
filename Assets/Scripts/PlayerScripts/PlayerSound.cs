using UnityEngine;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;

public class PlayerSound : MonoBehaviour
{
    public CharacterController _playerController;
    public FirstPersonController _fpsController;

    [Header("Movement SFX")]
    public List<AudioClip> walkFS;
    public List<AudioClip> runFS;
    public AudioSource walkSource, runSource;

    [Header("Action Audio Settings")]
    [SerializeField] private float WalkRadius = 10f;
    [SerializeField] private float WalkPriority = 50f;
    [SerializeField] private float RunRadius = 20f;
    [SerializeField] private float RunPriority = 60f;
    public float MaxRadius = 200f;

    [Header("Microphone Settings")]
    public int ChosenMic = 0;
    public int SampleWindow = 128;
    public float Threshold = 0f;
    public float SensitivityMultiplier = 1f;

    [Header("Public Variables")]
    public float CurrentSoundRadius = 0f;

    // Gun sound handled in gun script

    private float PlayerSpeed;
    private float WalkSpeed;
    private float RunSpeed;

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
        if (CurrentSoundRadius > 0f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, CurrentSoundRadius);
        }
        
    }

    void Update()
    {
        CurrentSoundRadius = 0f;
        PlayerSpeed = _playerController.velocity.magnitude;
        // Debug.Log($"Player Speed: {PlayerSpeed}");

        // prevent audio playing after jumpscare
        if (!_playerController.enabled)
        {
            walkSource.enabled = false;
            runSource.enabled = false;
        }

        // Movement noise
        if (PlayerSpeed > RunSpeed - 0.5f)
        {
            // Update detection range
            CurrentSoundRadius += RunRadius;
            SoundPriority += RunPriority;

            // Play audio

            //runSource.clip = runFS[Random.Range(0, runFS.Count)];
            //walkSource.clip = default;
            walkSource.enabled = false;
            runSource.enabled = true;
        }
        else if (PlayerSpeed > WalkSpeed - 0.5f)
        {
            CurrentSoundRadius += WalkRadius;
            SoundPriority += WalkPriority;

            //walkSource.clip = walkFS[Random.Range(0, walkFS.Count)];
            //runSource.clip = default;
            walkSource.enabled = true;
            runSource.enabled = false;
        }
        else
        {
            CurrentSoundRadius = 0f;

            //runSource.clip = default;
            //walkSource.clip = default;
            walkSource.enabled = false;
            runSource.enabled = false;
        }

        float micVolume = GetVolumeFromMic() * SensitivityMultiplier;
        if (micVolume < Threshold) micVolume = 0;
        // Debug.Log($"Mic Volume: {micVolume}");

        if (micVolume > CurrentSoundRadius)
            CurrentSoundRadius = Mathf.Min(MaxRadius, micVolume);

        if (CurrentSoundRadius > 0f)
        {
            // Debug.Log($"Sound Radius: {CurrentSoundRadius}");
            Sounds.MakeSound(new Sound(transform.position, CurrentSoundRadius, SoundPriority));
        }
    }


}
