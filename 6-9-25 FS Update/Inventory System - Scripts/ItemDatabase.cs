using FS_Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS_InventorySystem
{
    public class ItemDatabase : FSObjectDatabase<Item>
    {
        public static ItemDatabase i { get; private set; }
        private void Awake()
        {
            i = this;
            Init();
        }
    }
}
