using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PumpkinEnemy : MonoBehaviour
{
    [Header("References")]
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

    [Header("Effects")]
    public GameObject deathEffect;

    enum enemyStates
    {
        IDLE,
        JUMPING,
        WIND_UP,
        ATTACKING
    }
    enemyStates currentState = enemyStates.IDLE;
    private Transform target;

    private float stateTimer = 0;
    Rigidbody2D rb;

    Animator pumpkinAnimator;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDie += HandlePlayerDie;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDie -= HandlePlayerDie;
        }
    }

    private void HandlePlayerDie()
    {
        Destroy(gameObject);
    }

    void Start()
    {
        pumpkinAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        target = FindFirstObjectByType<Player>().transform;
        stateTimer = UnityEngine.Random.Range(idleDurationMin, idleDurationMax);

    }

    void FixedUpdate()
    {
        if (target == null) return;
        Vector2 playerDir = (target.position - transform.position).normalized;
        switch (currentState)
        {
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
                    pumpkinAnimator.SetTrigger("Attack");
                    HandleStateTimer(enemyStates.ATTACKING, attackDuration);
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
            Instantiate(deathEffect, gameObject.transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
