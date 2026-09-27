using UnityEngine;

public class Soil : MonoBehaviour
{
    [Header("Seed Settings")]
    public GameObject seed;

    [Header("UI")]
    public GameObject interactPopup; //andy may replace this, this for now is the game object to indicate we can plant

    //not meant to be touched in inspector but i made this public for testing
    public bool canInteract = true;

    private Player player;

    private void OnEnable()
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.OnDayBegin += HandleDayBegin;
        GameManager.Instance.OnNightBegin += HandleNightBegin;
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.OnDayBegin -= HandleDayBegin;
        GameManager.Instance.OnNightBegin -= HandleNightBegin;
    }

    private void HandleDayBegin()
    {
        canInteract = true;
    }

    private void HandleNightBegin()
    {
        canInteract = false;
    }

    private void Update()
    {
        if (player == null)
            return;

        bool canPlant = canInteract;

        interactPopup.SetActive(canPlant); //if u can plant has UI pop up

        if (canPlant && player.InteractPressed)
        {
            Instantiate(seed, transform.position, Quaternion.identity);
            canInteract = false;
            interactPopup.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player enteringPlayer = other.GetComponent<Player>();

        if (enteringPlayer != null)
            player = enteringPlayer;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Player leavingPlayer = other.GetComponent<Player>();

        if (leavingPlayer == player)
        {
            interactPopup.SetActive(false); //disables ui popup
            player = null;
        }
    }
}
