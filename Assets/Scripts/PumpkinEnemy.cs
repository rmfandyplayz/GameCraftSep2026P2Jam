using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PumpkinEnemy : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] AudioClip attackSound;
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip spawnSound;

    [Header("References")]
    [SerializeField] Transform healthBox;
    [SerializeField] GameObject enemyHealthPrefab;
    [SerializeField] SpriteRenderer pumpkinSprite;
    [SerializeField] Collider2D hitboxCollider;

    [Header("Idle")]
    [SerializeField] float idleDurationMin = 1f;
    [SerializeField] float idleDurationMax = 2.5f;

    [Header("Jumping")]
    [SerializeField] float targetRadius = 5;
    [SerializeField] AnimationCurve jumpCurve;
    [SerializeField] float jumpHeight = 5f;
    [SerializeField] float jumpDurationMin = 1f;
    [SerializeField] float jumpDurationMax = 3f;
    float jumpDuration;
    [SerializeField] float jumpMoveSpeed = 5f;
    [SerializeField] float jumpDirectionRandomness = 5;
    Vector2 jumpDirection = Vector2.zero;

    [Header("Wind Up")]
    [SerializeField] float windUpDuration = 0.5f;
    [SerializeField] float attackDistance = 5f;

    [Header("Attacking")]
    [SerializeField] float attackDuration = 0.2f;
    [SerializeField] float attackMoveSpeed = 15f;

    [Header("Combat")]
    [Min(0)] public int damage = 1;
    [Min(1)] public int hp = 1;
    [SerializeField, Min(0f)] float damageFlashDuration = 0.12f;
    [SerializeField] Color damageFlashColor = Color.red;

    [Header("Effects")]
    [Min(0)] public int value = 67;
    [Tooltip("The color used by this enemy's +value popup.")]
    [SerializeField] Color valuePopupTextColor = new Color(1f, 0.85f, 0.2f);
    [Tooltip("World-space canvas prefab with a TMP text child.")]
    [SerializeField] GameObject valuePopupPrefab;

    [Header("Value Popup Animation")]
    [Min(0.01f)] public float valuePopupRiseAndGrowDuration = 0.7f;
    [Min(0f)] public float valuePopupStationaryDuration = 0.4f;
    [Min(0.01f)] public float valuePopupFadeOutDuration = 0.3f;
    [Min(0f)] public float valuePopupRiseDistance = 0.75f;
    [Min(0.01f)] public float valuePopupStartingScale = 0.45f;
    public GameObject deathEffect;

    enum enemyStates
    {
        SPAWNING,
        IDLE,
        JUMPING,
        WIND_UP,
        ATTACKING
    }
    enemyStates currentState = enemyStates.SPAWNING;
    private Transform target;

    private float stateTimer = 0.5f;
    Rigidbody2D rb;

    Animator pumpkinAnimator;
    Player subscribedPlayer;
    Coroutine damageFlashRoutine;
    Color pumpkinDefaultColor = Color.white;

    private void OnEnable()
    {
        if (GameManager.Instance == null)
            return;

        subscribedPlayer = GameManager.Instance.player;

        if (subscribedPlayer != null)
            subscribedPlayer.OnPlayerDie += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        if (subscribedPlayer != null)
            subscribedPlayer.OnPlayerDie -= HandlePlayerDeath;

        subscribedPlayer = null;

        if (pumpkinSprite != null)
            pumpkinSprite.color = pumpkinDefaultColor;
    }

    private void GenerateHealthBox()
    {
        if (healthBox == null) return;
        foreach (Transform child in healthBox)
        {
            Destroy(child.gameObject);
        }
        for(int i = 0; i < hp; i++)
        {
            Instantiate(enemyHealthPrefab, healthBox);
        }
    }

    private void HandlePlayerDeath()
    {
        Destroy(gameObject);
    }

    void Start()
    {
        GetComponent<AudioSource>().PlayOneShot(spawnSound);
        pumpkinAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        target = FindFirstObjectByType<Player>().transform;
        stateTimer = 0.5f;

        if (pumpkinSprite != null)
            pumpkinDefaultColor = pumpkinSprite.color;
        GenerateHealthBox();
    }

    void FixedUpdate()
    {
        if (target == null) return;
        Vector2 playerDir = (target.position - transform.position).normalized;
        switch (currentState)
        {
            case enemyStates.SPAWNING:
                HandleStateTimer(enemyStates.IDLE, UnityEngine.Random.Range(idleDurationMin, idleDurationMax));
                break;
            case enemyStates.IDLE:
                rb.linearVelocity = Vector3.zero;
                jumpDuration = UnityEngine.Random.Range(jumpDurationMin, jumpDurationMax);
                HandleStateTimer(enemyStates.JUMPING, jumpDuration, () =>
                {
                    Vector2 targetOffset = new Vector2(UnityEngine.Random.Range(-targetRadius, targetRadius), UnityEngine.Random.Range(-targetRadius, targetRadius));
                    HandleJump(playerDir+ targetOffset);
                });
                break;
            case enemyStates.JUMPING:
                rb.MovePosition(rb.position + (jumpDirection * jumpMoveSpeed * Time.deltaTime));

                Vector3 tempSpritePos = pumpkinSprite.transform.localPosition;

                float progress = Mathf.Clamp01(1f - (stateTimer / jumpDuration));
                pumpkinSprite.sortingOrder = 10;

                tempSpritePos.y = jumpCurve.Evaluate(progress) * jumpHeight;
                pumpkinSprite.transform.localPosition = tempSpritePos;

                HandleStateTimer(enemyStates.WIND_UP, windUpDuration, () =>
                {
                    pumpkinAnimator.SetTrigger("Landed");
                    pumpkinSprite.sortingOrder = 0;
                    hitboxCollider.enabled = true;
                });
                break;
            case enemyStates.WIND_UP:
                rb.linearVelocity = Vector3.zero;

                bool canAttack = (target.position - transform.position).sqrMagnitude <= attackDistance * attackDistance;
                if (canAttack)
                {
                    HandleStateTimer(enemyStates.ATTACKING, attackDuration, () =>
                    {
                        GetComponent<AudioSource>().PlayOneShot(attackSound);
                        pumpkinAnimator.SetTrigger("Attack");
                    });
                }
                else
                {
                    HandleStateTimer(enemyStates.JUMPING, jumpDuration, () =>
                    {
                        HandleJump(playerDir);
                    });
                }
                break;
            case enemyStates.ATTACKING:
                rb.MovePosition(rb.position + (playerDir * attackMoveSpeed * Time.deltaTime));
                HandleStateTimer(enemyStates.IDLE, UnityEngine.Random.Range(idleDurationMin, idleDurationMax));
                break;
        }
    }

    private void HandleStateTimer(enemyStates nextState, float nextDuration, Action onComplete = null)
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            onComplete?.Invoke();
            currentState = nextState;
            stateTimer = nextDuration;
        }
    }

    private void HandleJump(Vector2 dir)
    {
        GetComponent<AudioSource>().PlayOneShot(jumpSound);
        pumpkinAnimator.SetTrigger("Jumping");
        jumpDirection = dir;
        float jumpAngle = Mathf.Atan2(jumpDirection.x, jumpDirection.y) * Mathf.Rad2Deg;
        jumpAngle += UnityEngine.Random.Range(-jumpDirectionRandomness, jumpDirectionRandomness);
        jumpAngle *= Mathf.Deg2Rad;
        jumpDirection = new Vector2(Mathf.Sin(jumpAngle), Mathf.Cos(jumpAngle));
        hitboxCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Attack"))
        {
            hp -= 1;
            GenerateHealthBox();
            if (hp <= 0)
            {
                GameManager.Instance.OnEnemyDie?.Invoke(value);
                CreateValuePopup();
                Instantiate(deathEffect, gameObject.transform.position, Quaternion.identity);
                Destroy(gameObject);
                return;
            }

            if (damageFlashRoutine != null)
                StopCoroutine(damageFlashRoutine);

            damageFlashRoutine = StartCoroutine(DamageFlash());
        }
    }

    private IEnumerator DamageFlash()
    {
        if (pumpkinSprite == null)
            yield break;

        pumpkinSprite.color = damageFlashColor;
        yield return new WaitForSeconds(damageFlashDuration);
        pumpkinSprite.color = pumpkinDefaultColor;
        damageFlashRoutine = null;
    }

    private void CreateValuePopup()
    {
        if (valuePopupPrefab == null)
            return;

        GameObject popupObject = Instantiate(valuePopupPrefab, transform.position, Quaternion.identity);
        EnemyValuePopup popup = popupObject.GetComponent<EnemyValuePopup>();

        if (popup == null)
            popup = popupObject.AddComponent<EnemyValuePopup>();

        popup.Play(
            value,
            valuePopupTextColor,
            valuePopupRiseAndGrowDuration,
            valuePopupStationaryDuration,
            valuePopupFadeOutDuration,
            valuePopupRiseDistance,
            valuePopupStartingScale);
    }
}
