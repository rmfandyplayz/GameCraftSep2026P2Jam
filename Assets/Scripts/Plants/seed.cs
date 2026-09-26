using UnityEngine;

public class seed : MonoBehaviour
{
    [SerializeField] GameObject enemyPlant;
    private void OnEnable()
    {
        GameManager.Instance.OnNightBegin += spawnPlant;
    }
    private void OnDisable()
    {
        GameManager.Instance.OnNightBegin -= spawnPlant;
    }

    void spawnPlant()
    {
        Instantiate(enemyPlant, transform.position,Quaternion.Euler(Vector3.zero));
        Destroy(this.gameObject);
    }
}
