using FS_Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FS_InventorySystem {

    public class AttributeUI : MonoBehaviour
    {
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text valueText;
        [SerializeField] TMP_Text valueDifferenceText;
        [SerializeField] Color positiveColor = Color.green;
        [SerializeField] Color negativeColor = Color.red;

        public void SetData(ItemAttribute attribute, ItemAttribute equippedItemAttribute=null)
        {
            nameText.text = attribute.attributeName;
            valueText.text = attribute.GetValueAsString();

            if (equippedItemAttribute != null && (attribute.attributeType == ItemAttributeType.Decimal || attribute.attributeType == ItemAttributeType.Integer)) 
            {
                valueDifferenceText?.gameObject.SetActive(true);

                float diff = attribute.GetValueAsFloat() - equippedItemAttribute.GetValueAsFloat();

                valueDifferenceText.text = (diff >= 0? "+" : "") + diff;
                valueDifferenceText.color = diff >= 0? positiveColor : negativeColor;
            }
            else
                valueDifferenceText?.gameObject.SetActive(false);
        }
    }

}
