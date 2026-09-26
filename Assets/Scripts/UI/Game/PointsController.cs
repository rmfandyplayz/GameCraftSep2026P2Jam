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
            incomingPtsAnim.Play("CountNumberUp");
        }
        else if (newPts < oldPoints) // count down
        {
            incomingPtsCountingTxt.CountTo(newPts);
            incomingPtsAnim.Play("CountNumberDown");
        }
    }

    /// <summary>
    /// public API. combines the total 
    /// </summary>
    /// <param name="newTotal"></param>
    public void CombinePoints(int newTotal)
    {
        totalPtsCountingTxt.CountTo(newTotal);
        totalPtsAnim.Play("CountNumberUp");

        //TODO: CHANGE LATER
        incomingPtsText.text = string.Empty;
        incomingPtsCountingTxt.SetImmediate(0);

        oldPoints = 0;
    }
}
