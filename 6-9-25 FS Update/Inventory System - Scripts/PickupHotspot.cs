using FS_Core;
using FS_Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS_InventorySystem
{
    public class PickupHotspot : Hotspot
    {
        [SerializeField] ItemStack pickupItem;
        [SerializeField] AudioClip pickupSound;

        public override void ShowIndicator(bool state)
        {
            base.ShowIndicator(state);

            if (interactText != null )
                interactText.text = "[E] " + pickupItem.Item.Name;
        }

        public override void Interact(HotspotDetector detector)
        {
            var inventory = detector.GetComponent<Inventory>();
            if (inventory != null)
            {
                FSAudioUtil.PlaySfx(pickupSound, overridePlayingAudio: true);
                inventory.AddItemStack(pickupItem);
            }

            base.Interact(detector);
        }
    }
}
