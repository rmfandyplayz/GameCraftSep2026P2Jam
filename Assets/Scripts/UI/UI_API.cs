using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

// author: andy (rmfandyplayz)
// adds controls to game logic to control UI behavior
public class UI_API : MonoBehaviour
{
    public static event Action RequestStart; // request the game to start
    public static event Action RequestPause; // request the game to pause
    public static event Action RequestResume;

    [Header("references")]
    [InspectorLabel("Daylight Cycle Manager"), SerializeField] DayLightCycleController daylightCycleController;
    [InspectorLabel("Points Manager"), SerializeField] PointsController pointsController;
    [InspectorLabel("Quota Manager"), SerializeField] QuotaController quotaController;
    [InspectorLabel("Health Manager"), SerializeField] HealthController healthController;

    int tempPoints; // money gained/lost since the last CombinePoints
    bool subscribed;

    // ===============================================================================================================================
    //                                                  GAMEMANAGER HOOKS
    // ===============================================================================================================================

    private void OnEnable()
    {
        SubscribeToGameManager();
    }

    private void Start()
    {
        SubscribeToGameManager();
    }

    private void OnDisable()
    {
        if (!subscribed || GameManager.Instance == null)
            return;

        GameManager gm = GameManager.Instance;
        gm.OnDayBegin -= AdvanceTime;
        gm.OnNightBegin -= AdvanceTime;
        gm.OnNightEnd -= HandleNightEnd;
        gm.OnEnemyDie -= HandleGainMoney;
        gm.OnUpdateQuota -= HandleNewQuota;
        gm.player.OnPlayerHealthHeal -= HandleOnGainHP;
        gm.player.OnPlayerHealthDamage -= HandleOnLoseHP;
        subscribed = false;
    }

    private void SubscribeToGameManager()
    {
        if (subscribed || GameManager.Instance == null)
            return;

        GameManager gm = GameManager.Instance;
        gm.OnDayBegin += AdvanceTime;
        gm.OnNightBegin += AdvanceTime;
        gm.OnNightEnd += HandleNightEnd;
        gm.OnEnemyDie += HandleGainMoney;
        gm.OnUpdateQuota += HandleNewQuota;
        gm.player.OnPlayerHealthHeal += HandleOnGainHP;
        gm.player.OnPlayerHealthDamage += HandleOnLoseHP;
        subscribed = true;
    }

    private void HandleGainMoney(int amount)
    {
        tempPoints += amount;
        SetTempPoints(tempPoints);
    }

    private void HandleNightEnd()
    {
        // Animate the expected amount back to zero as it is committed to the total.
        SetTempPoints(0);
        tempPoints = 0;
        CombinePoints(GameManager.Instance.currentMoney);
    }

    private void HandleNewQuota(int reqPoints, int daysLeft)
    {
        SetQuota(reqPoints, daysLeft);
    }

    private void HandleOnGainHP(int newHP)
    {
        
        healthController.Heal(newHP);
    }
    
    private void HandleOnLoseHP(int newHP)
    {
        Debug.Log($"hp lose called | new HP: {newHP}");
        healthController.Hurt(newHP);
    }

    // ===============================================================================================================================
    //                                             PUBLIC API
    // ===============================================================================================================================

    /// <summary>
    /// Advances the time. Defaults to daytime, day 1.
    /// Advancing the time will set it to nightttime, day 1. Running this again
    /// will set it back to daytime, day 2.
    /// </summary>
    public void AdvanceTime()
    {
        SetTempPoints(0);
        daylightCycleController.AdvanceTime();
    }

    /// <summary>
    /// Sets the amount of temporary points. Animation will respond
    /// correctly according to if it's added or subtracted. </br>
    /// 
    /// Call <see cref="CombinePoints"/> if you're looking to add
    /// temp points to permanent point count.
    /// </summary>
    public void SetTempPoints(int pts)
    {
        pointsController.SetTempPoints(pts);
    }

    /// <summary>
    /// Combines temp points with permanent points.
    /// </summary>
    public void CombinePoints(int newTotal)
    {
        pointsController.CombinePoints(newTotal);
    }

    /// <summary>
    /// Sets the quota line ("Req: quota in daysLeft days"). Only the numbers that changed animate.
    /// </summary>
    public void SetQuota(int quota, int daysLeft)
    {
        quotaController.SetQuota(quota, daysLeft);
    }
    
    public void FailQuota()
    {
        quotaController.FailQuota();
    }

    // ===============================================================================================================================
    //                                             PRIVATE API
    // ===============================================================================================================================

    public void StartGame()
    {
        RequestStart.Invoke();
    }

    public void ResumeGame()
    {
        RequestResume.Invoke();
    }

    public void PauseGame()
    {
        RequestPause.Invoke();
    }
}
