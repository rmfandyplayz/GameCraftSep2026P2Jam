using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 5f;
    [SerializeField] InputActionReference moveInput;
    [SerializeField] InputActionReference interactInput;

    [Header("Night Attack")]
    public GameObject attack;
    public Transform upAttackDirection;
    public Transform rightAttackDirection;
    public Transform leftAttackDirection;
    public Transform downAttackDirection;
    public float attackCooldown = 0.5f;

    [Header("Health")]
    public int HP = 3;
    public float invincibilityTime = 0.2f;

    [Header("I Frame Visual Effect")]
    [Range(0f, 1f)] public float damageFlashAlpha = 0.35f;
    public float flashInterval = 0.04f;

    [Header("Death")]
    public string deathSceneName = "deathScene";
    public float deathSceneDelay = 1f;

    // Soil can only consume the interact input during the day.
    [HideInInspector]
    public bool InteractPressed => !GameManager.Instance.nightTime && interactInput.action.WasPressedThisFrame();

    Rigidbody2D rb;
    SpriteRenderer playerSprite;
    Vector2 lastMoveDirection = Vector2.down;
    float nextAttackTime;
    bool isInvincible;
    bool isDead;

    private Animator playerAnimator;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerSprite = GetComponentInChildren<SpriteRenderer>();
        playerAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!isDead && GameManager.Instance.nightTime && interactInput.action.WasPressedThisFrame())
        {
            if (attack == null || Time.time < nextAttackTime) return;
            nextAttackTime = Time.time + attackCooldown;
            playerAnimator.SetTrigger("Attack");
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
            return;

        Vector2 direction = moveInput.action.ReadValue<Vector2>();

        if (direction.sqrMagnitude > 0.01f)
        {
            if (direction.x > 0)
            {
                playerAnimator.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else
            {
                playerAnimator.gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            lastMoveDirection = direction;
        }

        rb.MovePosition(rb.position + (direction.normalized * speed * Time.deltaTime));
    }

    private void TryAttack()
    {
        Transform spawnPoint;
        Quaternion rotation;

        if (Mathf.Abs(lastMoveDirection.x) > Mathf.Abs(lastMoveDirection.y))
        {
            if (lastMoveDirection.x > 0f)
            {
                spawnPoint = rightAttackDirection;
                rotation = Quaternion.identity;
            }
            else
            {
                spawnPoint = leftAttackDirection;
                rotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }
        else if (lastMoveDirection.y > 0f)
        {
            spawnPoint = upAttackDirection;
            rotation = Quaternion.Euler(0f, 0f, 90f);
        }
        else
        {
            spawnPoint = downAttackDirection;
            rotation = Quaternion.Euler(-180f, 0f, 90f);
        }

        if (spawnPoint == null)
            return;

        GameObject spawnedAttack = Instantiate(attack, spawnPoint.position, rotation);
        
        //makes sure the attack hitbox follows our player
        Attack attackBehaviour = spawnedAttack.GetComponent<Attack>();

        if (attackBehaviour != null)
            attackBehaviour.Initialize(transform);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TakeDamage(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TakeDamage(other);
    }

    private void TakeDamage(Collider2D other)
    {
        if (isDead || isInvincible || !other.CompareTag("Enemy"))
            return;

        HP -= 1;
        StartCoroutine(DamageFlash());

        if (HP <= 0)
        {
            isDead = true;
            GameManager.Instance.OnPlayerDie?.Invoke();
            StartCoroutine(DeathSequence());
        }
    }

    //vibe coded this bc the i frame math is a pain
    private IEnumerator DamageFlash()
    {
        isInvincible = true;

        if (playerSprite == null)
        {
            yield return new WaitForSecondsRealtime(invincibilityTime);
        }
        else
        {
            Color originalColor = playerSprite.color;
            Color transparentColor = originalColor;
            transparentColor.a = damageFlashAlpha;

            float elapsedTime = 0f;
            float pulseTime = Mathf.Max(0.01f, flashInterval);
            bool showTransparent = true;

            while (elapsedTime < invincibilityTime)
            {
                playerSprite.color = showTransparent ? transparentColor : originalColor;
                showTransparent = !showTransparent;

                float waitTime = Mathf.Min(pulseTime, invincibilityTime - elapsedTime);
                yield return new WaitForSecondsRealtime(waitTime);
                elapsedTime += waitTime;
            }

            playerSprite.color = originalColor;
        }

        isInvincible = false;
    }

    private IEnumerator DeathSequence()
    {
        GameObject panel = null;

        panel = GameObject.FindGameObjectWithTag("panel");

        if (panel != null)
        {
            Animator panelAnimator = panel.GetComponent<Animator>();

            if (panelAnimator != null)
                panelAnimator.SetTrigger("end");
        }

        //so the scene transition plays fully before scene load
        yield return new WaitForSecondsRealtime(deathSceneDelay);

        SceneManager.LoadScene(deathSceneName);
    }
}
