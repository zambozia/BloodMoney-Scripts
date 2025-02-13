using System.Collections;
using UnityEngine;
using UnityEngine.Events; // Required for UnityEvent
using FS_CombatSystem;
using TMPro;
using UnityEngine.UI;


public class WaveSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] enemyPrefabs;         // Array of enemy prefabs
    public Transform[] spawnPoints;           // Array of spawn points
    public int maxEnemiesPerWave = 5;         // Max enemies per wave
    public float spawnInterval = 2f;          // Time between enemy spawns
    public float timeBetweenWaves = 5f;       // Delay between waves
    public float spawnOffset = 1.5f;          // Offset to prevent overlap

    [Header("Wave and Player Settings")]
    public int maxWaves = 10;                 // Maximum number of waves
    public MeleeFighter player;               // Reference to the player's MeleeFighter

    [Header("Events")]
    public UnityEvent onGameOver;             // Event for Game Over
    public UnityEvent onVictory;              // Event for Victory

    [Header("UI References")]
    public TextMeshProUGUI killCountText;     // Kill count text
    public TextMeshProUGUI waveNumberText;    // Wave number text
    public TextMeshProUGUI enemiesLeftText;   // Enemies left in the wave
    public Image staminaBar;


    private int currentWave = 0;              // Tracks the current wave
    private int enemiesSpawnedInWave = 0;     // Enemies spawned in the current wave
    private int enemiesRemaining;             // Enemies still alive in the wave
    private int killCount = 0;                // Total kills

    private bool isWaveInProgress = false;

    private void Start()
    {
        // Initialize UI elements
        killCountText.text = "Kills: 0";
        waveNumberText.text = "Wave: 1";
        enemiesLeftText.text = "Enemies Left: 0";

        StartCoroutine(StartNextWave());
    }

    private void Update()
    {
        // Check if player health is zero
        if (PlayerLost())
        {
            TriggerGameOver();
            enabled = false; // Stop all spawning
        }
    }

    private bool PlayerLost()
    {
        // Check if the player's current health is zero or below
        return player != null && player.CurrentHealth <= 0;
    }

    private void TriggerGameOver()
    {
        Debug.Log("Game Over! Player has died.");
        onGameOver?.Invoke(); // Trigger Game Over event
    }

    private IEnumerator StartNextWave()
    {
        if (currentWave >= maxWaves)
        {
            TriggerVictory();
            yield break;
        }

        yield return new WaitForSeconds(timeBetweenWaves);

        currentWave++;
        enemiesSpawnedInWave = 0;
        enemiesRemaining = maxEnemiesPerWave + (currentWave - 1) * 2; // Wave scaling logic

        // Update wave number UI
        waveNumberText.text = "Wave: " + currentWave;
        enemiesLeftText.text = "Enemies Left: " + enemiesRemaining;

        Debug.Log("Wave " + currentWave + " started.");
        isWaveInProgress = true;

        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        while (enemiesSpawnedInWave < enemiesRemaining)
        {
            SpawnEnemy();
            enemiesSpawnedInWave++;
            yield return new WaitForSeconds(spawnInterval);
        }

        Debug.Log("All enemies spawned for Wave " + currentWave);
    }

    private void SpawnEnemy()
    {
        if (spawnPoints.Length == 0 || enemyPrefabs.Length == 0)
        {
            Debug.LogError("No spawn points or enemy prefabs assigned.");
            return;
        }

        // Pick a random spawn point and enemy prefab
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        // Adjust spawn position with offset to avoid overlap
        Vector3 adjustedSpawnPos = spawnPoint.position +
                                   new Vector3(Random.Range(-spawnOffset, spawnOffset), 0, Random.Range(-spawnOffset, spawnOffset));

        // Instantiate the enemy
        GameObject enemy = Instantiate(enemyPrefab, adjustedSpawnPos, Quaternion.identity);

        // Subscribe to the OnDeathEvent
        var meleeFighter = enemy.GetComponent<MeleeFighter>();
        if (meleeFighter != null)
        {
            meleeFighter.OnDeathEvent.AddListener(OnEnemyDefeated);
        }
        else
        {
            Debug.LogError("MeleeFighter component missing on enemy prefab.");
        }

        Debug.Log("Spawned enemy at " + adjustedSpawnPos);
    }

    public void OnEnemyDefeated()
    {
        enemiesRemaining--;
        Debug.Log("Enemy defeated. Enemies remaining: " + enemiesRemaining);

        // Update kill count and UI
        killCount++;
        killCountText.text = "Kills: " + killCount;

        // Update enemies remaining UI
        enemiesLeftText.text = "Enemies Left: " + enemiesRemaining;

        if (enemiesRemaining <= 0 && isWaveInProgress)
        {
            Debug.Log("Wave " + currentWave + " completed!");
            isWaveInProgress = false;

            StartCoroutine(StartNextWave());
        }
    }

    private void TriggerVictory()
    {
        Debug.Log("Victory! All waves completed.");
        onVictory?.Invoke(); // Trigger Victory event
    }

    void OnPlayerDeath()
    {
        onGameOver?.Invoke();
    }
}
