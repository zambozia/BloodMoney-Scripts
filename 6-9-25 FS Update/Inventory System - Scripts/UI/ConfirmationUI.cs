using FS_Core;
using FS_Util;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FS_InventorySystem
{
    [FoldoutGroup("Sound Effects", false, "appearSound", "closeSound", "submitSound")]
    public class ConfirmationUI : MonoBehaviour
    {
        [SerializeField] TMP_Text promptText;
        [SerializeField] Button submitBtn;
        [SerializeField] Button backBtn;

        [SerializeField] AudioClip appearSound;
        [SerializeField] AudioClip closeSound;
        [SerializeField] AudioClip submitSound;

        Action _onSubmit;
        Action _onBack;
        SelectorBase _parentUI;

        private void Awake()
        {
            submitBtn.onClick.AddListener(() => Submit());
            backBtn.onClick.AddListener(() => Back());
        }

        private void Start()
        {

        }

        private void OnEnable()
        {
            FSAudioUtil.PlaySfx(appearSound);
        }

        private void Update()
        {
            if (gameObject.activeSelf)
            {
                if (InventorySettings.i.InputManager.Select)
                    Submit();
                else if (InventorySettings.i.InputManager.Back)
                    Back();
            }
        }

        public void Show(string prompt, Action onSubmit = null, Action onBack = null, SelectorBase parentUI = null)
        {
            _parentUI = parentUI;
            _parentUI?.PauseSelection();

            gameObject.SetActive(true);

            promptText.text = prompt;
            _onSubmit = onSubmit;
            _onBack = onBack;
        }

        void Submit()
        {
            _onSubmit?.Invoke();
            _parentUI?.UnPauseSelection();
            gameObject.SetActive(false);

            FSAudioUtil.PlaySfx(submitSound);
        }

        void Back()
        {
            _onBack?.Invoke();
            _parentUI?.UnPauseSelection();
            gameObject.SetActive(false);

            FSAudioUtil.PlaySfx(closeSound);
        }

        private void OnDestroy()
        {
            submitBtn.onClick.RemoveAllListeners();
            backBtn.onClick.RemoveAllListeners();
        }
    }
}
