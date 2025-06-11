using FS_Util;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FS_InventorySystem {

    public class WeightUI : MonoBehaviour
    {
        [SerializeField] TMP_Text weightTxt;
        [SerializeField] Image weightImage;

        [SerializeField] bool usePlayerInventory = true;

        [ShowIf("usePlayerInventory", true)]
        [SerializeField] Inventory inventory;

        private void Start()
        {
            if (usePlayerInventory)
                inventory = InventorySettings.i.PlayerInventory;

            if (inventory != null)
            {
                SetInventoryWeight();
                inventory.OnUpdated += SetInventoryWeight;
                inventory.OnEquipmentsUpdated += SetInventoryWeight;
            }
        }

        public void SetWeight(float weight)
        {
            weightTxt.text = $"{weight}";

            if (weightImage != null)
                weightImage.color = new Color(1, 1, 1, 1);
        }

        public void ClearWeight()
        {
            weightTxt.text = "";

            if (weightImage != null)
                weightImage.color = new Color(1, 1, 1, 0);
        }

        public void SetInventoryWeight()
        {
            weightTxt.text = $"{inventory.TotalWeight}/{inventory.WeightCapacity}";

            if (weightImage != null)
                weightImage.color = new Color(1, 1, 1, 1);
        }
    }
}
