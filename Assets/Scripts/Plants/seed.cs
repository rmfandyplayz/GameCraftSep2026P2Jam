using UnityEngine;

public class seed : MonoBehaviour
{
    [SerializeField] private GameObject enemyPlant;

    private void OnEnable()
    {
        GameManager.Instance.OnSeedPlanted?.Invoke();
        GameManager.Instance.OnNightBegin += HandleNightBegin;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNightBegin -= HandleNightBegin;
        }

        // Cancel pending Invocations if disabled before spawning
        CancelInvoke(nameof(SpawnPlant));
    }

    private void HandleNightBegin()
    {
        Invoke(nameof(SpawnPlant), Random.Range(0f, 2f));
    }

    private void SpawnPlant()
    {
        Instantiate(enemyPlant, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}