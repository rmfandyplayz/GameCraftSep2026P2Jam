using UnityEngine;

public class Attack : MonoBehaviour
{
    [Header("Attack Lifetime")]
    public float lifetime = 0.3f;

    Transform playerTransform;
    Vector2 offsetFromPlayer;

    public void Initialize(Transform player)
    {
        playerTransform = player;
        offsetFromPlayer = (Vector2)transform.position - (Vector2)playerTransform.position;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void LateUpdate()
    {
        if (playerTransform != null)
        {
            transform.position = new Vector3(
                playerTransform.position.x + offsetFromPlayer.x,
                playerTransform.position.y + offsetFromPlayer.y,
                transform.position.z);
        }
    }
}
