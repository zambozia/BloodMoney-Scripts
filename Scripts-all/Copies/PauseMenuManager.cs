using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Main Pause UI")]
    public GameObject pauseCanvas;
    public GameObject pausePanel;
    public Button resumeButton;
    public Button mainMenuButton;
    public Button quitButton;

    [Header("Are You Sure? Popup")]
    public GameObject areYouSurePopup;
    public Button yesButton;
    public Button noButton;

    private PlayerInput playerInput;
    private InputAction pauseAction;
    private bool isPaused = false;
    private bool triedFindingInput = false;

    private void Awake()
    {
        TryInitializeInput();
        BindUIButtons();

        pauseCanvas.SetActive(false);
        areYouSurePopup.SetActive(false);
    }

    private void Start()
    {
        // Retry in Start in case PlayerInput spawns late
        if (playerInput == null && !triedFindingInput)
        {
            TryInitializeInput();
        }
    }

    private void Update()
    {
        // Manual fallback (only if Input System failed)
        if (playerInput == null && !triedFindingInput)
        {
            TryInitializeInput();
        }
    }

    private void TryInitializeInput()
    {
        playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null)
        {
            pauseAction = playerInput.actions.FindAction("Pause", true);
            if (pauseAction != null)
                pauseAction.performed += TogglePause;
            else
                Debug.LogWarning("[PauseMenu] 'Pause' action not found in Input Actions.");
        }
        else
        {
            Debug.LogError("[PauseMenu] No PlayerInput found in scene.");
        }

        triedFindingInput = true;
    }

    private void OnDestroy()
    {
        if (pauseAction != null)
            pauseAction.performed -= TogglePause;
    }

    private void BindUIButtons()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
        if (quitButton != null) quitButton.onClick.AddListener(ShowExitConfirmation);
        if (yesButton != null) yesButton.onClick.AddListener(QuitGame);
        if (noButton != null) noButton.onClick.AddListener(CloseExitConfirmation);
    }

    private void TogglePause(InputAction.CallbackContext context)
    {
        if (!isPaused)
            PauseGame();
        else
            ResumeGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseCanvas.SetActive(true);
        pausePanel.SetActive(true);
        areYouSurePopup.SetActive(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseCanvas.SetActive(false);
    }

    public void ShowExitConfirmation()
    {
        pausePanel.SetActive(false);
        areYouSurePopup.SetActive(true);
    }

    public void CloseExitConfirmation()
    {
        pausePanel.SetActive(true);
        areYouSurePopup.SetActive(false);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuUI");
    }

    public void QuitGame()
    {
        UnityEngine.Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
