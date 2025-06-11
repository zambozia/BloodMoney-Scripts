using FS_Core;
using FS_ThirdPerson;
using FS_Util;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FS_InventorySystem {

    [FoldoutGroup("Other References", false, "itemDetailsUI", "messageUI", "confirmationUI", "eventSystem", "graphicRaycaster")]
    [FoldoutGroup("Advanced Settings", false, "canUseItems", "canConsumeItems", "canDropItems",  "addEntireRowOnOverflow", "onlyShowItemsToDismantle")]
    public class InventoryUI : SelectionUI<ImageSlot>
    {
        [SerializeField] bool usePlayerInventory = false;

        [ShowIf("usePlayerInventory", false)]
        [SerializeField] Inventory inventory;
        [SerializeField] int defaultNumberOfSlots = 49;

        [SerializeField] InventorySlotUI inventorySlotUIPrefab;
        [SerializeField] Transform itemSlotsContainer;

        [Space(10)]

        [SerializeField] bool showAsCategories = true;
        [ShowIf("showAsCategories", true)]
        [SerializeField] TMP_Text itemCategoryText;
        [ShowIf("showAsCategories", true)]
        [SerializeField] Button nextCategoryButton;
        [ShowIf("showAsCategories", true)]
        [SerializeField] Button prevCategoryButton;

        [Space(10)]

        [SerializeField] bool allowDraggingItems = true;
        [ShowIf("allowDraggingItems", true)]
        [SerializeField] Image dragImage;

        [Space(10)]

        [SerializeField] bool compareEquippedAttributes = true;
        [SerializeField] List<EquipmentSlot> ignoreEquipmentSlots = new List<EquipmentSlot>();

        // Other References
        [SerializeField] ItemDetailsUI itemDetailsUI;
        [SerializeField] MessageUI messageUI;
        [SerializeField] ConfirmationUI confirmationUI;
        [SerializeField] GraphicRaycaster graphicRaycaster;

        // Advanced Settings
        [SerializeField] bool canUseItems = true;
        [SerializeField] bool canConsumeItems = true;
        [SerializeField] bool canDropItems = true;
        [SerializeField] bool addEntireRowOnOverflow = true;
        [SerializeField] bool onlyShowItemsToDismantle = false;

        // allows to modify the price when showing item details (For example in shop UI, sell price will be lesser than actual price)
        public float DisplayPriceModifier { get; set; } = 1f;

        public Inventory Inventory => inventory;

        List<InventorySlotUI> itemUISlots;

        public bool IsDraggingItem { get; private set; } = false;
        int categoryIndex = 0;
        ItemSlotUI draggingSlot;
        int draggingSlotIndex = 0;

        public event Action<int> OnCategoryChanged;
        public event Action<ItemStack> OnItemUsed;
        public event Action<ItemStack> OnItemConsumed;
        public event Action<ItemStack> OnItemDropped;
        public event Action<Item> OnUnEquip;

        // Dragged item will be ItemSlot, but it can be dropped to any slot (like equipment slots)
        public event Action<InventorySlotUI, ItemSlotUI> OnItemMoved;

        RectTransform itemSlotRectTransform;
        InventoryInputManager inputManager;

        bool initialized = false;
        private void Start()
        {
            if (usePlayerInventory)
                inventory = InventorySettings.i.PlayerInventory;

            inputManager = InventorySettings.i.InputManager;

            foreach (Transform child in itemSlotsContainer)
                Destroy(child.gameObject);

            itemUISlots = new List<InventorySlotUI>();
            for (int i = 0; i < defaultNumberOfSlots; i++)
            {
                AddInventorySlot(i);
            }

            var imageSlots = itemUISlots.Select(i => i.ImageSlot).ToList();
            Init();
            SetItems(imageSlots);

            inventory.OnUpdated += UpdateInventoryUI;
            UpdateInventoryUI();

            if (showAsCategories)
            {
                prevCategoryButton.onClick.AddListener(() => GoToPrevCategory());
                nextCategoryButton.onClick.AddListener(() => GoToNextCategory());
            }

            itemSlotRectTransform = itemSlotsContainer.GetComponent<RectTransform>();

            OnSelected += OnItemSelected;
            OnSelectionChanged += SelectedItemChanged;
            OnHoverChanged += HoveredItemChanged;

            if (IsInFocus && InventorySettings.i.ShowDetailsOfSelectedItem)
                StartCoroutine(AsyncUtil.RunAfterFrames(1, () => ShowDetailsOfItem(selectedItem)));

            initialized = true;
        }

        private void OnEnable()
        {
            // To update inventory UI when it's reopened
            if (initialized)
            {
                inventory.OnUpdated += UpdateInventoryUI;
                UpdateInventoryUI();
            }
        }

        private void Update()
        {
            if (!IsInFocus || SelectionPaused) return;

            HandleUpdate();

            if (showAsCategories)
            {
                if (inputManager.PreviousCategory)
                    GoToPrevCategory();
                else if (inputManager.NextCategory)
                    GoToNextCategory();
            }

            if (inputManager.QuickSort)
            {
                QuickSort();
                return;
            }

            var itemStack = selectedItem < itemUISlots.Count && selectedItem >= 0? itemUISlots[selectedItem].ItemStack : null;
            var item = itemStack?.Item;

            if (item != null)
            {
                if (inputManager.Select && canUseItems)
                {
                    OnItemUsed?.Invoke(itemStack);
                }
                else if (inputManager.Consume && canConsumeItems)
                {
                    if (item.Consumable)
                    {
                        inventory.ConsumeItem(item, selectedItem);
                        OnItemConsumed?.Invoke(itemStack);
                    }
                }
                else if (inputManager.Drop && canDropItems)
                {
                    if (item.Droppable)
                    {
                        confirmationUI.Show("Are you sure you want to drop this item?", parentUI: this,
                            onSubmit: () =>
                            {
                                inventory.RemoveItemStack(itemStack, selectedItem);
                                OnItemDropped?.Invoke(itemStack);
                            });
                    }
                }
            }
        }

        public ItemStack GetItemStackByIndex(int selection)
        {
            return selection < itemUISlots.Count? itemUISlots[selection]?.ItemStack : null;
        }

        public InventorySlotUI GetSelectedItemUISlot()
        {
            return itemUISlots[selectedItem];
        }

        void GoToPrevCategory()
        {
            if (IsDraggingItem) return;

            categoryIndex = (categoryIndex > 0) ? categoryIndex - 1 : inventory.CategorySlots.Count - 1;
            UpdateInventoryUI();
            OnCategoryChanged?.Invoke(categoryIndex);
        }

        void GoToNextCategory()
        {
            if (IsDraggingItem) return;

            categoryIndex = (categoryIndex + 1) % inventory.CategorySlots.Count;
            UpdateInventoryUI();
            OnCategoryChanged?.Invoke(categoryIndex);
        }

        public void UpdateInventoryUI()
        {
            var itemStacks = showAsCategories ? inventory.CategorySlots[categoryIndex].itemSlots : inventory.GetAllItemStacks();
            if (onlyShowItemsToDismantle)
                itemStacks = itemStacks.Where(s => s != null && s.Item.canDismantle).ToList();

            AddMoreSlotsIfRequired(itemStacks);
            RemoveUnusedSlots(itemStacks);

            for (int i = 0; i < itemUISlots.Count; i++)
            {
                if (i < itemStacks.Count && itemStacks[i] != null)
                    itemUISlots[i].SetData(itemStacks[i]);
                else
                    itemUISlots[i].ClearData();
            }

            UpdateSelection();

            if (showAsCategories)
                itemCategoryText.text = inventory.CategorySlots[categoryIndex].Name;
        }

        void AddMoreSlotsIfRequired(List<ItemStack> itemStacks)
        {
            if (itemStacks != null || itemUISlots != null) return;

            if (itemStacks.Count > itemUISlots.Count)
            {
                int requiredSlots = itemStacks.Count;
                if (addEntireRowOnOverflow)
                    requiredSlots = (int)Mathf.Ceil((float)requiredSlots / numberOfColumns) * numberOfColumns;

                for (int i = itemUISlots.Count; i < requiredSlots; i++)
                {
                    AddInventorySlot(i);
                }

                SetItems(itemUISlots.Select(i => i.ImageSlot).ToList());
            }
        }

        void RemoveUnusedSlots(List<ItemStack> itemStacks)
        {
            int requiredSlots = FindLastItemIndex(itemStacks) + 1;

            if (addEntireRowOnOverflow)
                requiredSlots = (int)Mathf.Ceil((float)requiredSlots / numberOfColumns) * numberOfColumns;

            requiredSlots = Mathf.Max(requiredSlots, defaultNumberOfSlots);

            if (itemUISlots.Count > requiredSlots)
            {
                for (int i = itemUISlots.Count-1; i >= requiredSlots; i--)
                {
                    Destroy(itemUISlots[i].gameObject);
                    itemUISlots.RemoveAt(i);
                }

                if (itemStacks.Count > requiredSlots)
                    inventory.RemoveSlotsTillIndex(requiredSlots - 1, categoryIndex);


                SetItems(itemUISlots.Select(i => i.ImageSlot).ToList());
            }
        }

        int FindLastItemIndex(List<ItemStack> itemStacks)
        {
            int lastItemIndex = itemStacks.Count;
            for (int i = itemUISlots.Count - 1; i >= 0; i--)
            {
                if (i < itemStacks.Count && itemStacks[i] != null)
                {
                    lastItemIndex = i;
                    break;
                }
            }

            return lastItemIndex;
        }

        void AddInventorySlot(int slotIndex)
        {
            var slot = Instantiate(inventorySlotUIPrefab, itemSlotsContainer);
            if (allowDraggingItems)
                slot.Init(OnDragStarted, OnDragEnded, dragImage, itemDetailsUI, slotIndex);
            else
                slot.Init(itemDetailsUI, slotIndex);
            itemUISlots.Add(slot);
        }

        void QuickSort()
        {
            inventory.RemoveEmptySlots(categoryIndex);
            RemoveUnusedSlots(inventory.GetSlotsByCategory(categoryIndex));
            UpdateInventoryUI();
        }

        void OnItemSelected(int selection)
        {
            
        }

        void SelectedItemChanged(int selection)
        {
            if (!IsInFocus) return;

            if (!InventorySettings.i.ShowDetailsOfSelectedItem) return;

            ShowDetailsOfItem(selection);
        }

        void HoveredItemChanged(int hoveredItem)
        {
            if (InventorySettings.i.ShowDetailsOfSelectedItem) return;

            ShowDetailsOfItem(hoveredItem);
        }

        void ShowDetailsOfItem(int index)
        {
            bool isDraggingInAnySelector = allowDraggingItems && dragImage.gameObject.activeSelf;
            if (!isDraggingInAnySelector && index >= 0 && index < itemUISlots.Count && itemUISlots[index]?.ItemStack?.Item != null)
            {
                Item equippedItem = null;
                var item = itemUISlots[index].ItemStack.Item;
                if (compareEquippedAttributes && item.Equippable && !ignoreEquipmentSlots.Any(e => e.equipmentSlotIndex == item.EquipmentSlotIndex))
                {
                    equippedItem = inventory.GetEquippedItem(item.EquipmentSlotIndex);
                }

                itemDetailsUI?.Show(item, itemUISlots[index].transform, equippedItem, DisplayPriceModifier);
            }
            else
                itemDetailsUI?.Hide();
        }

        void OnDragStarted(ItemSlotUI draggedSlot)
        {
            IsDraggingItem = true;
            draggingSlot = draggedSlot;
            draggingSlotIndex = selectedItem;
            itemDetailsUI?.Hide();
        }

        void OnDragEnded()
        {
            IsDraggingItem = false;

            var destSlot = ItemSlotUI.FindSlotAtMousePos(graphicRaycaster);
            if (destSlot != null && destSlot.CanDropItems)
            {
                if (destSlot is InventorySlotUI) // Dragged to an item slot
                {
                    OnItemDraggedToInventorySlot(draggingSlot, destSlot);
                }

                OnItemMoved?.Invoke(draggingSlot as InventorySlotUI, destSlot);

                destSlot.ShowItemDetails();
            }
            else
            {
                // Dragged to an empty are so reset the selection
                selectedItem = draggingSlotIndex;
                base.UpdateSelection();
            }
        }

        // This function will called when items outside the Inventory will dragged to the inventory
        public void OnItemDraggedToInventorySlot(ItemSlotUI sourceSlot, ItemSlotUI destSlot)
        {
            if (!inventory.CategorySlots[categoryIndex].categories.Contains(sourceSlot.ItemStack.Item.Category))
            {
                messageUI?.Show("Invalid Category");
                return;
            }

            PutItemInInventorySlot(sourceSlot, destSlot as InventorySlotUI);
        }

        void PutItemInInventorySlot(ItemSlotUI sourceSlot, InventorySlotUI destSlot)
        {
            if (sourceSlot == destSlot) return;

            var tempSlot = destSlot.ItemStack;

            inventory.RemoveWholeStack(destSlot.Index, categoryIndex);
            inventory.AddItemStack(sourceSlot.ItemStack, destSlot.Index);
            ChangeSelection(destSlot.Index);

            if (sourceSlot is InventorySlotUI)
            {
                // Inventory Slot -> Inventory Slot

                inventory.RemoveWholeStack(sourceSlot.Index, categoryIndex);

                if (tempSlot != null)
                    inventory.AddItemStack(tempSlot, sourceSlot.Index);
            }
            else
            {
                // Equipment Slot -> Inventory Slot
                OnUnEquip?.Invoke(sourceSlot.ItemStack.Item);

                inventory.RemoveItemFromEquipmentSlot(sourceSlot.Index);

                if (tempSlot != null)
                    inventory.SetItemToEquipmentSlot(tempSlot, sourceSlot.Index);
            }
        }

        public void SetInventory(Inventory inventory)
        {
            this.inventory = inventory;
        }

        public Item GetSelectedItem()
        {
            return itemUISlots[selectedItem]?.ItemStack?.Item;
        }

        public void InvokeUnEquipEvent(Item item)
        {
            OnUnEquip?.Invoke(item);
        }

        private void OnDisable()
        {
            Disable();

            if (inventory != null)
                inventory.OnUpdated -= UpdateInventoryUI;

            itemDetailsUI?.gameObject?.SetActive(false);
            messageUI?.gameObject?.SetActive(false);
            dragImage?.gameObject?.SetActive(false);
        }
    }
}
