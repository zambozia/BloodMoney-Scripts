using FS_Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS_InventorySystem
{
    public class HealthRestoreAction : ItemAction
    {
        public override void OnUse(Item item, GameObject character)
        {
            int hpRestore = item.GetAttributeValue<int>("health amount");
            var damagable = character.GetComponent<Damagable>();
            damagable.UpdateHealth(hpRestore);
        }
    }
}
