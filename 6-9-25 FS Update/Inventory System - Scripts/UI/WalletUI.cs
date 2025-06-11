using FS_Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS_InventorySystem
{
    public class WalletUI : MonoBehaviour
    {
        [SerializeField] bool usePlayerWallet = true;

        [ShowIf("usePlayerWallet", false)]
        [SerializeField] Wallet wallet;
        [SerializeField] CurrencyUI currencyUIPrefab;

        private void Start()
        {
            if (usePlayerWallet)
                wallet = InventorySettings.i.PlayerWallet;

            ShowCurrencies();
            wallet.OnUpdated += ShowCurrencies;
        }

        void ShowCurrencies()
        {
            var currencies = wallet.GetCurrencies();

            // Destroy old UI
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            foreach (var currency in currencies)
            {
                var currencyObj = Instantiate(currencyUIPrefab, transform);
                currencyObj.SetData(currency);
            }
        }
    }
}
