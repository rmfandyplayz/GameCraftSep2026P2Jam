using rmf_claude.DOTweenUI;
using TMPro;
using UnityEngine;


public class PointsController : MonoBehaviour
{
    [SerializeField] UIAnimationPlayer animationPlayer;
    [SerializeField] TextMeshProUGUI totalPointsText;
    [SerializeField] TextMeshProUGUI incomingPointsText;

    int oldPoints; // determine if increase or decrease

    private void Start()
    {
        totalPointsText.text = $"P: 0";
        incomingPointsText.text = "";
    }

    public void SetTempPoints(int newPts)
    {
        if(newPts > oldPoints)
        {
            //todo: animation
            incomingPointsText.text = $"(+{newPts})";
        }
        else if (newPts < oldPoints)
        {
            //todo: animation
            incomingPointsText.text = $"(+{newPts})";
        }
    }

    public void CombinePoints(int newPoints)
    {
        //todo: animation
        totalPointsText.text = $"P: {newPoints}";
        oldPoints = 0;
    }
}
