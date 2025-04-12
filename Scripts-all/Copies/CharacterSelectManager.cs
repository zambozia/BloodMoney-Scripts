using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("Character Models")]
    public GameObject[] characterPrefabs;
    public Transform[] spawnPoints;

    [Header("UI Elements")]
    public Button leftButton;
    public Button rightButton;
    public Button selectButton;
    public Button backButton;
    public GameObject confirmPopup;
    public Button confirmYesButton;
    public Button confirmNoButton;

    [Header("Input System")]
    public InputActionAsset inputAsset;

    [Header("Audio")]
    public AudioClip clickSound;
    public AudioClip confirmSound;
    private AudioSource audioSource;

    private int currentIndex = 0;
    private GameObject currentCharacter;
    private Animator currentAnimator;
    private bool isBackConfirmation = false;
    private bool popupJustOpened = false;

    private float lastNavigateTime = 0f;
    private float navigateCooldown = 0.3f;

    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;

    private void Awake()
    {
        ShowCharacter(currentIndex);
        audioSource = GetComponent<AudioSource>();

        var uiMap = inputAsset.FindActionMap("UI");
        navigateAction = uiMap.FindAction("Navigate");
        submitAction = uiMap.FindAction("Submit");
        cancelAction = uiMap.FindAction("Cancel");
    }

    private void OnEnable()
    {
        leftButton.onClick.AddListener(OnLeftClick);
        rightButton.onClick.AddListener(OnRightClick);
        selectButton.onClick.AddListener(OnSelectClick);
        backButton.onClick.AddListener(OnBackClick);
        confirmYesButton.onClick.AddListener(OnConfirmYes);
        confirmNoButton.onClick.AddListener(OnConfirmNo);

        navigateAction.Enable();
        submitAction.Enable();
        cancelAction.Enable();

        navigateAction.performed += OnNavigate;
        submitAction.performed += OnSubmit;
        cancelAction.performed += OnCancel;

        EventSystem.current.SetSelectedGameObject(selectButton.gameObject);
    }

    private void OnDisable()
    {
        leftButton.onClick.RemoveAllListeners();
        rightButton.onClick.RemoveAllListeners();
        selectButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
        confirmYesButton.onClick.RemoveAllListeners();
        confirmNoButton.onClick.RemoveAllListeners();

        navigateAction.performed -= OnNavigate;
        submitAction.performed -= OnSubmit;
        cancelAction.performed -= OnCancel;
    }

    private void OnNavigate(InputAction.CallbackContext context)
    {
        if (confirmPopup.activeSelf) return;
        if (Time.time - lastNavigateTime < navigateCooldown) return;

        Vector2 nav = context.ReadValue<Vector2>();
        if (Mathf.Abs(nav.x) > 0.5f)
        {
            if (nav.x < 0) OnLeftClick();
            else if (nav.x > 0) OnRightClick();
            lastNavigateTime = Time.time;
        }
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (popupJustOpened) return;

        if (confirmPopup.activeSelf)
        {
            EventSystem.current.currentSelectedGameObject?.GetComponent<Button>()?.onClick.Invoke();
        }
        else
        {
            selectButton.onClick.Invoke();
        }
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        if (popupJustOpened) return;

        if (confirmPopup.activeSelf)
        {
            confirmNoButton.onClick.Invoke();
        }
        else
        {
            backButton.onClick.Invoke();
        }
    }

    void OnLeftClick()
    {
        PlayClick();
        currentIndex = (currentIndex - 1 + characterPrefabs.Length) % characterPrefabs.Length;
        ShowCharacter(currentIndex);
    }

    void OnRightClick()
    {
        PlayClick();
        currentIndex = (currentIndex + 1) % characterPrefabs.Length;
        ShowCharacter(currentIndex);
    }

    void OnSelectClick()
    {
        PlayClick();
        isBackConfirmation = false;
        confirmPopup.SetActive(true);
        popupJustOpened = true;
        StartCoroutine(ResetPopupFlag());
        EventSystem.current.SetSelectedGameObject(confirmYesButton.gameObject);
    }

    void OnBackClick()
    {
        PlayClick();
        isBackConfirmation = true;
        confirmPopup.SetActive(true);
        popupJustOpened = true;
        StartCoroutine(ResetPopupFlag());
        EventSystem.current.SetSelectedGameObject(confirmYesButton.gameObject);
    }

    void OnConfirmYes()
    {
        PlayConfirm();
        PlayerPrefs.SetInt("SelectedCharacterIndex", currentIndex);

        if (isBackConfirmation)
            SceneLoader.LoadScene("MenuUI");
        else
            SceneLoader.LoadScene("TestCombatArea");
    }

    void OnConfirmNo()
    {
        PlayClick();
        confirmPopup.SetActive(false);
        EventSystem.current.SetSelectedGameObject(selectButton.gameObject);
    }

    void ShowCharacter(int index)
    {
        if (currentCharacter != null)
            Destroy(currentCharacter);

        currentCharacter = Instantiate(characterPrefabs[index], spawnPoints[index].position, spawnPoints[index].rotation);
        currentAnimator = currentCharacter.GetComponent<Animator>();

        if (currentAnimator != null)
            StartCoroutine(PlayRandomAttackThenIdle());
    }

    IEnumerator PlayRandomAttackThenIdle()
    {
        yield return new WaitForSeconds(0.5f);
        int attackIndex = UnityEngine.Random.Range(1, 4);
        string triggerName = "Attack" + attackIndex;

        if (currentAnimator != null)
        {
            currentAnimator.ResetTrigger("Attack1");
            currentAnimator.ResetTrigger("Attack2");
            currentAnimator.ResetTrigger("Attack3");
            currentAnimator.SetTrigger(triggerName);
        }
    }

    IEnumerator ResetPopupFlag()
    {
        yield return new WaitForSeconds(0.3f);
        popupJustOpened = false;
    }

    void PlayClick()
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }

    void PlayConfirm()
    {
        if (audioSource != null && confirmSound != null)
            audioSource.PlayOneShot(confirmSound);
    }
}
