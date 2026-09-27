using System;
using UnityEngine;

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

    [SerializeField] int quotaStart=600; // a public for me so i can change this and test, i didn't wanna mess with anything else lol - jackson

    private float quotaIncrease = 1.8f; // percent increase of quota
    [SerializeField] private float initialQuotaIncrease = 1.8f;
    private float quotaIncreaseIncrease = 0.35f; // amount the percent increases after quota is met
    [SerializeField] private float initialQuotaIncreaseInc = 0.35f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        player = FindFirstObjectByType<Player>();
    }

    private void Start()
    {
        OnGameStart?.Invoke();
    }

    private void OnEnable()
    {
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

    private void OnDisable()
    {
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
        quota = quotaStart;
        quotaIncrease = initialQuotaIncrease;
        quotaIncreaseIncrease = initialQuotaIncreaseInc;
        nightTime = false;
        Debug.Log($"{quota},{remainingDays}");
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
        yield return null;

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
        {
            if (currentMoney < quota)
            {
                OnQuotaFailed?.Invoke();
                Debug.Log("You lost!");
            }
            else
            {
                remainingDays = 4;
                OnLoseMoney?.Invoke(quota);

                quota = Mathf.RoundToInt(quota * quotaIncrease);
                quotaIncrease += quotaIncreaseIncrease;
                quotaIncreaseIncrease += 0.2f;
            }
        }
        OnUpdateQuota?.Invoke(quota, remainingDays);
        nightTime = false;
    }

    private void IncrementEnemyCount()
    {
        remainingEnemies += 1;
    }
}
