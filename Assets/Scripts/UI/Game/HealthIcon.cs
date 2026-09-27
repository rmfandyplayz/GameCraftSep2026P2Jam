using System;
using rmf_claude.DOTweenUI;
using UnityEngine;
using UnityEngine.UI;

public class HealthIcon : MonoBehaviour
{
    [SerializeField] private UIAnimationPlayer animPlayer;

    /// <summary>
    /// Overlays the full sprite on top of the empty sprite (plays animation)
    /// </summary>
    public void Heal()
    {
        animPlayer.Play("Heal");
    }

    /// <summary>
    /// Does the opposite of <see cref="Heal"/>
    /// </summary>
    public void Hurt()
    {
        animPlayer.Play("Hurt");
    }
}
