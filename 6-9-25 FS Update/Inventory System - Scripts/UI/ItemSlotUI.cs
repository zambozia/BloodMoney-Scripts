using FS_Core;
using FS_Util;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FS_InventorySystem
{
    [FoldoutGroup("Advanced Settings", false, "makeImageTransparentWhenClearing", "useCustomImageSlot", "imageSlot")]
    public class ItemSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] protected Image itemImage;
        [SerializeField] protected TMP_Text itemCount;
        [SerializeField] protected TMP_Text itemNameTxt;
        [SerializeField] protected TMP_Text categoryNameTxt;

        [SerializeField] bool makeImageTransparentWhenClearing = true;
        [SerializeField] bool useCustomImageSlot = false;
        [ShowIf("useCustomImageSlot", true)]
        [SerializeField] ImageSlot imageSlot;

        ItemDetailsUI _itemDetailsUI;
        Image _dragImage;

        public ItemStack ItemStack { get; private set; }
        public int Index { get; private set; }
        public ImageSlot ImageSlot {
            get 
            {
                if (!useCustomImageSlot)
                {
                    if (imageSlot == null)
                        imageSlot = GetComponent<ImageSlot>();
                }

                return imageSlot;
            }
        }

        Action<ItemSlotUI> _onDragStarted;
        Action _onDragEnded;

        public bool CanBeDragged { get; private set; } = true;
        public bool CanDropItems { get; private set; } = true;

        public void Init(ItemDetailsUI itemDetailsUI, int index = -1)
        {
            Index = index;
            _itemDetailsUI = itemDetailsUI;
            CanBeDragged = false;
            CanDropItems = false;
        }

        public void Init(Action<ItemSlotUI> onDragStarted, Action onDragEnded, Image dragImage,
            ItemDetailsUI itemDetailsUI, int index=-1)
        {
            _onDragStarted = onDragStarted;
            _onDragEnded = onDragEnded;
            _dragImage = dragImage;
            _itemDetailsUI = itemDetailsUI;
            Index = index;
            CanBeDragged = true;
            CanDropItems = true;
        }

        public virtual void SetData(ItemStack itemStack)
        {
            this.ItemStack = itemStack;

            if (itemImage != null)
            {
                itemImage.sprite = itemStack.Item.Icon;
                itemImage.color = new Color(1, 1, 1, 1);
            }

            if (itemNameTxt != null)
                itemNameTxt.text = itemStack.Item.Name;

            if (categoryNameTxt != null)
                categoryNameTxt.text = itemStack.Item.Category?.Name ?? "";

            if (itemCount != null)
            {
                if (!itemStack.Item.category.Stackable)
                {
                    itemCount?.gameObject.SetActive(false);
                    return;
                }

                itemCount.gameObject.SetActive(true);
                itemCount.text = "" + itemStack.Count;
            }
        }

        public virtual void ClearData()
        {
            this.ItemStack = null;

            if (itemImage != null)
            {
                itemImage.sprite = null;
                if (makeImageTransparentWhenClearing)
                    itemImage.color = new Color(1, 1, 1, 0);
            }

            if (itemNameTxt != null)
                itemNameTxt.text = "";

            if (categoryNameTxt != null)
                categoryNameTxt.text = "";

            if (itemCount != null)
                itemCount.text = "";
        }

        bool isDragging = false;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!CanBeDragged || ItemStack == null) return;

            itemImage.color = new Color(0.5f, 0.5f, 0.5f, 1);
            isDragging = true;

            _dragImage.gameObject.SetActive(true);
            _dragImage.sprite = ItemStack.Item.Icon;

            _onDragStarted?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            _dragImage.transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            itemImage.color = new Color(1, 1, 1, 1);
            isDragging = false;
            _dragImage.gameObject.SetActive(false);

            _onDragEnded.Invoke();
        }

        public void ShowItemDetails()
        {
            if (ItemStack?.Item != null)
                _itemDetailsUI?.Show(ItemStack?.Item, transform);
        }

        private void OnEnable()
        {
            if (ItemStack?.Item != null)
                itemImage.color = new Color(1, 1, 1, 1);    // color might be changed while dragging the slot
        }

        public static ItemSlotUI FindSlotAtMousePos(GraphicRaycaster graphicRaycaster)
        {
            Vector3[] offsets = new Vector3[]
            {
                Vector3.zero,
                new Vector3(-10, 0),
                new Vector3(0, -10),
                new Vector3(-10, -10)
            };

            foreach (var offset in offsets)
            {
                var eventSystem = EventSystem.current;
                PointerEventData pointerData = new PointerEventData(eventSystem)
                {
                    position = Input.mousePosition + offset
                };

                List<RaycastResult> results = new List<RaycastResult>();
                graphicRaycaster.Raycast(pointerData, results);

                foreach (var result in results)
                {
                    var slotUI = result.gameObject.GetComponent<ItemSlotUI>();
                    if (slotUI != null)
                        return slotUI;
                }
            }

            return null;
        }
    }
}

