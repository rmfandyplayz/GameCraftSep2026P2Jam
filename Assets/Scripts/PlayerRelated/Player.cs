using System;
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
    public Transform attackDirection;
    public float attackCooldown = 0.5f;

    [Header("Health")]
    public int maxHealth { get; private set; } = 3;
    public int hp { get; private set; } = 3;
    public float invincibilityTime = 0.2f;

    [Header("I Frame Visual Effect")]
    [Range(0f, 1f)] public float damageFlashAlpha = 0.35f;
    public float flashInterval = 0.04f;

    [Header("Death")]
    public string deathSceneName = "deathScene";
    public float deathSceneDelay = 1f;

    public Action OnPlayerDie;
    public Action<int> OnPlayerHealthChange;
    public Action<int> OnPlayerMaxHealthChange;

    // Soil can only consume the interact input during the day.
    [HideInInspector]
    public bool InteractPressed => !GameManager.Instance.nightTime && interactInput.action.WasPressedThisFrame();

    Rigidbody2D rb;
    SpriteRenderer playerSprite;
    float nextAttackTime;
    bool isInvincible;
    bool isDead;

    private Animator playerAnimator;

    private void OnEnable()
    {
        OnPlayerHealthChange += (int newHp) => hp = newHp;
        OnPlayerMaxHealthChange += (int newMax) => maxHealth = newMax;
        GameManager.Instance.OnNightEnd += () => hp = maxHealth;
    }
    private void OnDisable()
    {
        OnPlayerHealthChange -= (int newHp) => hp = newHp;
        OnPlayerMaxHealthChange -= (int newMax) => maxHealth = newMax;
        GameManager.Instance.OnNightEnd -= () => OnPlayerHealthChange?.Invoke(maxHealth);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerSprite = GetComponentInChildren<SpriteRenderer>();
        playerAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!isDead &&
            GameManager.Instance.nightTime &&
            interactInput.action.WasPressedThisFrame())
        {
            if (attack == null || Time.time < nextAttackTime)
                return;

            nextAttackTime = Time.time + attackCooldown;
            playerAnimator.SetTrigger("Attack");
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
            return;

        Vector2 direction = moveInput.action.ReadValue<Vector2>();

        if (direction.x > 0.01f)
        {
            playerAnimator.transform.localScale =
                new Vector3(1f, 1f, 1f);
        }
        else if (direction.x < -0.01f)
        {
            playerAnimator.transform.localScale =
                new Vector3(-1f, 1f, 1f);
        }

        rb.MovePosition(
            rb.position + direction.normalized * speed * Time.deltaTime
        );
    }

    private void TryAttack()
    {
        if (attack == null || attackDirection == null)
            return;

        GameObject spawnedAttack = Instantiate(attack, attackDirection.position,attackDirection.rotation);

        // Makes sure the attack hitbox follows our player.
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

        OnPlayerHealthChange?.Invoke(hp - 1);
        StartCoroutine(DamageFlash());

        if (hp <= 0)
        {
            isDead = true;
            OnPlayerDie?.Invoke();
            StartCoroutine(DeathSequence());
        }
    }

    // Vibe coded this because the i-frame math is a pain.
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
                playerSprite.color = showTransparent
                    ? transparentColor
                    : originalColor;

                showTransparent = !showTransparent;

                float waitTime = Mathf.Min(
                    pulseTime,
                    invincibilityTime - elapsedTime
                );

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

        // Allows the scene transition to finish before loading.
        yield return new WaitForSecondsRealtime(deathSceneDelay);

        SceneManager.LoadScene(deathSceneName);
    }
}