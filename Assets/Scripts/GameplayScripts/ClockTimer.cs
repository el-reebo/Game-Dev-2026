using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

// Code generated using Copilot "make a function for a clock timer
// in unity that takes in the timer duration in minutes"
public class ClockTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text Timer;

    public event System.Action TimerFinished;
    
    private Coroutine TimerRoutine;
    
    void Awake()
    {
        Timer = GetComponent<TMP_Text>();
    }

    public void StartTimer(float minutes)
    {
        if (TimerRoutine != null)
            StopCoroutine(TimerRoutine);

        TimerRoutine = StartCoroutine(TimerCoroutine(minutes));
    }

    private IEnumerator TimerCoroutine(float minutes)
    {
        float remaining = minutes * 60f;

        while (remaining > 0)
        {
            remaining -= Time.deltaTime;

            int mins = Mathf.FloorToInt(remaining / 60f);
            int secs = Mathf.FloorToInt(remaining % 60f);

            Timer.text = $"{mins:00}:{secs:00}";

            yield return null;
        }

        Timer.text = "00:00";
        TimerFinished?.Invoke(); // calls event if there are functions subscribed
    }
}
