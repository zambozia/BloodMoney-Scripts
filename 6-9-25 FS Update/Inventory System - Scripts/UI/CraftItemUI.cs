using FS_Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FS_InventorySystem
{

    public class CraftItemUI : MonoBehaviour
    {
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text categoryText;
        [SerializeField] Image image;

        public Item Item { get; private set; }

        public void SetData(Item item)
        {
            Item = item;

            nameText.text = item.Name;
            categoryText.text = item.category.Name;
            image.sprite = item.icon;
        }
    }

}
