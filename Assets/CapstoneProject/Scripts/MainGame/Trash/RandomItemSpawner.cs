using UnityEngine;

public class RandomItemSpawner : MonoBehaviour
{
    [Header("Items That Can Spawn")]
    public GameObject[] possibleItems;

    [Header("Spawn Settings")]
    public bool spawnOnStart = true;
    public bool destroySpawnerAfterSpawn = false;

    private GameObject spawnedItem;

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnRandomItem();
        }
    }

    public void SpawnRandomItem()
    {
        if (possibleItems == null || possibleItems.Length == 0)
        {
            Debug.LogWarning("No possible items assigned to RandomItemSpawner.");
            return;
        }

        int randomIndex = Random.Range(0, possibleItems.Length);

        GameObject itemToSpawn = possibleItems[randomIndex];

        if (itemToSpawn == null)
        {
            Debug.LogWarning("Randomly selected item is missing.");
            return;
        }

        spawnedItem = Instantiate(
            itemToSpawn,
            transform.position,
            transform.rotation
        );

        if (destroySpawnerAfterSpawn)
        {
            Destroy(gameObject);
        }
    }
}