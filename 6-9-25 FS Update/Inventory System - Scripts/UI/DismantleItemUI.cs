using FS_Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FS_InventorySystem
{
    public class DismantleItemUI : MonoBehaviour
    {
        [SerializeField] Image image;
        [SerializeField] TMP_Text countText;
        [SerializeField] TMP_Text nameText;

        public void SetData(ItemStack itemStack)
        {
            image.sprite = itemStack.Item.Icon;
            countText.text = "" + itemStack.Count;
            nameText.text = itemStack.Item.Name;
        }
    }

}
