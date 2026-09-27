using System;
using System.Collections;
using TMPro;
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
    public Color damageFlashColor = Color.red;
    [Range(0f, 1f)] public float damageFlashAlpha = 0.35f;
    public float flashInterval = 0.04f;

    [Header("Night Start Transition")]
    [SerializeField] SpriteRenderer shadowSprite;
    [SerializeField, Min(0f)] float nightFadeOutDuration = 0.35f;
    [SerializeField, Min(0f)] float nightInvisibleDuration = 1f;
    [SerializeField, Min(0f)] float nightFadeInDuration = 0.5f;
    [SerializeField, Min(0f)] float nightRiseDistance = 0.5f;

    [Header("Death")]
    public string deathSceneName = "deathScene";
    public float deathSceneDelay = 1f;

    [Header("Five Day Popup")]
    [Tooltip("An initially inactive UI object with the TMP text as a child.")]
    [SerializeField] GameObject fiveDayPopup;
    [SerializeField] TMP_Text fiveDayPopupText;
    [SerializeField, Min(1)] int daysPerPopup = 5;
    [SerializeField, Min(0f)] float popupFadeInDuration = 0.25f;
    [SerializeField, Min(0f)] float popupVisibleDuration = 3f;
    [SerializeField, Min(0f)] float popupFadeOutDuration = 0.25f;
    [SerializeField, Range(0f, 1f)] float popupStartScale = 0.6f;

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
    bool movementLocked;
    int daysSincePopup;
    Color playerDefaultColor = Color.white;
    Color shadowDefaultColor = Color.white;
    Vector3 playerSpriteDefaultLocalPosition;
    Coroutine nightTransitionRoutine;
    CanvasGroup fiveDayPopupCanvasGroup;
    Vector3 fiveDayPopupFullScale;
    Vector3 fiveDayPopupTextFullScale;
    Vector3 fiveDayPopupTextWorldOffset;
    Coroutine fiveDayPopupRoutine;

    private Animator playerAnimator;

    private void OnEnable()
    {
        OnPlayerHealthChange += HandleHealthChange;
        OnPlayerMaxHealthChange += HandleMaxHealthChange;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNightEnd += RestoreHealth;
            GameManager.Instance.OnDayBegin += HandleDayBegin;
            GameManager.Instance.OnNightBegin += HandleNightBegin;
        }
    }

    private void OnDisable()
    {
        OnPlayerHealthChange -= HandleHealthChange;
        OnPlayerMaxHealthChange -= HandleMaxHealthChange;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNightEnd -= RestoreHealth;
            GameManager.Instance.OnDayBegin -= HandleDayBegin;
            GameManager.Instance.OnNightBegin -= HandleNightBegin;
        }

        movementLocked = false;
        nightTransitionRoutine = null;

        if (playerSprite != null)
        {
            playerSprite.color = playerDefaultColor;
            playerSprite.transform.localPosition = playerSpriteDefaultLocalPosition;
        }

        if (shadowSprite != null)
            shadowSprite.color = shadowDefaultColor;
    }

    private void HandleHealthChange(int newHp)
    {
        hp = newHp;
    }

    private void HandleMaxHealthChange(int newMax)
    {
        maxHealth = newMax;
    }

    private void RestoreHealth()
    {
        OnPlayerHealthChange?.Invoke(maxHealth);
    }

    private void HandleDayBegin()
    {
        daysSincePopup += 1;

        if (daysSincePopup < daysPerPopup)
            return;

        daysSincePopup = 0;

        if (fiveDayPopup == null)
            return;

        if (fiveDayPopupRoutine != null)
            StopCoroutine(fiveDayPopupRoutine);

        fiveDayPopupRoutine = StartCoroutine(ShowFiveDayPopup());
    }

    private void HandleNightBegin()
    {
        if (playerSprite == null)
            return;

        if (nightTransitionRoutine != null)
            StopCoroutine(nightTransitionRoutine);

        nightTransitionRoutine = StartCoroutine(PlayNightStartTransition());
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();

        SpriteRenderer[] childSprites = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer childSprite in childSprites)
        {
            if (childSprite.gameObject.name.Equals(
                "PlayerSprite",
                StringComparison.OrdinalIgnoreCase
            ))
            {
                playerSprite = childSprite;
            }
            else if (shadowSprite == null && childSprite.gameObject.name.Equals(
                "Shadow",
                StringComparison.OrdinalIgnoreCase
            ))
            {
                shadowSprite = childSprite;
            }
        }

        if (playerSprite == null && childSprites.Length > 0)
            playerSprite = childSprites[0];

        if (playerSprite != null)
        {
            playerDefaultColor = playerSprite.color;
            playerSpriteDefaultLocalPosition = playerSprite.transform.localPosition;
        }

        if (shadowSprite != null)
            shadowDefaultColor = shadowSprite.color;

        if (fiveDayPopup != null)
        {
            fiveDayPopupCanvasGroup = fiveDayPopup.GetComponent<CanvasGroup>();

            if (fiveDayPopupCanvasGroup == null)
                fiveDayPopupCanvasGroup = fiveDayPopup.AddComponent<CanvasGroup>();

            fiveDayPopupFullScale = fiveDayPopup.transform.localScale;

            if (fiveDayPopupText == null)
                fiveDayPopupText = fiveDayPopup.GetComponentInChildren<TMP_Text>(true);

            if (fiveDayPopupText != null)
            {
                fiveDayPopupTextFullScale = fiveDayPopupText.transform.localScale;
                fiveDayPopupTextWorldOffset =
                    fiveDayPopupText.transform.position - transform.position;
                KeepFiveDayPopupTextUnflipped();
            }

            fiveDayPopup.SetActive(false);
        }
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
        if (isDead || movementLocked)
            return;

        Vector2 direction = moveInput.action.ReadValue<Vector2>();

        if (direction.x > 0.01f)
        {
            playerAnimator.transform.localScale =
                new Vector3(1f, 1f, 1f);
            KeepFiveDayPopupTextUnflipped();
        }
        else if (direction.x < -0.01f)
        {
            playerAnimator.transform.localScale =
                new Vector3(-1f, 1f, 1f);
            KeepFiveDayPopupTextUnflipped();
        }

        rb.MovePosition(
            rb.position + direction.normalized * speed * Time.deltaTime
        );
    }

    private void KeepFiveDayPopupTextUnflipped()
    {
        if (fiveDayPopupText == null)
            return;

        Transform textTransform = fiveDayPopupText.transform;
        float parentScaleX = textTransform.parent == null
            ? 1f
            : textTransform.parent.lossyScale.x;

        Vector3 textScale = fiveDayPopupTextFullScale;
        textScale.x = Mathf.Abs(textScale.x) * (parentScaleX < 0f ? -1f : 1f);
        textTransform.localScale = textScale;
        textTransform.position = transform.position + fiveDayPopupTextWorldOffset;
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
        if (collision != null)
        {
            TakeDamage(collision.collider);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TakeDamage(other);
    }

    private void TakeDamage(Collider2D other)
    {
        if (isDead || isInvincible || !other.CompareTag("Enemy"))
            return;

        PumpkinEnemy pumpkinEnemy = other.GetComponentInParent<PumpkinEnemy>();
        int damageTaken = pumpkinEnemy == null ? 1 : pumpkinEnemy.damage;

        OnPlayerHealthChange?.Invoke(hp - damageTaken);
        StartCoroutine(DamageFlash());

        if (hp <= 0)
        {
            //isDead = true;
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
            Color flashColor = damageFlashColor;
            flashColor.a = damageFlashAlpha;

            float elapsedTime = 0f;
            float pulseTime = Mathf.Max(0.01f, flashInterval);
            bool showTransparent = true;

            while (elapsedTime < invincibilityTime)
            {
                playerSprite.color = showTransparent
                    ? flashColor
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

    private IEnumerator PlayNightStartTransition()
    {
        movementLocked = true;

        Vector3 raisedPosition = playerSpriteDefaultLocalPosition
            + Vector3.up * nightRiseDistance;
        float elapsedTime = 0f;

        while (elapsedTime < nightFadeOutDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = nightFadeOutDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsedTime / nightFadeOutDuration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            SetNightTransitionAlpha(1f - smoothProgress);
            playerSprite.transform.localPosition = Vector3.Lerp(
                playerSpriteDefaultLocalPosition,
                raisedPosition,
                smoothProgress
            );

            yield return null;
        }

        SetNightTransitionAlpha(0f);
        playerSprite.transform.localPosition = raisedPosition;

        yield return new WaitForSecondsRealtime(nightInvisibleDuration);

        // Reset only the visual child while it is invisible. The Rigidbody and
        // Player root never moved, so the player reappears in the same place.
        playerSprite.transform.localPosition = playerSpriteDefaultLocalPosition;
        elapsedTime = 0f;

        while (elapsedTime < nightFadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = nightFadeInDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsedTime / nightFadeInDuration);

            SetNightTransitionAlpha(progress);

            yield return null;
        }

        SetNightTransitionAlpha(1f);
        playerSprite.transform.localPosition = playerSpriteDefaultLocalPosition;
        movementLocked = false;
        nightTransitionRoutine = null;
    }

    private void SetNightTransitionAlpha(float alpha)
    {
        Color playerColor = playerDefaultColor;
        playerColor.a = playerDefaultColor.a * alpha;
        playerSprite.color = playerColor;

        if (shadowSprite == null)
            return;

        Color shadowColor = shadowDefaultColor;
        shadowColor.a = shadowDefaultColor.a * alpha;
        shadowSprite.color = shadowColor;
    }

    private IEnumerator ShowFiveDayPopup()
    {
        Transform popupTransform = fiveDayPopup.transform;
        Vector3 smallScale = fiveDayPopupFullScale * popupStartScale;

        fiveDayPopup.SetActive(true);
        fiveDayPopupCanvasGroup.alpha = 0f;
        popupTransform.localScale = smallScale;
        KeepFiveDayPopupTextUnflipped();

        float elapsedTime = 0f;

        while (elapsedTime < popupFadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = popupFadeInDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsedTime / popupFadeInDuration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            fiveDayPopupCanvasGroup.alpha = smoothProgress;
            popupTransform.localScale = Vector3.Lerp(
                smallScale,
                fiveDayPopupFullScale,
                smoothProgress
            );
            KeepFiveDayPopupTextUnflipped();

            yield return null;
        }

        fiveDayPopupCanvasGroup.alpha = 1f;
        popupTransform.localScale = fiveDayPopupFullScale;
        KeepFiveDayPopupTextUnflipped();

        yield return new WaitForSecondsRealtime(popupVisibleDuration);

        elapsedTime = 0f;

        while (elapsedTime < popupFadeOutDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = popupFadeOutDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsedTime / popupFadeOutDuration);

            fiveDayPopupCanvasGroup.alpha = 1f - progress;
            yield return null;
        }

        fiveDayPopupCanvasGroup.alpha = 0f;
        fiveDayPopup.SetActive(false);
        fiveDayPopupRoutine = null;
    }

    private IEnumerator DeathSequence()
    {
        OnPlayerDie?.Invoke();
        GameObject panel = null;

        panel = GameObject.FindGameObjectWithTag("panel");
        Animator panelAnimator = panel.GetComponent<Animator>();
        panelAnimator.SetTrigger("end");

        // Allows the scene transition to finish before loading.
        yield return new WaitForSecondsRealtime(deathSceneDelay);
        panelAnimator.SetTrigger("reset");

        // Ending the night already raises OnDayBegin through GameManager.EndNight.
        // Raise it before changing scenes so current scene listeners receive it.
        //if (GameManager.Instance != null)
        //GameManager.Instance.OnNightEnd?.Invoke();

        //SceneManager.LoadScene(deathSceneName);
    }
}
