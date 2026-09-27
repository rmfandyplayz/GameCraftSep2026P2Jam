using rmf_claude.DOTweenUI;
using TMPro;
using UnityEngine;

// ai-assisted. author: andy (@rmfandyplayz) and claude (anthropic) opus 5.5
public class QuotaController : MonoBehaviour
{
    [Header("quota amount")]
    [SerializeField] CountingText quotaCountingTxt;
    [SerializeField] UIAnimationPlayer quotaAnim;

    [Header("days left")]
    [SerializeField] CountingText daysCountingTxt;
    [SerializeField] UIAnimationPlayer daysAnim;
    [SerializeField] TextMeshProUGUI daysLabelText; // "day" / "days"

    [Header("animations")]
    [Tooltip("Played on a number's UIAnimationPlayer when it changes. Needs a CountingText.Progress 0 -> 1 step to count.")]
    [SerializeField] string countUpAnimName = "CountNumberUp";
    [SerializeField] string countDownAnimName = "CountNumberDown";

    bool shown; // false while the texts still show the "???" placeholder
    int tempDays; // keep track


    
    public void FailQuota()
    {

    }

    /// <summary>
    /// public API. counts each number that changed to its new value.
    /// </summary>
    public void SetQuota(int quota, int daysLeft)
    {
        CountTo(quotaCountingTxt, quotaAnim, quota, countUpAnimName);
        CountTo(daysCountingTxt, daysAnim, daysLeft, (daysLeft > tempDays ? countUpAnimName : countDownAnimName));

        tempDays = daysLeft;

        daysLabelText.text = daysLeft == 1 ? "day" : "days";
        shown = true;
    }

    void CountTo(CountingText counter, UIAnimationPlayer anim, int value, string animationName)
    {
        if (shown && value == counter.To)
            return;

        anim.Stop(animationName);
        counter.CountTo(value);
        anim.Play(animationName);
    }
}
