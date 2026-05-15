using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

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
    public GameWonScreen gameWonScreen;
    public TMP_Text RoundMessage;

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
    private SpawnAmmo spawnAmmo;

    void Awake()
    {
        Timer.TimerFinished += HandlePhaseEnd;
        MainMonster.HidingDamageEvent += HandleMonsterDamage;
        MainMonster.MonsterKilled += HandleMonsterKilled;

        fadingScript = GetComponent<FadingScript>();
        spawnAmmo = GetComponent<SpawnAmmo>();

        spawnAmmo.Spawn();

        fadingScript.FadeIn(1f);

        if (MainMonster.State == MonsterState.Hiding)
        {
            Debug.Log("Beginning hiding phase timer");
            CurrentPhase = Phase.Hiding;
            Timer.StartTimer(HidingDuration);
            StartCoroutine(UpdateRoundMessage());
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
        gameWonScreen.Setup();
    }

    private IEnumerator EndGame()
    {
        yield return new WaitForSeconds(2f);

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
            CurrentPhase = Phase.Seeking;
            MainMonster.LastSawPlayer = Time.time + TimeoutChaseDuration;
            MainMonster.EnterChase();
            
            Timer.StartTimer(SeekingDuration);

            StartCoroutine(BoostMonsterSpeed());
            StartCoroutine(UpdateWarningMessage());
        }
        else
        {
            Debug.Log("HandlePhase End (seeking) called");
            Round ++;

            spawnAmmo.Spawn();

            MainMonster.EnterHiding();

            fadingScript.FadeOut(1f);

            CurrentPhase = Phase.Hiding;
            Timer.StartTimer(HidingDuration);

            fadingScript.FadeIn(1f);

            StartCoroutine(UpdateRoundMessage());
        }
    }

    private IEnumerator UpdateWarningMessage()
    {
        RoundMessage.text = "RUN";
        RoundMessage.color = Color.red;
        RoundMessage.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        RoundMessage.gameObject.SetActive(false);
        RoundMessage.color = Color.white;
    }

    private IEnumerator UpdateRoundMessage()
    {
        yield return new WaitForSeconds(2f);
        RoundMessage.gameObject.SetActive(true);
        RoundMessage.text = $"Round {Round}";
        
        
        if (MonsterHealth <= 1)
        {
            yield return new WaitForSeconds(2f);
            RoundMessage.text = $"Kill Charlie";
        }
        else if (Round == 1)
        {
            yield return new WaitForSeconds(2f);
            RoundMessage.text = $"Call out to Charlie";
        }
        
        yield return new WaitForSeconds(2f);
        RoundMessage.gameObject.SetActive(false);
        
    }
}
