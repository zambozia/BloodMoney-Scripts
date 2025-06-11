using FS_InventorySystem;
using FS_ThirdPerson;
using FS_Util;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS_InventorySystem
{

    public class IngredientsUI : SelectionUI<ImageSlot>
    {
        [SerializeField] ItemDetailsUI itemDetailsUI;

        List<IngredientSlotUI> ingredientsUISlots = new List<IngredientSlotUI>();

        private void Start()
        {
            Init();

            OnSelectionChanged += OnSelectedItemChanged;
            OnHoverChanged += OnHoveredItemChanged;
        }

        private void Update()
        {
            if (!IsInFocus || SelectionPaused) return;

            if (ingredientsUISlots.Count > 0)
                HandleUpdate();
        }

        public void SetIngredientsSlots(List<IngredientSlotUI> ingredientSlots)
        {
            ingredientsUISlots = ingredientSlots;
            SetItems(ingredientsUISlots.Select(s => s.ImageSlot).ToList());
        }

        void OnSelectedItemChanged(int selection)
        {
            // Wait for a frame since ingredients slots will be destoryed and added again
            StartCoroutine(AsyncUtil.RunAfterFrames(1,
                () =>
                {
                    if (!IsInFocus)
                    {
                        itemDetailsUI?.Hide();
                        return;
                    }

                    if (!InventorySettings.i.ShowDetailsOfSelectedItem) return;

                    ShowDetailsOfItem(selection);
                })
            );
        }

        void OnHoveredItemChanged(int hoveredItem)
        {
            if (InventorySettings.i.ShowDetailsOfSelectedItem) return;

            ShowDetailsOfItem(hoveredItem);
        }

        void ShowDetailsOfItem(int index)
        {
            if (index >= 0 && index < ingredientsUISlots.Count && ingredientsUISlots[index]?.Item != null)
                itemDetailsUI?.Show(ingredientsUISlots[index].Item, ingredientsUISlots[index].transform);
            else
                itemDetailsUI?.Hide();
        }

        private void OnDisable()
        {
            itemDetailsUI?.gameObject?.SetActive(false);
            base.Disable();
        }
    }
}
