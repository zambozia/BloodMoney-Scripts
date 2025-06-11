using FS_Core;
using FS_Util;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS_InventorySystem
{
    public class InventorySoundEffects : MonoBehaviour
    {
        [SerializeField] AudioClip consumeClip;
        [SerializeField] AudioClip equipClip;
        [SerializeField] AudioClip unEquipClip;
        [SerializeField] AudioClip dropClip;
        [SerializeField] AudioClip buyClip;
        [SerializeField] AudioClip sellClip;
        [SerializeField] AudioClip craftClip;
        [SerializeField] AudioClip dismantleClip;
        [SerializeField] AudioClip inventoryOpenClip;
        [SerializeField] AudioClip inventoryCloseClip;
        [SerializeField] AudioClip categoryChangeClip;


        EquipmentUI equipmentUI;
        ShopUI shopUI;
        CraftingUI craftingUI;
        DismantleUI dismantleUI;
        List<InventoryUI> inventoryUIs = new List<InventoryUI>();
        InventoryOpener inventoryOpener;
        ShopOpener shopOpener;
        CountSelectorUI countSelectorUI;
        ConfirmationUI confirmationUI;

        private void Awake()
        {
            equipmentUI = GetComponentInChildren<EquipmentUI>(includeInactive: true);
            shopUI = GetComponentInChildren<ShopUI>(includeInactive: true);
            craftingUI = GetComponentInChildren<CraftingUI>(includeInactive: true);
            dismantleUI = GetComponentInChildren<DismantleUI>(includeInactive: true);
            inventoryUIs = GetComponentsInChildren<InventoryUI>(includeInactive: true).ToList();
            inventoryOpener = GetComponentInChildren<InventoryOpener>(includeInactive: true);
            shopOpener = GetComponentInChildren<ShopOpener>(includeInactive: true);
        }

        private void Start()
        {
            if (shopUI != null)
            {
                shopUI.OnBuy += (Item item, int count) => FSAudioUtil.PlaySfx(buyClip, overridePlayingAudio: true);
                shopUI.OnSell += (Item item, int count) => FSAudioUtil.PlaySfx(sellClip, overridePlayingAudio: true);
            }

            if (equipmentUI != null)
            {
                equipmentUI.OnEquip += (Item item) => FSAudioUtil.PlaySfx(equipClip, overridePlayingAudio: true);
                equipmentUI.InventoryUI.OnUnEquip += (Item item) => FSAudioUtil.PlaySfx(unEquipClip, overridePlayingAudio: true);
            }

            if (craftingUI != null)
            {
                craftingUI.OnItemCrafted += (Item item) => FSAudioUtil.PlaySfx(craftClip);
            }

            if (dismantleUI != null)
            {
                dismantleUI.OnItemDismantled += (Item item) => FSAudioUtil.PlaySfx(dismantleClip, overridePlayingAudio: true);
            }

            if (inventoryUIs != null)
            {
                foreach (var inventoryUI in  inventoryUIs)
                {
                    inventoryUI.OnCategoryChanged += (int index) => FSAudioUtil.PlaySfx(categoryChangeClip, overridePlayingAudio: true);
                    inventoryUI.OnItemConsumed += (ItemStack itemStack) => FSAudioUtil.PlaySfx(consumeClip, overridePlayingAudio: true);
                    inventoryUI.OnItemDropped += (ItemStack itemStack) => FSAudioUtil.PlaySfx(dropClip, overridePlayingAudio: true);
                }
            }

            if (inventoryOpener != null)
            {
                inventoryOpener.OnOpened += () => FSAudioUtil.PlaySfx(inventoryOpenClip);
                inventoryOpener.OnClosed += () => FSAudioUtil.PlaySfx(inventoryCloseClip);
            }

            if (shopOpener != null)
            {
                shopOpener.OnOpened += () => FSAudioUtil.PlaySfx(inventoryOpenClip);
                shopOpener.OnClosed += () => FSAudioUtil.PlaySfx(inventoryCloseClip);
            }

        }
    }
}
