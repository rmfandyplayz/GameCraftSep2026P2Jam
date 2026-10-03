using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Action OnGameStart;
    public Action OnDayBegin;
    public Action OnDayEnd;
    public Action OnNightBegin;
    public Action OnNightEnd;
    public Action<int, int> OnUpdateQuota;
    public Action OnQuotaFailed;
    public Action OnSeedPlanted;
    public Action<int> OnEnemyDie;
    public Action<int> OnGainMoney;
    public Action<int> OnLoseMoney;


    public static GameManager Instance { get; private set; }
    public Player player { get; private set; }

    public int currentMoney { get; private set; } = 0;
    public int expectedProfit { get; private set; } = 0;
    public int quota { get; private set; } = 600;
    public int remainingDays { get; private set; } = 4;
    public int remainingEnemies { get; private set; } = 0;
    public bool nightTime { get; private set; } = false;

    [SerializeField, Tooltip("Quota required for each four-day cycle, in order. The final value repeats if all entries are used.")]
    private int[] quotaPerCycle = { 600, 1080, 2322, 6269 };

    [Tooltip("Seconds after a new day starts before the quota is collected - lets the night's profit finish adding to the total in the UI first.")]
    [SerializeField] private float quotaEnforceDelay = 1.5f;

    private int currentQuotaIndex;

    private void Awake()
    {
        /*
        if (Instance != null && Instance != this)
        {
            //Destroy(gameObject);
            return;
        }
        */
        Instance = this;
        //DontDestroyOnLoad(gameObject);

        player = FindFirstObjectByType<Player>();
    }

    private void OnEnable()
    {
        UI_API.RequestStart += HandleStartRequest;
        UI_API.RequestPause += HandlePauseRequest;
        UI_API.RequestResume += HandleResumeRequest;
        UI_API.RequestReturnToMenu += HandleReturnToMenuRequest;

        OnGameStart += StartGame;
        OnDayBegin += ResetNightTime;
        OnNightBegin += BeginNight;
        OnNightEnd += EndNight;
        OnSeedPlanted += IncrementEnemyCount;
        OnEnemyDie += EnemyDied;
        OnGainMoney += GainMoney;
        OnLoseMoney += LoseMoney;
        player.OnPlayerDie += HandlePlayerDeath;
    }

    private void HandleStartRequest() => OnGameStart?.Invoke();
    private void HandlePauseRequest() => Time.timeScale = 0f;
    private void HandleResumeRequest() => Time.timeScale = 1f;

    // The main menu lives in the game scene, so reloading it is a full reset back to the title screen.
    private void HandleReturnToMenuRequest()
    {
        Time.timeScale = 1f; // requested from the pause menu, and timeScale survives scene loads
        SceneManager.LoadScene(0);
    }

    private void OnDisable()
    {
        UI_API.RequestStart -= HandleStartRequest;
        UI_API.RequestPause -= HandlePauseRequest;
        UI_API.RequestResume -= HandleResumeRequest;
        UI_API.RequestReturnToMenu -= HandleReturnToMenuRequest;

        OnGameStart -= StartGame;
        OnDayBegin -= ResetNightTime;
        OnNightBegin -= BeginNight;
        OnNightEnd -= EndNight;
        OnSeedPlanted -= IncrementEnemyCount;
        OnEnemyDie -= EnemyDied;
        OnGainMoney -= GainMoney;
        OnLoseMoney -= LoseMoney;
        player.OnPlayerDie -= HandlePlayerDeath;
    }

    private void Start()
    {
        //OnGameStart?.Invoke();
    }

    private void HandlePlayerDeath()
    {
        expectedProfit = 0;
        nightEndQueued = false;
        OnNightEnd?.Invoke();
    }
     
    private void StartGame()
    {
        remainingDays = 4;
        currentMoney = 0;
        currentQuotaIndex = 0;
        quota = GetQuotaForCycle(currentQuotaIndex);
        nightTime = false;
        OnUpdateQuota?.Invoke(quota, remainingDays);
    }

    private void BeginNight()
    {
        nightTime = true;
    }

    private void EndNight()
    {
        nightEndQueued = false;
        OnGainMoney?.Invoke(expectedProfit);
        expectedProfit = 0;
        remainingEnemies = 0;
        OnDayBegin?.Invoke();
    }

    private bool nightEndQueued;

    private void EnemyDied(int value)
    {
        remainingEnemies -= 1;
        expectedProfit += value;

        if (remainingEnemies <= 0)
        {
            // Let every OnEnemyDie listener (including the UI) process the last
            // enemy before ending the night and committing the accumulated profit.
            if (!nightEndQueued)
            {
                nightEndQueued = true;
                StartCoroutine(EndNightAfterEnemyDeath());
            }
        }
    }

    private System.Collections.IEnumerator EndNightAfterEnemyDeath()
    {
        yield return new WaitForSeconds(1);

        if (!nightEndQueued)
            yield break;

        OnNightEnd?.Invoke();
    }

    private void GainMoney(int amount)
    {
        currentMoney += amount;
    }

    private void LoseMoney(int amount)
    {
        currentMoney -= amount;
    }

    private void ResetNightTime()
    {
        remainingDays -= 1;
        if (remainingDays <= 0)
            StartCoroutine(EnforceQuotaAfterDelay());
        OnUpdateQuota?.Invoke(quota, remainingDays);
        nightTime = false;
    }

    // Waits for the UI to finish adding the night's profit to the total before collecting the quota,
    // so the two point animations play one after the other instead of on top of each other.
    private System.Collections.IEnumerator EnforceQuotaAfterDelay()
    {
        yield return new WaitForSeconds(quotaEnforceDelay);

        if (currentMoney < quota)
        {
            OnQuotaFailed?.Invoke();
            SceneManager.LoadScene(0);
            Debug.Log("You lost!");
            yield break;
        }

        remainingDays = 4;
        OnLoseMoney?.Invoke(quota);

        currentQuotaIndex += 1;
        quota = GetQuotaForCycle(currentQuotaIndex);
        OnUpdateQuota?.Invoke(quota, remainingDays);
    }

    private int GetQuotaForCycle(int cycleIndex)
    {
        if (quotaPerCycle == null || quotaPerCycle.Length == 0)
        {
            Debug.LogWarning("No quota values are configured. Falling back to 600.", this);
            return 600;
        }

        int quotaIndex = Mathf.Min(cycleIndex, quotaPerCycle.Length - 1);
        return Mathf.Max(0, quotaPerCycle[quotaIndex]);
    }

    private void IncrementEnemyCount()
    {
        remainingEnemies += 1;
    }
}
