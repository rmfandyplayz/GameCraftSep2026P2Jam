using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Action OnGameStart;
    public Action OnDayBegin;
    public Action OnDayEnd;
    public Action OnNightBegin;
    public Action OnNightEnd;
    public Action<int> OnNewQuota;
    public Action OnQuotaFailed;
    public Action OnSeedPlanted;
    public Action<int> OnEnemyDie;
    public Action<int> OnGainMoney;
    public Action<int> OnLoseMoney;


    public static GameManager Instance { get; private set; }
    public Player player { get; private set; }

    public int currentMoney { get; private set; } = 0;
    public int expectedProfit { get; private set; } = 0;
    public int quota { get; private set; } = 300;
    public int remainingDays { get; private set; } = 5;
    public int remainingEnemies { get; private set; } = 0;
    public bool nightTime { get; private set; } = false;

    private float quotaIncrease = 1.1f;
    private float quotaIncreaseIncrease = 0.2f;

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
    }


    private void StartGame()
    {
        remainingDays = 5;
        currentMoney = 0;
        quota = 300;
        quotaIncrease = 1.1f;
        quotaIncreaseIncrease = 0.2f;
        nightTime = false;

        OnNewQuota?.Invoke(quota);
    }

    private void BeginNight()
    {
        nightTime = true;
        remainingDays -= 1;
    }

    private void EndNight()
    {
        OnGainMoney?.Invoke(expectedProfit);
        expectedProfit = 0;
        remainingEnemies = 0;

        if (remainingDays <= 0)
        {
            if (currentMoney < quota)
            {
                OnQuotaFailed?.Invoke();
                return;
            }

            remainingDays = 5;
            OnLoseMoney?.Invoke(quota);

            quota = Mathf.RoundToInt(quota * quotaIncrease);
            quotaIncrease += quotaIncreaseIncrease;
            quotaIncreaseIncrease += 0.2f;

            OnNewQuota?.Invoke(quota);
        }

        OnDayBegin?.Invoke();
    }

    private void EnemyDied(int value)
    {
        remainingEnemies -= 1;
        expectedProfit += value;

        if (remainingEnemies <= 0)
        {
            OnNightEnd?.Invoke();
        }
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
        nightTime = false;
    }

    private void IncrementEnemyCount()
    {
        remainingEnemies += 1;
    }
}