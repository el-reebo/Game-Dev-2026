using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class VoiceDetection : MonoBehaviour
{
    [Header("Monster SFX")]
    [SerializeField] private AudioClip monsterRoar;
    [SerializeField] private float voiceCallDelay = 1.0f;
    
    private Animator m_Animator;
    private AudioSource monsterAudio;
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Action> monsterActions = new Dictionary<string, Action>();

    private float lastVoiceCall;

    void Start()
    {
        monsterAudio = GetComponent<AudioSource>();

        // Check mic detection
        foreach (var device in Microphone.devices)
        {
            Debug.Log("Mic detected: " + device);
        }

        m_Animator = GetComponent<Animator>();

        // Recognised words
        monsterActions.Add("Stay", Stay);
        monsterActions.Add("Charlie", Charlie);

        // Initialise keyword recogniser
        keywordRecognizer = new KeywordRecognizer(monsterActions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += RecognizedPhrase;
        keywordRecognizer.Start();

        Debug.Log("Keyword Recognizer status: "+keywordRecognizer.IsRunning);
    }

    void RecognizedPhrase(PhraseRecognizedEventArgs phrase)
    {
        Debug.Log(phrase.text);
        monsterActions[phrase.text].Invoke();
    }

    // --- Voice activation functions ---
    void Stay()
    {
        Debug.Log("Stay function played");
    }

    void Charlie()
    {
        if (Time.time > lastVoiceCall + voiceCallDelay)
        {
            monsterAudio.PlayOneShot(monsterRoar, 1.0f);
            lastVoiceCall = Time.time;
            Debug.Log("Charlie function played");
        }
    }
}
