using System.Collections.Generic;
using UnityEngine;

public class FiveDayObjectActivator : MonoBehaviour
{
    [Header("Activation Schedule")]
    [SerializeField, Min(1)] int daysPerActivation = 5;

    [Tooltip("Objects are activated in this order. Set them inactive in the scene first.")]
    [SerializeField] List<GameObject> objectsToActivate = new List<GameObject>();

    [Header("Rare Soil")]
    [Tooltip("The columns that are already available before this activator unlocks any more.")]
    [SerializeField, Min(1)] int startingColumnCount = 1;
    [SerializeField, Min(0)] int rareSoilsPerColumn = 2;

    int daysPassed;
    int nextObjectIndex;
    int unlockedColumnCount;
    bool activationScheduleComplete;
    GameManager subscribedGameManager;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        // OnEnable can run before GameManager.Awake, so try again in Start.
        TrySubscribe();

        // The first playable day has already begun before this component's Start.
        RefreshDailySoils();
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

        if (!activationScheduleComplete && daysPassed >= daysPerActivation)
        {
            daysPassed = 0;
            ActivateNextObject();
        }

        RefreshDailySoils();
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
            unlockedColumnCount += 1;
            return;
        }

        // Keep listening so the soil variants still reroll every day.
        activationScheduleComplete = true;
    }

    private void RefreshDailySoils()
    {
        Soil[] allSoils = FindObjectsByType<Soil>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        List<Soil> activeSoils = new List<Soil>();

        // Reset every loaded plot, including plots in columns that are still locked.
        foreach (Soil soil in allSoils)
        {
            soil.SetVariant(Soil.SoilVariant.Regular);

            if (soil.gameObject.activeInHierarchy)
                activeSoils.Add(soil);
        }

        int availableColumnCount = startingColumnCount + unlockedColumnCount;
        int rareSoilCount = Mathf.Min(
            availableColumnCount * rareSoilsPerColumn,
            activeSoils.Count);

        Soil.SoilVariant[] rareVariants =
        {
            Soil.SoilVariant.Speed,
            Soil.SoilVariant.Health,
            Soil.SoilVariant.Damage
        };

        // Shuffle the three types once, then cycle through them. This guarantees
        // that the first two differ and every group of three contains every type.
        for (int i = rareVariants.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Soil.SoilVariant chosenVariant = rareVariants[randomIndex];
            rareVariants[randomIndex] = rareVariants[i];
            rareVariants[i] = chosenVariant;
        }

        // Partial Fisher-Yates shuffle: the first rareSoilCount entries are unique.
        for (int i = 0; i < rareSoilCount; i++)
        {
            int randomIndex = Random.Range(i, activeSoils.Count);
            Soil chosenSoil = activeSoils[randomIndex];
            activeSoils[randomIndex] = activeSoils[i];
            activeSoils[i] = chosenSoil;

            activeSoils[i].SetVariant(rareVariants[i % rareVariants.Length]);
        }
    }
}
