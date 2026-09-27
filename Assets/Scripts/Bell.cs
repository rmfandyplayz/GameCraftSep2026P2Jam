using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Bell : MonoBehaviour
{
    [SerializeField] InputActionReference interactInput;
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
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStart += ()=> seedsLeftDisplay.SetActive(true);
            GameManager.Instance.OnDayBegin += HandleDayBegin;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStart -= () => seedsLeftDisplay.SetActive(true);
            GameManager.Instance.OnDayBegin -= HandleDayBegin;
    }

    private void HandleDayBegin()
    {
        hasSeeds = false;
        requiredSeeds += 1;
        seedsLeftDisplay.SetActive(true);
    }
    
    void Update(){
        seedsLeft = requiredSeeds - GameObject.FindGameObjectsWithTag("seeds").Length;

        requiredSeedsText.text = seedsLeft.ToString();
        

        hasSeeds = GameObject.FindGameObjectsWithTag("seeds").Length >= requiredSeeds;
        if (interactInput.action.WasPressedThisFrame() && canInteract == true && hasSeeds == true) 
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
            interactUIPopup.SetActive(false);
            
            GameManager.Instance.OnDayEnd?.Invoke();
            GameManager.Instance.OnNightBegin?.Invoke();        
        }
    }
}
