using FS_Core;
using FS_ThirdPerson;
using FS_Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FS_InventorySystem
{
    public class EquipmentUI : SelectionUI<ImageSlot>
    {
        [SerializeField] List<EquipmentSlotUI> equipmentSlots;
        [SerializeField] InventoryUI inventoryUI;
        [SerializeField] ItemDetailsUI itemDetailsUI;
        [SerializeField] int totalColumns = 2;
        [SerializeField] MessageUI messageUI;
        [SerializeField] Image dragImage;
        [SerializeField] GraphicRaycaster raycaster;

        bool isDraggingItem = false;
        ItemSlotUI draggingSlot;
        int draggingSlotIndex = 0;

        public event Action OnEquipmentsUpdated;
        public event Action<Item> OnEquip;

        public List<EquipmentSlotUI> EquipmentSlots => equipmentSlots;
        public Inventory Inventory => inventory;
        public InventoryUI InventoryUI => inventoryUI;

        bool initialized = false;

        Inventory inventory;

        private void Start()
        {
            inventory = InventorySettings.i.PlayerInventory;

            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                equipmentSlots[i].Init(OnDragStarted, OnDragEnded, dragImage, itemDetailsUI, i);
            }

            var imageSlots = equipmentSlots.Select(s => s.ImageSlot).ToList();
            Init();
            SetItems(imageSlots);

            inventoryUI.OnItemUsed += EquipToFreeSlot;
            inventoryUI.OnItemMoved += OnItemDraggedToEquipmentSlot;

            inventory.OnEquipmentsUpdated += UpdateEquipmentsInUI;
            UpdateEquipmentsInUI();

            OnSelectionChanged += OnSelectedItemChanged;
            OnHoverChanged += OnHoveredItemChanged;

            initialized = true;
        }

        private void OnEnable()
        {
            if (initialized && inventory != null)
            {
                inventory.OnEquipmentsUpdated += UpdateEquipmentsInUI;
                UpdateEquipmentsInUI();
                UpdateSelection();
            }
        }

        void UpdateEquipmentsInUI()
        {
            for (int i = 0; i < inventory.EquipmentSlots.Count; i++)
            {
                var equipmentSlot = inventory.EquipmentSlots[i];
                if (equipmentSlot?.ItemStack != null)
                    equipmentSlots[i].SetData(equipmentSlot.ItemStack);
                else
                    equipmentSlots[i].ClearData();
            }
        }

        private void Update()
        {
            if (!IsInFocus) return;

            HandleUpdate();

            var selectedSlot = selectedItem >= 0 ? equipmentSlots[selectedItem] : null;
            if (selectedSlot?.ItemStack != null && selectedSlot.ItemStack.Item != null)
            {
                var itemSlot = selectedSlot.ItemStack;

                if (InventorySettings.i.ShowDetailsOfSelectedItem)
                    selectedSlot?.ShowItemDetails();

                if (InventorySettings.i.InputManager.Select)
                {
                    if (inventory.CanAddItem(itemSlot.Item, itemSlot.Count))
                    {
                        inventoryUI.InvokeUnEquipEvent(itemSlot.Item);
                        inventory.RemoveItemFromEquipmentSlot(selectedSlot.Index);
                        inventory.AddItemStack(itemSlot);
                    }
                    else
                        messageUI?.Show("No space in the inventory");
                }
            }
        }

        void EquipToFreeSlot(ItemStack itemStack)
        {
            if (!itemStack.Item.Equippable) return;

            var possibleSlots = equipmentSlots.Where(s => s.CanEquipItem(itemStack.Item));
            if (possibleSlots.Count() == 0) return;

            EquipmentSlotUI slotToEquip;
            var freeSlot = possibleSlots.FirstOrDefault(s => s.ItemStack == null);
            if (freeSlot != null)
                slotToEquip = freeSlot;
            else
                slotToEquip = possibleSlots.Last();

            PutItemInEquipmentSlot(inventoryUI.GetSelectedItemUISlot(), slotToEquip);
        }

        void OnDragStarted(ItemSlotUI draggedSlot)
        {
            isDraggingItem = true;
            draggingSlot = draggedSlot;
            draggingSlotIndex = selectedItem;
        }

        void OnDragEnded()
        {
            isDraggingItem = false;

            var destSlot = ItemSlotUI.FindSlotAtMousePos(raycaster);
            if (destSlot != null && destSlot.CanDropItems)
            {
                if (destSlot is EquipmentSlotUI)
                {
                    OnItemDraggedToEquipmentSlot(draggingSlot, destSlot);
                }
                else if (destSlot is InventorySlotUI) // Dragged to an item slot
                {
                    inventoryUI.OnItemDraggedToInventorySlot(draggingSlot, destSlot);
                }

                destSlot.ShowItemDetails();
            }
            else
            {
                // Dragged to an empty are so reset the selection
                selectedItem = draggingSlotIndex;
                base.UpdateSelection();
            }
        }

        void OnItemDraggedToEquipmentSlot(ItemSlotUI sourceSlot, ItemSlotUI destSlot)
        {
            var item = sourceSlot.ItemStack?.Item;
            if (item != null && destSlot is EquipmentSlotUI)
            {
                var equipmentSlot = destSlot as EquipmentSlotUI;
                if (equipmentSlot.CanEquipItem(item))
                    PutItemInEquipmentSlot(sourceSlot, equipmentSlot);
            }
        }

        void PutItemInEquipmentSlot(ItemSlotUI sourceSlot, EquipmentSlotUI equipmentSlot)
        {
            var tempSlot = equipmentSlot.ItemStack;
            inventory.SetItemToEquipmentSlot(sourceSlot.ItemStack, equipmentSlot.Index);

            if (sourceSlot is InventorySlotUI)
            {
                // Inventory Slot ->  Equipment Slot
                OnEquip?.Invoke(sourceSlot.ItemStack.Item);

                inventory.RemoveItemStack(sourceSlot.ItemStack, sourceSlot.Index);

                if (tempSlot != null)
                    inventory.AddItemStack(tempSlot, sourceSlot.Index);
            }
            else
            {
                // Equipment Slot ->  Equipment Slot

                if (tempSlot != null)
                    inventory.SetItemToEquipmentSlot(tempSlot, sourceSlot.Index);
                else
                    inventory.RemoveItemFromEquipmentSlot(sourceSlot.Index);
            }

            OnEquipmentsUpdated?.Invoke();

            // Only change the selection after a frame
            StartCoroutine(AsyncUtil.RunAfterFrames(1, () => ChangeSelection(equipmentSlot.Index)));
        }

        void OnSelectedItemChanged(int selection)
        {
            if (!IsInFocus) return;

            if (!InventorySettings.i.ShowDetailsOfSelectedItem) return;

            ShowDetailsOfItem(selection);
        }

        void OnHoveredItemChanged(int hoveredItem)
        {
            if (InventorySettings.i.ShowDetailsOfSelectedItem) return;

            ShowDetailsOfItem(hoveredItem);
        }

        void ShowDetailsOfItem(int index)
        {
            bool isDraggingInAnySelector = dragImage.gameObject.activeSelf;
            if (!isDraggingInAnySelector && index >= 0 && index < equipmentSlots.Count && equipmentSlots[index]?.ItemStack?.Item != null)
            {
                var hoveredSlot = equipmentSlots[index];
                hoveredSlot?.ShowItemDetails();
            }
            else
                itemDetailsUI?.Hide();
        }

        private void OnDisable()
        {
            itemDetailsUI?.gameObject.SetActive(false);
            Disable();
        }
    }
}
