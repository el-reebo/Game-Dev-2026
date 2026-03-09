using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class VoiceDetection : MonoBehaviour
{
    private Animator m_Animator;

    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Action> monsterActions = new Dictionary<string, Action>();

    void Start()
    {
        // Check mic detection
        foreach (var device in Microphone.devices)
        {
            Debug.Log("Mic detected: " + device);
        }

        m_Animator = GetComponent<Animator>();

        monsterActions.Add("Stay", Stay);

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

    void Stay()
    {
        Debug.Log("Stay function played");
    }
}
