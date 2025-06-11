using FS_Core;
using FS_Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FS_InventorySystem {

    public class CraftingUI : SelectionUI<ImageSlot>
    {
        [SerializeField] CraftItemUI craftItemPrefab;
        [SerializeField] Transform craftItemsContainer;

        [SerializeField] Image craftItemImage;
        [SerializeField] Transform ingredientsContainer;
        [SerializeField] IngredientsUI ingredientsUI;
        [SerializeField] IngredientSlotUI ingredientSlotUI;

        [SerializeField] ItemDetailsUI itemDetailsUI;
        [SerializeField] CurrencyUI currencyUI;

        [SerializeField] MessageUI messageUI;

        [Tooltip("If you want to use the Crafting UI without talking to a merchant, you can set the default merchant.")]
        [SerializeField] Merchant defaultMerchant;

        Merchant merchant;

        List<CraftItemUI> craftUISlots;

        public event Action<Item> OnItemCrafted;


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

            UpdateCraftingItemsUI();
            SetItems(craftUISlots.Select(i => i.GetComponent<ImageSlot>()).ToList());
            Init();
        }

        bool firstEnable = true;
        private void OnEnable()
        {
            if (!firstEnable)
                UpdateSelection();
            else
                firstEnable = false;
        }

        void UpdateCraftingItemsUI()
        {
            craftUISlots = new List<CraftItemUI>();

            // Destroy old craft items
            foreach (Transform child in craftItemsContainer)
                Destroy(child.gameObject);

            foreach (var item in merchant.CraftableItems)
            {
                if (!item.canCraft) continue;

                var obj = Instantiate(craftItemPrefab, craftItemsContainer);
                obj.SetData(item);
                craftUISlots.Add(obj);
            }
        }

        private void Update()
        {
            if (!IsInFocus) return;

            HandleUpdate();

            if (inputManager.Select)
            {
                var itemToCraft = merchant.CraftableItems[selectedItem];
                if (itemToCraft != null)
                {
                    string errorMessage = null;
                    if (!wallet.HasCurrency(itemToCraft.priceToCraft))
                        errorMessage = "Not enough cash";
                    else if (!playerInventory.CanAddItem(itemToCraft))
                        errorMessage = "No space in the Inventory";
                    else if (!playerInventory.HasRequiredItemsCount(itemToCraft.ingredients))
                        errorMessage = "Ingredients Missing";

                    if (errorMessage != null)
                    {
                        messageUI?.Show(errorMessage);
                        return;
                    }

                    playerInventory.AddItem(itemToCraft);
                    wallet.TakeCurrency(itemToCraft.priceToCraft);
                    itemToCraft.Ingredients.ForEach(i => playerInventory.RemoveItem(i.Item, i.Count));

                    messageUI?.Show($"{itemToCraft.Name} Crafted"!, MessageBoxType.Positive);
                    OnItemCrafted?.Invoke(itemToCraft);

                    UpdateSelection();
                }
            }
        }

        public override void UpdateSelection()
        {
            base.UpdateSelection();

            if (selectedItem < 0) return;

            var item = merchant.CraftableItems[selectedItem];

            craftItemImage.sprite = item.Icon;

            itemDetailsUI?.SetData(item);

            // Show Ingredients
            // Destroy old ingredients
            foreach (Transform child in ingredientsContainer)
                Destroy(child.gameObject);

            var ingredientsUISlots = new List<IngredientSlotUI>();
            foreach (var ingredient in item.Ingredients)
            {
                var ingredientObj = Instantiate(ingredientSlotUI, ingredientsContainer);
                ingredientObj.SetData(ingredient, playerInventory);
                ingredientsUISlots.Add(ingredientObj);
            }
            ingredientsUI.SetIngredientsSlots(ingredientsUISlots);

            currencyUI.SetData(item.priceToCraft);
        }

        public void SetMerchant(Merchant merchant)
        {
            this.merchant = merchant;
        }

        private void OnDisable()
        {
            messageUI?.gameObject?.SetActive(false);

            base.Disable();
        }
    }

}
