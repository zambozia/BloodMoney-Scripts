using FS_Core;
using FS_Util;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FS_InventorySystem {

    public class IngredientSlotUI : MonoBehaviour
    {
        [SerializeField] Image image;
        [SerializeField] TMP_Text countText;
        [SerializeField] Color positiveColor = Color.green;
        [SerializeField] Color negativeColor = Color.red;
        [SerializeField] ImageSlot imageSlot;

        public Item Item { get; private set; }

        public void SetData(ItemStack ingredient, Inventory inventory)
        {
            Item = ingredient.Item;

            image.sprite = ingredient.Item.Icon;

            int itemCount = 0;
            var itemStack = inventory.GetItemStack(ingredient.Item);
            if (itemStack != null)
                itemCount = itemStack.Count;

            countText.text = $"{itemCount}/{ingredient.Count}";
            countText.color = (itemCount >= ingredient.Count) ? positiveColor : negativeColor;
        }

        public ImageSlot ImageSlot => imageSlot;
    }

}
