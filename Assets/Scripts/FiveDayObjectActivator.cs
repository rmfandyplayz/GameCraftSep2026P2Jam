using System.Collections.Generic;
using UnityEngine;

public class FiveDayObjectActivator : MonoBehaviour
{
    [Header("Activation Schedule")]
    [SerializeField, Min(1)] int daysPerActivation = 5;

    [Tooltip("Objects are activated in this order. Set them inactive in the scene first.")]
    [SerializeField] List<GameObject> objectsToActivate = new List<GameObject>();

    int daysPassed;
    int nextObjectIndex;
    GameManager subscribedGameManager;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        // OnEnable can run before GameManager.Awake, so try again in Start.
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void TrySubscribe()
    {
        if (subscribedGameManager != null || GameManager.Instance == null)
            return;

        subscribedGameManager = GameManager.Instance;
        subscribedGameManager.OnDayBegin += HandleDayBegin;
    }

    private void Unsubscribe()
    {
        if (subscribedGameManager != null)
            subscribedGameManager.OnDayBegin -= HandleDayBegin;

        subscribedGameManager = null;
    }

    private void HandleDayBegin()
    {
        daysPassed += 1;

        if (daysPassed < daysPerActivation)
            return;

        daysPassed = 0;
        ActivateNextObject();
    }

    private void ActivateNextObject()
    {
        // Skip empty entries without breaking the rest of the list.
        while (nextObjectIndex < objectsToActivate.Count)
        {
            GameObject nextObject = objectsToActivate[nextObjectIndex];
            nextObjectIndex += 1;

            if (nextObject == null)
                continue;

            nextObject.SetActive(true);
            return;
        }

        // The schedule is complete, so this component no longer needs the event.
        Unsubscribe();
    }
}
