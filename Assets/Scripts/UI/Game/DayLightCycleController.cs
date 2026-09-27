using rmf_claude.DOTweenUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// author: andy (rmfandyplayz)
public class DayLightCycleController : MonoBehaviour
{
    [SerializeField] UIAnimationPlayer rotatingCircAnimPlayer;
    [SerializeField] UIAnimationPlayer dayTextAnimPlayer;
    [SerializeField] TextMeshProUGUI dayText;

    int day = 1;
    bool isDay = true;

    /// <summary>
    /// returns true if this call will turn it into day
    /// </summary>
    /// <returns></returns>
    public bool AdvanceTime()
    {
        if (isDay == false)
        {
            day++;
            isDay = true;

            dayTextAnimPlayer.Play("Disappear", () =>
            {
                dayText.text = $"Day {day}";
                dayTextAnimPlayer.Play("Appear");
            });


            rotatingCircAnimPlayer.Stop("RotateCircle", true);
            rotatingCircAnimPlayer.PlayAnimation("RotateCircle");

            return true;
        }
        else
        {
            isDay = false;
            rotatingCircAnimPlayer.Stop("RotateCircle", true);
            rotatingCircAnimPlayer.PlayAnimation("RotateCircle");

            return false;
        }
    }
}
