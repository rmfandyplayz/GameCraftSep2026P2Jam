using System;
using UnityEngine;
using UnityEngine.Events;

// author: andy (rmfandyplayz)
// adds controls to game logic to control UI behavior
public class UI_API : MonoBehaviour
{
    public static event Action RequestStart; // request the game to start
    public static event Action RequestPause; // request the game to pause
    public static event Action RequestResume;



    public void ResumeGame()
    {
        RequestResume.Invoke();
    }

    public void PauseGame()
    {
        RequestPause.Invoke();
    }
}
