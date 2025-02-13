using System.Collections;
using UnityEngine;
using FS_CombatSystem; // Assuming FS_CombatSystem is the namespace for MeleeFighter and other fight components

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public GameObject enemyPrefab; // Reference to the enemy prefab
    public Transform spawnPoint; // Position where enemies spawn
    public float spawnDelay = 1f; // Delay between enemy spawns

    private GameObject currentEnemy; // Track the currently spawned enemy

    void Start()
    {
        SpawnEnemy(); // Start by spawning the first enemy
    }

    void SpawnEnemy()
    {
        if (currentEnemy == null) // Check if there's no active enemy
        {
            currentEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            MeleeFighter meleeFighter = currentEnemy.GetComponent<MeleeFighter>();

            if (meleeFighter != null)
            {
                meleeFighter.OnDeath -= OnEnemyDeath; // Prevent duplicate subscriptions
                meleeFighter.OnDeath += OnEnemyDeath; // Subscribe to enemy's death event
            }
            else
            {
                Debug.LogError("MeleeFighter component missing on spawned enemy.");
            }
        }
    }

    void OnEnemyDeath()
{
    if (currentEnemy != null)
    {
        currentEnemy = null; // Clear the reference to the dead enemy
    }
    StartCoroutine(SpawnEnemyWithDelay());
}


    IEnumerator SpawnEnemyWithDelay()
    {
        yield return new WaitForSeconds(spawnDelay); // Wait for the specified delay
        SpawnEnemy(); // Spawn the next enemy
    }
}
