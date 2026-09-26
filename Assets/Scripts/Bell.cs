using UnityEngine;

public class Bell : MonoBehaviour
{
    private bool canInteract;
    public GameObject interactUIPopup;

    private Player player;

    void Update(){
    if (Input.GetKeyDown(KeyCode.E) && canInteract == true)
        Interact();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = true;
            interactUIPopup.SetActive(true);
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
