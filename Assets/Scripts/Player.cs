using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public InputActionAsset inputSystem;
    private InputAction interact;

    [Header("UI")]
    public GameObject interactPopup; //andy may replace this, this for now is the game object to indicate we can plant

    //checks if we pressed E (to be used for soil)
    [HideInInspector]
    public bool InteractPressed => interact.WasPressedThisFrame();

    private void Start()
    {
        interact = inputSystem.FindAction("Player/Interact");
        interact.Enable();
    }

    private void Update()
    {
        //basic ass movement
        Vector2 direction = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) direction.y = 1;
        if (Keyboard.current.sKey.isPressed) direction.y = -1;
        if (Keyboard.current.aKey.isPressed) direction.x = -1;
        if (Keyboard.current.dKey.isPressed) direction.x = 1;

        transform.Translate(direction.normalized * speed * Time.deltaTime);
    }
}
