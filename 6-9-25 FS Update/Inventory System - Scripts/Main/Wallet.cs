using FS_Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS_InventorySystem
{

    public class Wallet : MonoBehaviour, ISavable
    {
        [SerializeField] List<CurrencyAmount> currencies;

        public event Action OnUpdated;

        public void AddCurrency(CurrencyAmount currencyToAdd)
        {
            var currency = currencies.FirstOrDefault(c => c.currencyIndex == currencyToAdd.currencyIndex);
            if (currency != null)
            {
                currency.amount += currencyToAdd.amount;
                OnUpdated?.Invoke();
            }
        }

        public void TakeCurrency(CurrencyAmount currencyToTake)
        {
            var currency = currencies.FirstOrDefault(c => c.currencyIndex == currencyToTake.currencyIndex);
            if (currency != null)
            {
                currency.amount -= currencyToTake.amount;
                OnUpdated?.Invoke();
            }
        }

        public bool HasCurrency(CurrencyAmount currencyToCheck)
        {
            var currency = currencies.FirstOrDefault(c => c.currencyIndex == currencyToCheck.currencyIndex);
            if (currency == null || currency.amount < currencyToCheck.amount)
                return false;

            return true;
        }

        public void TakeCurrencies(List<CurrencyAmount> currencies)
        {
            foreach (var currencyToTake in currencies)
            {
                TakeCurrency(currencyToTake);
            }
        }

        public bool HasCurrencies(List<CurrencyAmount> currenciesToCheck)
        {
            foreach (var currencyToCheck in currenciesToCheck)
            {
                if (!HasCurrency(currencyToCheck))
                    return false;
            }

            return true;
        }

        public List<CurrencyAmount> GetCurrencies() => currencies.GetRange(0, currencies.Count);

        public object CaptureState()
        {
            var saveData = new WalletSaveData()
            {
                currencies = currencies,
            };

            return saveData;
        }

        public void RestoreState(object state)
        {
            var saveData = (WalletSaveData)state;
            currencies = saveData.currencies;

            OnUpdated?.Invoke();
        }

        public Type GetSavaDataType() => typeof(WalletSaveData);
    }

    [System.Serializable]
    public class WalletSaveData
    {
        public List<CurrencyAmount> currencies;
    }

}
