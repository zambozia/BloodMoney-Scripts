using FS_Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS_InventorySystem
{
    public class LootHotspot : Hotspot
    {
        [SerializeField] List<ItemStack> lootItems;

        public override void ShowIndicator(bool state)
        {
            base.ShowIndicator(state);

            if (interactText != null)
                interactText.text = "[E] Loot";
        }

        public override void Interact(HotspotDetector detector)
        {
            var inventory = detector.GetComponent<Inventory>();
            if (inventory != null)
               lootItems.ForEach(item => inventory.AddItemStack(item));

            base.Interact(detector);
        }
    }
}
