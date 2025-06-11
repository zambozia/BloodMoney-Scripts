using FS_Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS_InventorySystem
{
    public class MerchantHotspot : Hotspot
    {
        [SerializeField] ShopOpener shopOpener;
        [SerializeField] Merchant merchant;

        public override void ShowIndicator(bool state)
        {
            base.ShowIndicator(state);

            if (interactText != null)
                interactText.text = "[E] Merchant";
        }

        public override void Interact(HotspotDetector detector)
        {
            if (shopOpener.gameObject.activeSelf) return;

            shopOpener.OpenShop(merchant);

            base.Interact(detector);
        }
    }
}
