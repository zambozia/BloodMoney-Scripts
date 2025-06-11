using FS_Core;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FS_InventorySystem
{

    public class EquipmentSlotUI : ItemSlotUI
    {
        [ShowEquipmentDropdown]
        [SerializeField] public int selectedSlotIndex;

        EquipmentUI equipmentUI;
        Sprite defaultIcon;
        private void Awake()
        {
            equipmentUI = GetComponentInParent<EquipmentUI>();
            defaultIcon = itemImage.sprite;
        }

        public override void SetData(ItemStack itemStack)
        {
            base.SetData(itemStack);

            //itemStack.Item.Equip(equipmentUI.Inventory.gameObject);
        }

        public override void ClearData()
        {
            base.ClearData();

            itemImage.sprite = defaultIcon;
        }

        public bool CanEquipItem(Item item)
        {
            return selectedSlotIndex == item.EquipmentSlotIndex;
        }
    }
}
