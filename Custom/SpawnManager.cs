using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;        // The enemy prefab to spawn
    public Transform[] spawnPoints;       // Array of spawn points
    public float spawnInterval = 5f;      // Time between spawns
    public int maxEnemies = 5;            // Maximum number of active enemies
    public float spawnHeightOffset = 1.0f; // Offset to spawn enemies slightly above ground

    private int currentEnemyCount = 0;    // Keeps track of active enemies in the scene

    private void Start()
    {
        // Start spawning enemies periodically
        StartCoroutine(SpawnEnemiesPeriodically());
    }

    private IEnumerator SpawnEnemiesPeriodically()
    {
        while (true)
        {
            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
            }

            // Wait for the specified interval before the next spawn
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        // Ensure we have spawn points assigned
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned to SpawnManager.");
            return;
        }

        // Pick a random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Calculate the spawn position with the height offset
        Vector3 spawnOffset = new Vector3(0, spawnHeightOffset, 0);
        Vector3 adjustedSpawnPosition = spawnPoint.position + spawnOffset;

        // Instantiate the enemy at the adjusted spawn position
        GameObject enemy = Instantiate(enemyPrefab, adjustedSpawnPosition, Quaternion.identity);

        // Initialize enemy components (optional, if needed)
        InitializeEnemy(enemy);

        // Increment the active enemy count
        currentEnemyCount++;
    }

    private void InitializeEnemy(GameObject enemy)
    {
        // Ensure enemy has required components
        if (enemy.GetComponent<Animator>() == null)
        {
            Debug.LogError("Enemy is missing an Animator component!");
        }
        if (enemy.GetComponent<Rigidbody>() == null)
        {
            Debug.LogError("Enemy is missing a Rigidbody component!");
        }
        if (enemy.GetComponent<Collider>() == null)
        {
            Debug.LogError("Enemy is missing a Collider component!");
        }

        // Additional initialization logic (e.g., assigning layers)
        enemy.layer = LayerMask.NameToLayer("Enemy");
    }

    public void OnEnemyDestroyed()
    {
        // Called when an enemy is destroyed to decrement the count
        currentEnemyCount--;
    }
}
