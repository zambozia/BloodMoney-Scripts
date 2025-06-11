using FS_Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS_InventorySystem
{

    public class InventorySettings : MonoBehaviour
    {
        [Header("References")]
        public Inventory playerInventory;
        public Wallet playerWallet;
        
        public InventoryInputManager InputManager { get; private set; }

        [Header("Settings")]
        [SerializeField] bool showDetailsOfSelectedItem = false;

        EquipmentUI equipmentUI;

        public static InventorySettings i { get; private set; }
        private void Awake()
        {
            i = this;

            var player = GameObject.FindGameObjectWithTag("Player");

            if (playerInventory == null)
                playerInventory = player.GetComponent<Inventory>();

            if (playerWallet == null)
                playerWallet = player.GetComponent<Wallet>();

            InputManager = player.GetComponent<InventoryInputManager>();

            equipmentUI = GetComponentInChildren<EquipmentUI>(includeInactive: true);
            foreach (var equipmentSlotUI in equipmentUI.EquipmentSlots)
            {
                playerInventory.AddEquipmentSlot(new EquipmentSlot()
                {
                    equipmentSlotIndex = equipmentSlotUI.selectedSlotIndex
                });
            }
        }

        public bool ShowDetailsOfSelectedItem => showDetailsOfSelectedItem;
        public Inventory PlayerInventory => playerInventory;
        public Wallet PlayerWallet => playerWallet;
    }

}
