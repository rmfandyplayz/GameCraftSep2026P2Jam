using UnityEngine;

public class PumpkinEnemy : MonoBehaviour
{
    enum enemyStates
    {
        IDLE,
        JUMPING,
        ATTACKING
    }
    enemyStates currentState = enemyStates.IDLE;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case enemyStates.IDLE:
                break;
            case enemyStates.JUMPING:
                break;
            case enemyStates.ATTACKING:
                break;
        }
    }
}
