using FS_Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FS_InventorySystem {

    public class CurrencyUI : MonoBehaviour
    {
        [SerializeField] bool showIcon = true;
        [SerializeField] Image iconImage;
        [SerializeField] TMP_Text symbolText;
        [SerializeField] TMP_Text amountText;

        public void SetData(CurrencyAmount currency, float priceModifier = 1)
        {
            amountText.text = "" + (currency.amount * priceModifier);

            if (showIcon)
            {
                iconImage.sprite = CurrencyDatabase.Instance.GetCurrencyByIndex(currency.currencyIndex).icon;
                iconImage.color = new Color(1, 1, 1, 1);
            }
            else
                symbolText.text = CurrencyDatabase.Instance.GetCurrencyByIndex(currency.currencyIndex).symbol;
        }

        public void SetAmount(string amount)
        {
            amountText.text = "" + amount;
        }

        public void ClearData()
        {
            amountText.text = "";

            if (showIcon)
                iconImage.color = new Color(1, 1, 1, 0);
        }
    }

}
