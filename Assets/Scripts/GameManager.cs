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

    public int currentMoney { get; private set; } = 0;
    public int quota { get; private set; } = 300;
    public int remainingDays { get; private set; } = 3;
    public int dayNumber { get; private set; } = 0;

    public static GameManager Instance { get; private set; }

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
}
