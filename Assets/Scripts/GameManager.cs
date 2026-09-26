using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Action OnGameStart;
    public Action OnDayBegin;
    public Action OnDayEnd;
    public Action OnNightBegin;
    public Action OnNightEnd;
    public Action OnPlayerDie;
    public Action<int> OnGainMoney;
    public Action<int> OnLoseMoney;

    public int currentMoney { get; private set; } = 0;
    public int quota { get; private set; } = 300;
    private float quotaIncrease = 1.1f;
    private float quotaIncreaseIncrease = 0.2f;
    public int remainingDays { get; private set; } = 3;

    public bool nightTime { get; private set; } = false;

    public static GameManager Instance { get; private set; }

    private void OnEnable()
    {
        OnNightBegin += BeginNight;
        OnGainMoney += (int moneyGained) => GainMoney(moneyGained);
        OnLoseMoney += (int moneyLost) => LoseMoney(moneyLost);
    }
    private void OnDisable()
    {
        OnNightBegin -= EndNight;
        OnGainMoney -= (int moneyGained) => GainMoney(moneyGained);
        OnLoseMoney -= (int moneyLost) => LoseMoney(moneyLost);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void GainMoney(int amount)
    {
        currentMoney += amount;
    }
    private void LoseMoney(int amount)
    {
        currentMoney += amount;
    }

    private void BeginNight()
    {
        nightTime = true;
        remainingDays -= 1;
    }
    private void EndNight()
    {
        if (remainingDays <= 0)
        {
            if (currentMoney < quota)
            {
                OnPlayerDie?.Invoke();
            }
            else
            {
                OnLoseMoney?.Invoke(quota);
                quota = (int)((float)quota * quotaIncrease);
                quotaIncrease += quotaIncreaseIncrease;
                quotaIncreaseIncrease += 0.2f;
            }
        }
        nightTime = false;
    }
}
