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

    int tempPoints; // running total of money gained/lost since the last CombinePoints
    bool subscribed;

    // ===============================================================================================================================
    //                                             GAMEMANAGER HOOKS
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
        gm.OnGainMoney -= HandleGainMoney;
        gm.OnLoseMoney -= HandleLoseMoney;
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
        gm.OnGainMoney += HandleGainMoney;
        gm.OnLoseMoney += HandleLoseMoney;
        subscribed = true;
    }

    private void HandleGainMoney(int amount)
    {
        tempPoints += amount;
        SetTempPoints(tempPoints);
    }

    private void HandleLoseMoney(int amount)
    {
        tempPoints -= amount;
        SetTempPoints(tempPoints);
    }

    private void HandleNightEnd()
    {
        CombinePoints(GameManager.Instance.currentMoney);
        tempPoints = 0;
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

    // ===============================================================================================================================
    //                                             PRIVATE API
    // ===============================================================================================================================

    public static void StartGame()
    {
        RequestStart.Invoke();
    }

    public static void ResumeGame()
    {
        RequestResume.Invoke();
    }

    public static void PauseGame()
    {
        RequestPause.Invoke();
    }
}
