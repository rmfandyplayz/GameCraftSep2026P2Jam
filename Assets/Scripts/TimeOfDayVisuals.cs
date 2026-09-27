using UnityEngine;

public class TimeOfDayVisuals : MonoBehaviour
{
   public GameObject DaytimeVisuals;
   public GameObject NightVisuals;

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.nightTime == false){
            NightVisuals.SetActive(false);
            DaytimeVisuals.SetActive(true);
        } else {
            DaytimeVisuals.SetActive(false);
            NightVisuals.SetActive(true);
        }
    }
}
