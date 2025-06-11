using FS_ThirdPerson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FS_InventorySystem 
{
    public class InventoryOpener : MonoBehaviour
    {
        [SerializeField] GameObject inventoryUI;
        [SerializeField] bool inventoryPausesGame = true;

        bool currCursorVisible = false;
        CursorLockMode currCursorLockMode;

        public event Action OnOpened;
        public event Action OnClosed;

        private void Awake()
        {
            
        }

        private void Start()
        {

        }

        public void Update()
        {
            if (InventorySettings.i.InputManager.OpenInventory)
            {
                // If already paused by some other UI then return
                if (Time.deltaTime == 0) return;

                if (!inventoryUI.activeSelf)
                {
                    currCursorLockMode = Cursor.lockState;
                    currCursorVisible = Cursor.visible;

                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;

                    if (inventoryPausesGame)
                        Time.timeScale = 0;

                    inventoryUI.SetActive(true);

                    OnOpened?.Invoke();
                } 
            }
            else if (InventorySettings.i.InputManager.Back)
            {
                if (inventoryUI.activeSelf)
                {
                    Cursor.visible = currCursorVisible;
                    Cursor.lockState = currCursorLockMode;

                    if (inventoryPausesGame)
                        Time.timeScale = 1;
                    
                    inventoryUI.SetActive(false);

                    OnClosed?.Invoke();
                }
            }
        }
    }
}
