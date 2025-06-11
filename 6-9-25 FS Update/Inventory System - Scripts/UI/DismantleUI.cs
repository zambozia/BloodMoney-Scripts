using FS_Core;
using FS_Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FS_InventorySystem
{
    public class DismantleUI : MonoBehaviour
    {
        [SerializeField] DismantleItemUI dismantleItemPrefab;
        [SerializeField] Transform dismantleItemsContainer;
        [SerializeField] MessageUI messageUI;
        [SerializeField] InventoryUI inventoryUI;
        [SerializeField] CurrencyUI currencyUI;

        public event Action<Item> OnItemDismantled;

        Wallet wallet;
        Inventory inventory;

        bool initialized = false;
        private IEnumerator Start()
        {
            ClearData();

            yield return null;  // wait for a frame to initialize the inventoryUI from it's start

            inventory = InventorySettings.i.PlayerInventory;
            wallet = InventorySettings.i.PlayerWallet;

            inventory.OnUpdated += UpdateDismantleUI;
            inventoryUI.OnCategoryChanged += (int index) => UpdateDismantleUI();
            inventoryUI.OnSelectionChanged += (int index) => UpdateDismantleUI();
            UpdateDismantleUI();

            inventoryUI.OnItemUsed += Dismantle;

            initialized = true;
        }

        private void OnEnable()
        {
            if (initialized)
                UpdateDismantleUI();
        }

        public void UpdateDismantleUI()
        {
            ClearData();

            var itemToDismantle = inventoryUI.GetItemStackByIndex(inventoryUI.SelectedItem)?.Item;

            if (itemToDismantle == null || !itemToDismantle.canDismantle) return;

            foreach (var item in itemToDismantle.ingredients)
            {
                var obj = Instantiate(dismantleItemPrefab, dismantleItemsContainer);
                obj.SetData(item);
            }

            currencyUI.SetData(itemToDismantle.priceToDismantle);
        }

        void ClearData()
        {
            foreach (Transform child in dismantleItemsContainer)
                Destroy(child.gameObject);

            currencyUI.ClearData();
        }

        public void Dismantle(ItemStack itemStack)
        {
            var itemToDismantle = itemStack.Item;

            if (!itemToDismantle.canDismantle) return;

            string errorMessage = null;
            if (!wallet.HasCurrency(itemToDismantle.priceToCraft))
                errorMessage = "Not enought cash";
            else if (itemToDismantle.ingredients.Any(i => !inventory.CanAddItem(i.Item, i.Count)))
                errorMessage = "No space in the Inventory";

            if (errorMessage != null)
            {
                messageUI?.Show(errorMessage);
                return;
            }

            inventory.RemoveItem(itemToDismantle, inventoryUI.SelectedItem);
            wallet.TakeCurrency(itemToDismantle.priceToCraft);
            itemToDismantle.Ingredients.ForEach(i => inventory.AddItem(i.Item, i.Count));

            messageUI?.Show($"{itemToDismantle.Name} Dismantled"!, MessageBoxType.Positive);

            OnItemDismantled?.Invoke(itemToDismantle);
        }
    }

}
