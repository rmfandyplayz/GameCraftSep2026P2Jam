using System;
using UnityEngine;

public class PumpkinEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform spriteTransform;
    [SerializeField] Collider2D hitboxCollider;

    [Header("Idle")]
    [SerializeField] float idleDurationMin = 1f;
    [SerializeField] float idleDurationMax = 2.5f;

    [Header("Jumping")]
    [SerializeField] AnimationCurve jumpCurve;
    [SerializeField] float jumpHeight = 5f;
    [SerializeField] float jumpDuration = 0.5f;
    [SerializeField] float jumpMoveSpeed = 5f;

    [Header("Wind Up")]
    [SerializeField] float windUpDuration = 0.5f;

    [Header("Attacking")]
    [SerializeField] float attackDuration = 0.2f;
    [SerializeField] float attackMoveSpeed = 15f;

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

    void Start()
    {
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
                HandleStateTimer(enemyStates.JUMPING, jumpDuration, () =>
                {
                    hitboxCollider.enabled = false;
                });
                break;
            case enemyStates.JUMPING:
                rb.MovePosition(rb.position + (playerDir * jumpMoveSpeed * Time.deltaTime));

                Vector3 tempSpritePos = spriteTransform.localPosition;
                float progress = 1f - (stateTimer / jumpDuration);
                Debug.Log(progress);
                tempSpritePos.y = jumpCurve.Evaluate(progress) * jumpHeight;
                spriteTransform.localPosition = tempSpritePos;

                HandleStateTimer(enemyStates.WIND_UP, windUpDuration, () =>
                {
                    hitboxCollider.enabled = true;
                });
                break;
            case enemyStates.WIND_UP:
                rb.linearVelocity = Vector3.zero;
                HandleStateTimer(enemyStates.ATTACKING, attackDuration);
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
}
