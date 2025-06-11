using FS_Core;
using FS_Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS_InventorySystem
{
    public class ShopUI : MonoBehaviour
    {
        [SerializeField] InventoryUI playerInventoryUI;
        [SerializeField] InventoryUI merchantInventoryUI;
        [SerializeField] MessageUI messageUI;
        [SerializeField] CountSelectorUI countSelectorUI;

        [Tooltip("Useful for testing shop without speaking to a merchant")]
        [SerializeField] Merchant defaultMerchant;

        public event Action<Item, int> OnBuy;
        public event Action<Item, int> OnSell;

        Merchant merchant;
        Inventory playerInventory;
        Wallet wallet;
        InventoryInputManager inputManager;


        private void Start()
        {
            playerInventory = InventorySettings.i.PlayerInventory;
            wallet = InventorySettings.i.PlayerWallet;
            inputManager = InventorySettings.i.InputManager;

            if (merchant == null)
                merchant = defaultMerchant;

            playerInventoryUI.OnItemUsed += TryToSell;
            merchantInventoryUI.OnItemUsed += TryToBuy;
        }

        private void Update()
        {
            if (gameObject.activeSelf && inputManager.Back)
            {
                gameObject.SetActive(false);
            } 
        }

        public void SetMerchant(Merchant merchant)
        {
            this.merchant = merchant;
            merchantInventoryUI.SetInventory(merchant.Inventory);
            playerInventoryUI.DisplayPriceModifier = merchant.SellPriceModifier;
        }

        void TryToBuy(ItemStack itemStack)
        {
            if (itemStack.Count > 1)
            {
                countSelectorUI.Show("How many would you like to buy?", maxCount: itemStack.Count, parentUI: merchantInventoryUI,
                    onSubmit: (int count) =>
                    {
                        BuyItem(itemStack, count);
                    });
            }
            else
                BuyItem(itemStack, 1);
        }

        void BuyItem(ItemStack itemStack, int count)
        {
            var item = itemStack.Item;
            var currencyToTake = new CurrencyAmount(item.price.amount * count, item.price.currencyIndex);

            if (wallet.HasCurrency(currencyToTake))
            {
                if (playerInventory.CanAddItem(itemStack.Item, count))
                {
                    playerInventory.AddItem(itemStack.Item, count);
                    merchant.Inventory.RemoveItem(itemStack.Item, count);
                    wallet.TakeCurrency(currencyToTake);

                    OnBuy?.Invoke(itemStack.Item, itemStack.Count);
                }
                else
                    messageUI?.Show("No space in the Inventory");
            }
            else
                messageUI?.Show("Not enough cash");
        }

        void TryToSell(ItemStack itemStack)
        {
            if (itemStack.Count > 1)
            {
                countSelectorUI.Show("How many would you like to sell?", maxCount: itemStack.Count, parentUI: playerInventoryUI,
                    onSubmit: (int count) =>
                    {
                        SellItem(itemStack.Item, count);
                    });
            }
            else
                SellItem(itemStack.Item, 1);
        }

        public void SellItem(Item item, int count)
        {
            if (!item.Sellable) return;

            playerInventory.RemoveItem(item, count, playerInventoryUI.SelectedItem);
            merchant.Inventory.AddItem(item, count);

            wallet.AddCurrency(new CurrencyAmount(item.price.amount * merchant.SellPriceModifier * count, item.price.currencyIndex));

            OnSell?.Invoke(item, count);
        }

        public Merchant Merchant => merchant;

    }
}