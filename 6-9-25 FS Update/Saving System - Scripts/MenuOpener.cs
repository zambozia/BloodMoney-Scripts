using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS_ThirdPerson
{
    public class MenuOpener : MonoBehaviour
    {
        [SerializeField] MenuSelectionUI menuUI;
        [SerializeField] bool menuPausesGame = true;
        [SerializeField] LocomotionInputManager inputManager;

        bool currCursorVisible = false;
        CursorLockMode currCursorLockMode;

        private void Awake()
        {
            if (inputManager == null)
                inputManager = FindObjectOfType<LocomotionInputManager>();
        }

        private void Start()
        {

        }

        public void LateUpdate()
        {
            if (inputManager.Back)
            {
                if (!menuUI.gameObject.activeSelf)
                {
                    // If already paused by some other UI then return
                    if (Time.deltaTime == 0) return;

                    currCursorLockMode = Cursor.lockState;
                    currCursorVisible = Cursor.visible;

                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;

                    if (menuPausesGame)
                        Time.timeScale = 0;

                    menuUI.gameObject.SetActive(true);
                    menuUI.OnClose += CloseMenu;
                }
                else
                {
                    CloseMenu();
                }
            }
        }

        void CloseMenu()
        {
            Cursor.visible = currCursorVisible;
            Cursor.lockState = currCursorLockMode;

            if (menuPausesGame)
                Time.timeScale = 1;

            menuUI.OnClose -= CloseMenu;
            menuUI.gameObject.SetActive(false);
        }
    }
}
