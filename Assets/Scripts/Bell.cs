using UnityEngine;
using TMPro;

public class Bell : MonoBehaviour
{
    private bool canInteract;
    public GameObject interactUIPopup;

    public int requiredSeeds;
    private int seedsLeft;

    public GameObject seedsLeftDisplay;
    public TMP_Text requiredSeedsText;

    private Player player;

    private bool hasSeeds;

    private void OnEnable()
    {
        GameManager.Instance.OnDayBegin += () =>
        {
            hasSeeds = false;
            requiredSeeds += 1;
            seedsLeftDisplay.SetActive(true);
        };
    }
    
    void Update(){
        seedsLeft = requiredSeeds - GameObject.FindGameObjectsWithTag("seeds").Length;

        requiredSeedsText.text = seedsLeft.ToString();
        

        hasSeeds = GameObject.FindGameObjectsWithTag("seeds").Length >= requiredSeeds;
        if (Input.GetKeyDown(KeyCode.E) && canInteract == true && hasSeeds == true) 
            Interact();

        if (hasSeeds == true){
            seedsLeftDisplay.SetActive(false);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = true;
            
            if (hasSeeds == true){
            interactUIPopup.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            interactUIPopup.SetActive(false);
        }
    }

    public void Interact(){
        if (GameManager.Instance.nightTime == false){
            GameManager.Instance.OnDayEnd?.Invoke();
            GameManager.Instance.OnNightBegin?.Invoke();        
        }
    }
}
