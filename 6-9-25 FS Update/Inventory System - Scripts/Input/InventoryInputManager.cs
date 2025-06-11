using FS_Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS_InventorySystem
{

    public class InventoryInputManager : MonoBehaviour
    {
        [Header("Keys")]
        [SerializeField] KeyCode selectKey = KeyCode.E;
        [SerializeField] KeyCode consumeKey = KeyCode.C;
        [SerializeField] KeyCode dropKey = KeyCode.R;
        [SerializeField] KeyCode backKey = KeyCode.Escape;
        [SerializeField] KeyCode quickSortKey = KeyCode.Q;
        [SerializeField] KeyCode nextCategoryKey = KeyCode.None;
        [SerializeField] KeyCode previousCategoryKey = KeyCode.None;
        [SerializeField] KeyCode openInventoryKey = KeyCode.I;

        [Header("Buttons")]
        [SerializeField] string selectButton;
        [SerializeField] string consumeButton;
        [SerializeField] string dropButton;
        [SerializeField] string backButton;
        [SerializeField] string quickSortButton;
        [SerializeField] string nextCategoryButton;
        [SerializeField] string previousCategoryButton;
        [SerializeField] string openInventoryButton;

        public bool Select { get; set; }
        public bool Consume { get; set; }
        public bool Drop { get; set; }
        public bool Back { get; set; }
        public bool QuickSort { get; set; }
        public bool NextCategory { get; set; }
        public bool PreviousCategory { get; set; }
        public bool OpenInventory { get; set; }

#if inputsystem
        FSSystemsInputAction input;
        private void OnEnable()
        {
            input = new FSSystemsInputAction();
            input.Enable();
        }
        private void OnDisable()
        {
            input.Disable();
        }
#endif

        private void Update()
        {
            HandleSelect();
            HandleConsume();
            HandleDrop();
            HandleBack();
            HandleQuickSort();
            HandlePreviousCategory();
            HandleNextCategory();
            HandleOpenInventory();
        }

        private void HandleSelect()
        {
#if inputsystem
            Select = input.Inventory.Select.WasPressedThisFrame();
#else
            Select = Input.GetKeyDown(selectKey) || (!string.IsNullOrEmpty(selectButton) && Input.GetButtonDown(selectButton));
#endif
        }

        private void HandleBack()
        {
#if inputsystem
            Back = input.Inventory.Back.WasPressedThisFrame();
#else
            Back = Input.GetKeyDown(backKey) || (!string.IsNullOrEmpty(backButton) && Input.GetButtonDown(backButton));
#endif
        }

        private void HandleConsume()
        {
#if inputsystem
            Consume = input.Inventory.Consume.WasPressedThisFrame();
#else
            Consume = Input.GetKeyDown(consumeKey) || (!string.IsNullOrEmpty(consumeButton) && Input.GetButtonDown(consumeButton));
#endif
        }

        private void HandleDrop()
        {
#if inputsystem
            Drop = input.Inventory.Drop.WasPressedThisFrame();
#else
            Drop = Input.GetKeyDown(dropKey) || (!string.IsNullOrEmpty(dropButton) && Input.GetButtonDown(dropButton));
#endif
        }

        private void HandleQuickSort()
        {
#if inputsystem
            QuickSort = input.Inventory.QuickSort.WasPressedThisFrame();
#else
        QuickSort = Input.GetKeyDown(quickSortKey) || (!string.IsNullOrEmpty(quickSortButton) && Input.GetButtonDown(quickSortButton));
#endif
        }

        private void HandleNextCategory()
        {
#if inputsystem
            NextCategory = input.Inventory.NextCategory.WasPressedThisFrame();
#else
        NextCategory = Input.GetKeyDown(nextCategoryKey) || (!string.IsNullOrEmpty(nextCategoryButton) && Input.GetButtonDown(nextCategoryButton));
#endif
        }

        private void HandlePreviousCategory()
        {
#if inputsystem
            PreviousCategory = input.Inventory.PreviousCategory.WasPressedThisFrame();
#else
        PreviousCategory = Input.GetKeyDown(previousCategoryKey) || (!string.IsNullOrEmpty(previousCategoryButton) && Input.GetButtonDown(previousCategoryButton));
#endif
        }

        private void HandleOpenInventory()
        {
#if inputsystem
            OpenInventory = input.Inventory.OpenInventory.WasPressedThisFrame();
#else
            OpenInventory = Input.GetKeyDown(openInventoryKey) || (!string.IsNullOrEmpty(openInventoryButton) && Input.GetButtonDown(openInventoryButton));
#endif
        }
    }
}
