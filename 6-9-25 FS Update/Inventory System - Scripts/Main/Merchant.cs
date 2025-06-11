using FS_Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS_InventorySystem
{
    [RequireComponent(typeof(Inventory))]
    public class Merchant : MonoBehaviour
    {


        [Tooltip("Can this merchant buy and sell items")]
        [SerializeField] bool canTrade = true;
        [Tooltip("Can this merchant craft items")]
        [SerializeField] bool canCraft = true;
        [Tooltip("Can this merchant dismantle items")]
        [SerializeField] bool canDismantle = true;

        [Tooltip("When player sells items to a merchant, they don't have to pay full price. The sell price modifier can be used to modify the price the merchant is willing to pay")]
        [SerializeField] float sellPriceModifier = 0.5f;

        [SerializeField] List<Item> craftableItems;

        Inventory inventory;

        private void Awake()
        {
            inventory = GetComponent<Inventory>();
        }

        public bool CanTrade => canTrade;
        public bool CanCraft => canCraft;
        public bool CanDismantle => canDismantle;

        public float SellPriceModifier => sellPriceModifier;

        public List<Item> CraftableItems => craftableItems;

        public Inventory Inventory => inventory;
    }
}
