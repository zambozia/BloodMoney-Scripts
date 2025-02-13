using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; 

public class VictoryManager : MonoBehaviour
{
    public GameObject victoryCanvas;       // Reference to the Victory UI Canvas
    public Animator playerAnimator;        // Reference to the Player's Animator
    public AudioClip victorySound;         // Reference to Victory Sound
    public AudioClip selectionChangeSound; // Sound to play when changing button selection
    public CanvasGroup victoryCanvasGroup; // Reference to the Canvas Group
    public GameObject retryPanel;          // Panel for retry and quit options
    public GameObject firstSelectedButton; // First button selected when UI appears
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        // Automatically select the first button when the Victory screen is enabled
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    public void ShowVictoryScreen()
    {
        // Display Victory UI
        victoryCanvas.SetActive(true);
        FadeInVictoryCanvas();

        // Disable player input
        var playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }

        // Play victory sound
        if (victorySound != null)
        {
            audioSource.PlayOneShot(victorySound);
        }

        // Trigger victory animation
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("Victory");
        }

        // Show retry options
        ShowRetryOptions();
        Debug.Log("Victory! Animation triggered.");

        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    private void ShowRetryOptions()
    {
        // Activate retry panel
        retryPanel.SetActive(true);
    }

    public void RestartGame()
    {
        // Fade out, then restart
        StartCoroutine(RestartWithFade());
    }

    private IEnumerator RestartWithFade()
    {
        FadeOutVictoryCanvas();
        yield return new WaitForSeconds(1f); // Wait for fade out to complete
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Button Pressed");
        Debug.Log("QuitGame() method called.");
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    public void PlaySelectionChangeSound()
    {
        // Play sound when navigating between UI elements
        if (selectionChangeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(selectionChangeSound);
            Debug.Log("Selection sound played.");
        }
    }

    private void FadeInVictoryCanvas()
    {
        StartCoroutine(FadeCanvasGroup(victoryCanvasGroup, 0f, 1f, 1f));
    }

    private void FadeOutVictoryCanvas()
    {
        StartCoroutine(FadeCanvasGroup(victoryCanvasGroup, 1f, 0f, 1f));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = endAlpha;
    }
}
