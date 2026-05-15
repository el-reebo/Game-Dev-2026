using UnityEngine;
using System.Collections.Generic;

public class MonsterSound : MonoBehaviour
{
    [Header("Footsteps")]
    public List<AudioClip> FS;

    [SerializeField] private AudioSource source;

    void PlayFootstep()
    {
        AudioClip clip;

        clip = FS[Random.Range(0, FS.Count)];

        source.clip = clip;
        //source.volume = Random.Range(0.02f, 0.05f);
        source.pitch = Random.Range(0.8f, 1.2f);
        source.PlayOneShot(clip);
    }
}
