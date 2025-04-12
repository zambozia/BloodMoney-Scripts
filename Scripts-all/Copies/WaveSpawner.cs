using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using FS_CombatSystem;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static System.Net.Mime.MediaTypeNames;
using System;

public class WaveSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public int maxEnemiesPerWave = 5;
    public float spawnInterval = 2f;
    public float timeBetweenWaves = 5f;
    public float spawnOffset = 1.5f;

    [Header("Wave and Player Settings")]
    public int maxWaves = 10;
    public MeleeFighter player;

    [Header("Events")]
    public UnityEvent onGameOver;
    public UnityEvent onVictory;

    [Header("UI References")]
    public TextMeshProUGUI killCountText;
    public TextMeshProUGUI waveNumberText;
    public TextMeshProUGUI enemiesLeftText;
    public UnityEngine.UI.Image staminaBar;

    private int currentWave = 0;
    private int enemiesSpawnedInWave = 0;
    private int enemiesRemaining;
    private int killCount = 0;
    private bool isWaveInProgress = false;

    private void Start()
    {
        StartCoroutine(InitializeAndBeginWaves());
    }

    private IEnumerator InitializeAndBeginWaves()
    {
        // Runtime Scene-Based Discovery (using root objects)
        while (player == null)
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                player = root.GetComponentInChildren<MeleeFighter>();
                if (player != null)
                {
                    Debug.Log("[WaveSpawner] MeleeFighter discovered in scene.");
                    break;
                }
            }

            if (player == null)
                yield return null;
        }

        // UI Init
        killCountText.text = "Kills: 0";
        waveNumberText.text = "Wave: 1";
        enemiesLeftText.text = "Enemies Left: 0";

        yield return StartCoroutine(StartNextWave());
    }

    private void Update()
    {
        if (PlayerLost())
        {
            TriggerGameOver();
            enabled = false;
        }
    }

    private bool PlayerLost()
    {
        return player != null && player.CurrentHealth <= 0;
    }

    private void TriggerGameOver()
    {
        Debug.Log("Game Over! Player has died.");
        onGameOver?.Invoke();
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
        enemiesRemaining = maxEnemiesPerWave + (currentWave - 1) * 2;

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

        Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        GameObject enemyPrefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
        Vector3 adjustedSpawnPos = spawnPoint.position + new Vector3(UnityEngine.Random.Range(-spawnOffset, spawnOffset), 0, UnityEngine.Random.Range(-spawnOffset, spawnOffset));
        GameObject enemy = Instantiate(enemyPrefab, adjustedSpawnPos, Quaternion.identity);

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
        killCount++;
        killCountText.text = "Kills: " + killCount;
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
        onVictory?.Invoke();
    }

    void OnPlayerDeath()
    {
        onGameOver?.Invoke();
    }
}
