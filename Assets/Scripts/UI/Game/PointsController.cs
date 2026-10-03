using rmf_claude.DOTweenUI;
using TMPro;
using UnityEngine;

// author: andy (@rmfandyplayz)
public class PointsController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI totalPtsText;
    [SerializeField] TextMeshProUGUI incomingPtsText;

    [SerializeField] CountingText totalPtsCountingTxt;
    [SerializeField] CountingText incomingPtsCountingTxt;
    [SerializeField] UIAnimationPlayer incomingPtsAnim;
    [SerializeField] UIAnimationPlayer totalPtsAnim;

    int oldPoints; // determine if increase or decrease


    private void Awake()
    {
        totalPtsCountingTxt.SetImmediate(0);
        incomingPtsCountingTxt.SetImmediate(0);
        incomingPtsText.text = string.Empty;
    }

    /// <summary>
    /// public API. sets the temporary point text
    /// </summary>
    public void SetTempPoints(int newPts)
    {
        if(newPts > oldPoints) // count up
        {
            incomingPtsCountingTxt.CountTo(newPts);
            incomingPtsAnim.Stop("CountNumberUp", true);
            incomingPtsAnim.Play("CountNumberUp");
        }
        else if (newPts < oldPoints) // count down
        {
            incomingPtsCountingTxt.CountTo(newPts);
            incomingPtsAnim.Stop("CountNumberDown", true);
            incomingPtsAnim.Play("CountNumberDown");
        }

        oldPoints = newPts;
    }

    /// <summary>
    /// public API. combines the temp points with total points
    /// </summary>
    public void CombinePoints(int newTotal)
    {
        incomingPtsCountingTxt.CountTo(0);
        incomingPtsAnim.Stop("CombineAnim", true);
        incomingPtsAnim.Play("CombineAnim", () =>
        {
            incomingPtsText.text = string.Empty;
            incomingPtsCountingTxt.SetImmediate(0);
        });

        // nothing was earned (e.g. the player died), so don't count up or play the money sound
        if (newTotal != totalPtsCountingTxt.To)
        {
            totalPtsCountingTxt.CountTo(newTotal);
            totalPtsAnim.Stop("CountNumberUp", true);
            totalPtsAnim.Play("CountNumberUp");
        }

        oldPoints = 0;
    }

    /// <summary>
    /// public API. counts the total points to a new value without touching the temp points (e.g. paying the quota)
    /// </summary>
    public void SetTotalPoints(int newTotal)
    {
        string anim = newTotal < totalPtsCountingTxt.To ? "CountNumberDown" : "CountNumberUp";
        totalPtsCountingTxt.CountTo(newTotal);
        totalPtsAnim.Stop(anim, true);
        totalPtsAnim.Play(anim);
    }
}
