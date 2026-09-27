using System;
using System.Collections.Generic;
using UnityEngine;

// author: andy (rmfandyplayz)
// controls multiple health icons
public class HealthController : MonoBehaviour
{
    private List<HealthIcon> healthIcons = new List<HealthIcon>();
    private int currentHP; // how many icons are currently full (icons [0, currentHP) are full)

    private void Awake()
    {
        healthIcons.AddRange(GetComponentsInChildren<HealthIcon>());
        currentHP = healthIcons.Count; // start full
    }

    /// <summary>
    /// Sets HP to newHP, filling in icons that were empty.
    /// </summary>
    public void Heal(int newHP)
    {
        SetHP(newHP);
    }

    /// <summary>
    /// Sets HP to newHP, emptying icons that were full.
    /// </summary>
    public void Hurt(int newHP)
    {
        SetHP(newHP);
    }
    
    private void SetHP(int newHP)
    {
        newHP = Mathf.Clamp(newHP, 0, healthIcons.Count);

        for (int i = currentHP; i < newHP; i++)
            healthIcons[i].Heal();

        for (int i = currentHP - 1; i >= newHP; i--) // empty from the end first
            healthIcons[i].Hurt();

        currentHP = newHP;
    }
}
