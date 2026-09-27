using UnityEngine;

/// <summary>
/// Shows the Day 1 daytime/nighttime objects in order, then removes the Day 2
/// daytime object when the second night begins.
/// </summary>
public class DayOneObjectSequence : MonoBehaviour
{
    [Header("Sequence Objects")]
    [Tooltip("Shown at the beginning of Day 1.")]
    [SerializeField] GameObject day1daytime;
    [Tooltip("Shown during Night 1.")]
    [SerializeField] GameObject day1nighttime;
    [Tooltip("Shown during Day 2, then destroyed when Night 2 begins.")]
    [SerializeField] GameObject day2daytime;

    enum SequencePhase
    {
        Day1Daytime,
        Day1Nighttime,
        Day2Daytime,
        Complete
    }

    SequencePhase phase = SequencePhase.Day1Daytime;
    GameManager subscribedGameManager;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
        //SetActive(day1daytime, true);
        SetActive(day1nighttime, false);
        SetActive(day2daytime, false);
    }

    private void Update()
    {
        // Supports a persistent GameManager that becomes available after this object.
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void TrySubscribe()
    {
        if (subscribedGameManager != null || GameManager.Instance == null)
            return;

        subscribedGameManager = GameManager.Instance;
        subscribedGameManager.OnGameStart += () => day1daytime.SetActive(true);
        subscribedGameManager.OnNightBegin += HandleNightBegin;
        subscribedGameManager.OnDayBegin += HandleDayBegin;
    }

    private void Unsubscribe()
    {
        if (subscribedGameManager == null)
            return;

        subscribedGameManager.OnGameStart -= () => day1daytime.SetActive(true);
        subscribedGameManager.OnNightBegin -= HandleNightBegin;
        subscribedGameManager.OnDayBegin -= HandleDayBegin;
        subscribedGameManager = null;
    }

    private void HandleNightBegin()
    {
        if (phase == SequencePhase.Day1Daytime)
        {
            SetActive(day1daytime, false);
            SetActive(day1nighttime, true);
            phase = SequencePhase.Day1Nighttime;
            return;
        }

        if (phase == SequencePhase.Day2Daytime)
        {
            if (day2daytime != null)
                Destroy(day2daytime);

            phase = SequencePhase.Complete;
        }
    }

    private void HandleDayBegin()
    {
        if (phase != SequencePhase.Day1Nighttime)
            return;

        SetActive(day1nighttime, false);
        SetActive(day2daytime, true);
        phase = SequencePhase.Day2Daytime;
    }

    private static void SetActive(GameObject target, bool isActive)
    {
        if (target != null)
            target.SetActive(isActive);
    }
}
