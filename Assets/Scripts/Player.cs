using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 5f;
    [SerializeField] InputActionReference moveInput;
    [SerializeField] InputActionReference interactInput;

    [Header("UI")]
    [SerializeField] GameObject interactPopup; //andy may replace this, this for now is the game object to indicate we can plant

    //checks if we pressed E (to be used for soil)
    [HideInInspector]
    public bool InteractPressed => interactInput.action.WasPressedThisFrame();

    Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 direction = moveInput.action.ReadValue<Vector2>();
        rb.MovePosition(rb.position + (direction.normalized * speed * Time.deltaTime));
    }

    public void ToggleInteractPopup(bool show)
    {
        if (interactPopup != null)
        {
            interactPopup.SetActive(show);
        }
    }
}
