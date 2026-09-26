using UnityEngine;

public class Soil : MonoBehaviour
{
    [Header("Seed Settings")]
    public GameObject seed;

    //not meant to be touched in inspector but i made this public for testing
    public bool canInteract = true;

    private Player player;

    private void Update()
    {
        if (player == null)
            return;

        bool canPlant = canInteract;

        player.interactPopup.SetActive(canPlant); //if u can plant has UI pop up

        if (canPlant && player.InteractPressed)
        {
            Instantiate(seed, transform.position, Quaternion.identity);
            canInteract = false;
            player.interactPopup.SetActive(false);
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
            player.interactPopup.SetActive(false); //disables ui popup
            player = null;
        }
    }
}
