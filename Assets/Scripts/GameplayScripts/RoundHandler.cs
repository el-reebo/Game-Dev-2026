using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum Phase
{
    Hiding,
    Seeking,
}

public class RoundHandler : MonoBehaviour
{
    [Header("References")]
    public MonsterAIController MainMonster;
    public ClockTimer Timer;

    [Header("Game Settings")]
    public int MonsterHealth = 3;
    public float HidingDuration = 3f;
    public float SeekingDuration = 3f;
    public float TimeoutChaseDuration = 10f; // chase duration when hiding timer runs out
    public float BoostSpeedMultiplier = 2f;

    [Header("Public Variables")]
    public int Round = 1; // number of pairs of Hiding + Seeking phases

    public Phase CurrentPhase = Phase.Hiding;

    private FadingScript fadingScript;

    void Awake()
    {
        Timer.TimerFinished += HandlePhaseEnd;
        MainMonster.HidingDamageEvent += HandleMonsterDamage;
        MainMonster.MonsterKilled += HandleMonsterKilled;

        fadingScript = GetComponent<FadingScript>();

        fadingScript.FadeIn(1f);

        if (MainMonster.State == MonsterState.Hiding)
        {
            Debug.Log("Beginning hiding phase timer");
            CurrentPhase = Phase.Hiding;
            Timer.StartTimer(HidingDuration);
        }
        else
        {
            Debug.Log("Beginning seeking phase timer");
            CurrentPhase = Phase.Seeking;
            Timer.StartTimer(SeekingDuration);
        }
    }

    private IEnumerator BoostMonsterSpeed()
    {
        float OriginalSpeed = MainMonster.MonsterSpeed;
        MainMonster.MonsterSpeed = OriginalSpeed * BoostSpeedMultiplier;

        Debug.Log("Boosting");
        yield return new WaitForSeconds(TimeoutChaseDuration);
        Debug.Log("Done boosting");
        MainMonster.MonsterSpeed = OriginalSpeed;
    }

    private void HandleMonsterKilled()
    {
        Debug.Log("Yay you won");
    }

    private void HandleMonsterDamage()
    {
        Debug.Log($"HandleMonsterDamage called {MonsterHealth}");
        MonsterHealth --;
        CurrentPhase = Phase.Seeking;

        if (MonsterHealth <= 1)
        {
            MainMonster.Killable = true;
        }

        Timer.StartTimer(SeekingDuration);
    }

    private void HandlePhaseEnd()
    {
        // Case: hiding phase timer runs out
        if (CurrentPhase == Phase.Hiding)
        {
            Debug.Log("HandlePhaseEnd (hiding) called");
            CurrentPhase = Phase.Seeking;
            MainMonster.LastSawPlayer = Time.time + TimeoutChaseDuration;
            MainMonster.EnterChase();
            
            Timer.StartTimer(SeekingDuration);

            StartCoroutine(BoostMonsterSpeed());
        }
        else
        {
            Debug.Log("HandlePhase End (seeking) called");
            Round ++;

            MainMonster.EnterHiding();

            fadingScript.FadeOut(1f);

            CurrentPhase = Phase.Hiding;
            Timer.StartTimer(HidingDuration);

            fadingScript.FadeIn(1f);
        }
    }

    // if monster hurt during hiding phase, start seeking phase
}
