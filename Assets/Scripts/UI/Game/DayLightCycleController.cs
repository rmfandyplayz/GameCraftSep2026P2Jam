using rmf_claude.DOTweenUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// author: andy (rmfandyplayz)
public class DayLightCycleController : MonoBehaviour
{
    [SerializeField] UIAnimationPlayer animationPlayer;
    [SerializeField] TextMeshProUGUI dayText;

    int day = 1;
    bool isDay = true;

    public void AdvanceTime()
    {
        if (isDay == false)
        {
            day++;
            isDay = true;
        }
        else
            isDay = false;

        animationPlayer.PlayAnimation("TODO: CHANGE LATER");
        dayText.text = $"Day {day}";
    }
}
