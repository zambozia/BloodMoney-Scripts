using UnityEngine;
using UnityEngine.UI;
using FS_CombatSystem;
using System.Collections;
using static System.Net.Mime.MediaTypeNames;


public class CombatCharacterLoader : MonoBehaviour
{
    [Header("Combat Character Prefabs")]
    public GameObject olderPCPrefab;
    public GameObject youngerPCPrefab;
    public Transform playerSpawnPoint;

    [Header("UI Health Bar References")]
    public PlayerCanvasHealth playerCanvasHealth;
    public UnityEngine.UI.Image healthBarImage;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);

        int selectedIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
        GameObject selectedPrefab = selectedIndex == 0 ? olderPCPrefab : youngerPCPrefab;

        if (selectedPrefab == null)
        {
            Debug.LogError("Selected prefab is null! Make sure prefabs are assigned.");
            yield break;
        }

        GameObject playerInstance = Instantiate(selectedPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);

        if (playerCanvasHealth != null)
        {
            // Assign directly from newly instantiated player
            playerCanvasHealth.meleeFighter = playerInstance.GetComponent<MeleeFighter>();
            playerCanvasHealth.healthManager = playerInstance.GetComponent<HealthManager>();

            if (healthBarImage != null)
            {
               // playerCanvasHealth.healthBarImg = healthBarImage;
            }
        }
        else
        {
            Debug.LogWarning("PlayerCanvasHealth reference not set on CombatCharacterLoader.");
        }

        yield return new WaitForSeconds(0.3f);

        var meleeFighter = playerInstance.GetComponent<MeleeFighter>();
        if (meleeFighter != null)
        {
            meleeFighter.OnDeath += () =>
            {
                GameOverManager gameOverManager = FindObjectOfType<GameOverManager>();
                if (gameOverManager != null)
                    gameOverManager.ShowGameOverScreen();
            };
        }
    }
}
