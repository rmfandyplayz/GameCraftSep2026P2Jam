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
    [Tooltip("Relative to the other rare types, which each have a weight of 1.")]
    [SerializeField, Range(0f, 1f)] float rainbowSoilWeight = 0.5f;

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
        // Day 1 always starts with regular soil.
        RefreshDailySoils(allowRareSoils: false);
    }

    private void Update()
    {
        // If this scene object starts before the persistent GameManager is ready,
        // keep trying until it can receive every later day-start event.
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

        if (!activationScheduleComplete && daysPassed >= daysPerActivation)
        {
            daysPassed = 0;
            ActivateNextObject();
        }

        // OnDayBegin is raised after the first playable day, so rare soil is
        // available from Day 2 onward.
        RefreshDailySoils(allowRareSoils: true);
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

    private void RefreshDailySoils(bool allowRareSoils)
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
        int rareSoilCount = allowRareSoils
            ? Mathf.Min(availableColumnCount * rareSoilsPerColumn, activeSoils.Count)
            : 0;

        Soil.SoilVariant[] guaranteedCoreVariants =
        {
            Soil.SoilVariant.Speed,
            Soil.SoilVariant.Health,
            Soil.SoilVariant.Damage
        };
        ShuffleVariants(guaranteedCoreVariants);

        List<Soil.SoilVariant> availableVariants = CreateRareVariantPool();

        // Partial Fisher-Yates shuffle: the first rareSoilCount entries are unique.
        for (int i = 0; i < rareSoilCount; i++)
        {
            int randomIndex = Random.Range(i, activeSoils.Count);
            Soil chosenSoil = activeSoils[randomIndex];
            activeSoils[randomIndex] = activeSoils[i];
            activeSoils[i] = chosenSoil;

            Soil.SoilVariant chosenVariant;

            if (rareSoilCount >= guaranteedCoreVariants.Length &&
                i < guaranteedCoreVariants.Length)
            {
                // Preserve one Speed, Health, and Damage soil when at least three
                // rare plots are available.
                chosenVariant = guaranteedCoreVariants[i];
            }
            else
            {
                chosenVariant = ChooseRareVariant(availableVariants);

                // With fewer than three spots, remove the chosen type so the
                // day's rare soils cannot match each other.
                if (rareSoilCount < guaranteedCoreVariants.Length)
                    availableVariants.Remove(chosenVariant);
            }

            activeSoils[i].SetVariant(chosenVariant);
        }
    }

    private static void ShuffleVariants(Soil.SoilVariant[] variants)
    {
        for (int i = variants.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Soil.SoilVariant chosenVariant = variants[randomIndex];
            variants[randomIndex] = variants[i];
            variants[i] = chosenVariant;
        }
    }

    private static List<Soil.SoilVariant> CreateRareVariantPool()
    {
        return new List<Soil.SoilVariant>
        {
            Soil.SoilVariant.Speed,
            Soil.SoilVariant.Health,
            Soil.SoilVariant.Damage,
            Soil.SoilVariant.Rainbow
        };
    }

    private Soil.SoilVariant ChooseRareVariant(List<Soil.SoilVariant> variants)
    {
        float totalWeight = 0f;

        foreach (Soil.SoilVariant variant in variants)
            totalWeight += GetVariantWeight(variant);

        float roll = Random.value * totalWeight;

        foreach (Soil.SoilVariant variant in variants)
        {
            roll -= GetVariantWeight(variant);

            if (roll <= 0f)
                return variant;
        }

        return variants[variants.Count - 1];
    }

    private float GetVariantWeight(Soil.SoilVariant variant)
    {
        return variant == Soil.SoilVariant.Rainbow ? rainbowSoilWeight : 1f;
    }
}
