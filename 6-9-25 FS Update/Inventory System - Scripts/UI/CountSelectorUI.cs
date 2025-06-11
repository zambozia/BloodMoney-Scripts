using FS_Core;
using FS_ThirdPerson;
using FS_Util;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FS_InventorySystem
{
    [FoldoutGroup("Sound Effects", false, "appearSound", "closeSound", "submitSound", "countChangeSound")]
    public class CountSelectorUI : MonoBehaviour
    {
        [SerializeField] TMP_Text promptText;
        [SerializeField] TMP_Text countText;
        [SerializeField] Button decreaseBtn;
        [SerializeField] Button increaseBtn;
        [SerializeField] Button submitBtn;
        [SerializeField] Button backBtn;
        [SerializeField] bool loopSelection = true;

        [SerializeField] AudioClip appearSound;
        [SerializeField] AudioClip closeSound;
        [SerializeField] AudioClip submitSound;
        [SerializeField] AudioClip countChangeSound;

        int _count = 1;
        int _minCount = 1;
        int _maxCount = int.MaxValue;
        Action<int> _onSubmit;
        Action _onBack;
        SelectorBase _parentUI;

        private void Awake()
        {
            increaseBtn.onClick.AddListener(() => IncreaseSelection());
            decreaseBtn.onClick.AddListener(() => DecreaseSelection());
            submitBtn.onClick.AddListener(() => Submit());
            backBtn.onClick.AddListener(() => Back());
        }


        InventoryInputManager inputManager;
        LocomotionInputManager selectionManager;
        private void Start()
        {
            inputManager = InventorySettings.i.InputManager;
            selectionManager = inputManager.GetComponent<LocomotionInputManager>();
        }

        float selectionTimer = 0;

        private void Update()
        {
            if (gameObject.activeSelf)
            {
                var h = selectionManager.NavigationInput.x;

                if (Mathf.Abs(h) > 0.2f && selectionTimer <= 0)
                {
                    if (h < 0)
                        DecreaseSelection();
                    else if (h > 0)
                        IncreaseSelection();

                    selectionTimer = 0.25f;
                }
                if (selectionTimer > 0)
                    selectionTimer -= Time.unscaledDeltaTime;
                
                
                if (inputManager.Select)
                    Submit();
                else if (inputManager.Back)
                    Back();

                countText.text = "" + _count;
            }
        }

        public void Show(string prompt, int defaultCount=1, int minCount=1, int maxCount=int.MaxValue,
            Action<int> onSubmit = null, Action onBack = null, SelectorBase parentUI = null)
        {
            _parentUI = parentUI;
            _parentUI?.PauseSelection();
            gameObject.SetActive(true);

            promptText.text = prompt;
            _onSubmit = onSubmit;
            _onBack = onBack;
            _count = defaultCount; 
            _minCount = minCount;
            _maxCount = maxCount;
        }

        void IncreaseSelection()
        {
            int prevCount = _count;
            if (loopSelection)
            {
                if (_count == _maxCount)
                    _count = _minCount;
                else
                    ++_count;
            }
            else
            {
                _count = Mathf.Clamp(_count + 1, _minCount, _maxCount);
            }

            if (prevCount != _count)
                FSAudioUtil.PlaySfx(countChangeSound);
        }

        void DecreaseSelection()
        {
            int prevCount = _count;
            if (loopSelection)
            {
                if (_count == _minCount)
                    _count = _maxCount;
                else
                    --_count;
            }
            else
            {
                _count = Mathf.Clamp(_count - 1, _minCount, _maxCount);
            }

            if (prevCount != _count)
                FSAudioUtil.PlaySfx(countChangeSound);
        }

        void Submit()
        {
            _onSubmit?.Invoke(_count);
            _parentUI?.UnPauseSelection();
            gameObject.SetActive(false);

            FSAudioUtil.PlaySfx(submitSound);
        }

        void Back()
        {
            _onBack?.Invoke();
            _parentUI?.UnPauseSelection();
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            FSAudioUtil.PlaySfx(appearSound);
        }

        private void OnDisable()
        {
            FSAudioUtil.PlaySfx(closeSound);
        }

        private void OnDestroy()
        {
            submitBtn.onClick.RemoveAllListeners();
            backBtn.onClick.RemoveAllListeners();
            increaseBtn.onClick.RemoveAllListeners();
            decreaseBtn.onClick.RemoveAllListeners();
        }
    }
}
