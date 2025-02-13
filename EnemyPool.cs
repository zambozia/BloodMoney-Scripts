using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance; // Singleton instance

    public GameObject enemyPrefab; // Reference to the enemy prefab
    public int poolSize = 10; // Pool size to initialize
    public Transform spawnPoint; // Point where enemies spawn

    private Queue<GameObject> enemyPool = new Queue<GameObject>();

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of EnemyPool
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optionally keep the pool across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize the pool with inactive enemies
        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab);
            enemy.SetActive(false);
            enemyPool.Enqueue(enemy);
        }
    }

    // Method to get an enemy from the pool
    public GameObject GetEnemy()
    {
        if (enemyPool.Count > 0)
        {
            GameObject enemy = enemyPool.Dequeue();
            enemy.SetActive(true);
            return enemy;
        }
        else
        {
            // Optionally expand pool if needed
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            return enemy;
        }
    }

    public void ReturnEnemyToPool(GameObject enemy)
    {
        enemy.SetActive(false);
        enemyPool.Enqueue(enemy);
    }

}
