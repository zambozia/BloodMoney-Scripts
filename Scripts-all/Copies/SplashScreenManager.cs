using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreenManager : MonoBehaviour
{
    public CanvasGroup studioLogo;  // The first logo (Studio)
    public CanvasGroup gameLogo;    // The second logo (Game Title)

    public float fadeDuration = 2f;
    public float displayDuration = 3f;
    public string nextScene = "StartMenu";

    private void Start()
    {
        StartCoroutine(PlaySplashScreen());
    }

    private IEnumerator PlaySplashScreen()
    {
        // Ensure both logos start hidden
        studioLogo.alpha = 0f;
        gameLogo.alpha = 0f;
        studioLogo.gameObject.SetActive(false);
        gameLogo.gameObject.SetActive(false);

        // Fade in Studio Logo
        studioLogo.gameObject.SetActive(true);
        yield return StartCoroutine(FadeIn(studioLogo));
        yield return new WaitForSeconds(displayDuration);
        yield return StartCoroutine(FadeOut(studioLogo));

        // Fade in Game Logo
        gameLogo.gameObject.SetActive(true);
        yield return StartCoroutine(FadeIn(gameLogo));
        yield return new WaitForSeconds(displayDuration);
        yield return StartCoroutine(FadeOut(gameLogo));

        // Transition to Game Menu Scene
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(nextScene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find the Game Menu Canvas
        GameObject menuCanvas = GameObject.Find("GameMenuCanvas");
        if (menuCanvas != null)
        {
            Debug.Log("GameMenuCanvas found, enabling it...");

            // Enable the GameMenuCanvas
            menuCanvas.SetActive(true);

            // Ensure Canvas Group is fully visible
            CanvasGroup cg = menuCanvas.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }
        else
        {
            Debug.LogError("GameMenuCanvas not found in the scene!");
        }

        // Unsubscribe from scene load event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private IEnumerator FadeIn(CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 0f;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut(CanvasGroup canvasGroup)
    {
        float elapsedTime = 0f; // Start from 0 instead of fadeDuration
        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration); // Fade out properly
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(false); // Disable logo after fade-out
    }
}
