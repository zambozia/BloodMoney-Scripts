using FS_Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS_InventorySystem
{
    public class ShopOpener : MonoBehaviour
    {
        [SerializeField] UISwitcher uiSwitcher;
        [SerializeField] ShopUI shopUI;
        [SerializeField] CraftingUI craftingUI;

        bool currCursorVisible = false;
        CursorLockMode currCursorLockMode;

        public event Action OnOpened;
        public event Action OnClosed;

        private void Awake()
        {

        }

        private void OnEnable()
        {
            
        }

        public void OpenShop(Merchant merchant)
        {
            gameObject.SetActive(true);

            currCursorLockMode = Cursor.lockState;
            currCursorVisible = Cursor.visible;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (shopUI != null && merchant.CanTrade)
                shopUI.SetMerchant(merchant);

            if (craftingUI != null && merchant.CanCraft)
                craftingUI.SetMerchant(merchant);

            List<string> activePages = new List<string>();
            if (merchant.CanTrade)
                activePages.Add("Shop");
            if (merchant.CanCraft && merchant.CraftableItems.Count(i => i.canCraft) > 0)
                activePages.Add("Crafting");
            if (merchant.CanDismantle)
                activePages.Add("Dismantle");

            uiSwitcher.Init(activePages);

            Time.timeScale = 0;

            OnOpened?.Invoke();
        }

        public void Update()
        {
            
            if (InventorySettings.i.InputManager.Back)
            {
                Cursor.visible = currCursorVisible;
                Cursor.lockState = currCursorLockMode;

                Time.timeScale = 1;
                gameObject.SetActive(false);

                OnClosed?.Invoke();
            }
        }
    }
}
