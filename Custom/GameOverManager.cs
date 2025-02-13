using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverCanvas;    // Reference to the Game Over Canvas
    public CanvasGroup gameOverCanvasGroup;
    public GameObject retryPanel;       // Panel for retry buttons
    public AudioClip gameOverSound;     // Sound to play on game over
    public AudioClip selectionChangeSound; // Sound to play on UI selection change
    public GameObject firstSelectedButton;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (!audioSource)
        {
            Debug.LogError("AudioSource missing from LoseManager GameObject!");
        }
    }

    public void ShowGameOverScreen()
    {
        // Activate Game Over UI
        gameOverCanvas.SetActive(true);
        retryPanel.SetActive(true);

        // Fade in the Game Over Canvas
        StartCoroutine(FadeCanvasGroup(gameOverCanvasGroup, 0, 1, 0.5f));

        // Play Game Over Sound
        if (gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }

        // Select the first button
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    public void PlaySelectionChangeSound()
    {
        if (selectionChangeSound != null)
        {
            audioSource.PlayOneShot(selectionChangeSound);
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float start, float end, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = end;
    }

    public void RestartGame()
    {
        // Logic to restart the game
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Debug.Log("QuitGame() method called.");
        Application.Quit();
    }
}
