using System.Collections.Generic;
using UnityEngine;

namespace FS_CombatSystem
{
    public class SpawnSystemController : MonoBehaviour
    {
        [Header("Enemy Setup")]
        public GameObject enemyPrefab; // Prefab to spawn
        public List<Transform> spawnPoints; // List of spawn points
        public int maxEnemies = 5; // Max number of enemies allowed at once
        public float spawnInterval = 10f; // Time between spawns

        [Header("Dependencies")]
       // public DemoSceneDoorManager doorManager; // Reference to DemoSceneDoorManager
        public Material spawnRoomMaterial;

        private List<GameObject> activeEnemies = new List<GameObject>(); // Track spawned enemies
        private bool isSpawning = false;

        private void Start()
        {
            StartSpawning();
        }

        // Start spawning enemies periodically
        public void StartSpawning()
        {
            if (!isSpawning)
            {
                isSpawning = true;
                StartCoroutine(SpawnEnemiesPeriodically());
            }
        }

        // Stop spawning enemies
        public void StopSpawning()
        {
            isSpawning = false;
            StopCoroutine(SpawnEnemiesPeriodically());
        }

        private System.Collections.IEnumerator SpawnEnemiesPeriodically()
        {
            while (isSpawning)
            {
                if (activeEnemies.Count < maxEnemies)
                {
                    SpawnEnemy();
                }
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        private void SpawnEnemy()
        {
            if (enemyPrefab == null || spawnPoints.Count == 0)
            {
                Debug.LogError("Enemy prefab or spawn points not assigned.");
                return;
            }

            // Pick a random spawn point
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

            // Instantiate enemy at spawn point
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

            // Set up the enemy (if additional setup is needed)
            SetupEnemy(enemy);

            // Track the spawned enemy
            activeEnemies.Add(enemy);
        }

        private void SetupEnemy(GameObject enemy)
        {
            // Assign a layer for the enemy
            enemy.layer = LayerMask.NameToLayer("Enemy");

            // Ensure the enemy has all required components
            var controller = enemy.GetComponent<EnemyController>();
            if (controller == null)
            {
                Debug.LogError("Spawned enemy is missing EnemyController.");
                return;
            }

            // Initialize enemy in the current room (interacts with DemoSceneDoorManager)
            //if (doorManager != null)
            {
                //doorManager.SetRoomForTutorial(null, enemy);
            }
        }

        public void DespawnEnemy(GameObject enemy)
        {
            if (activeEnemies.Contains(enemy))
            {
                activeEnemies.Remove(enemy);
                Destroy(enemy);
            }
        }

        public void ClearAllEnemies()
        {
            foreach (var enemy in activeEnemies)
            {
                Destroy(enemy);
            }
            activeEnemies.Clear();
        }
    }
}
