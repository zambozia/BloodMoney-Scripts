using FS_Core;
using FS_ThirdPerson;
using FS_Util;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FS_InventorySystem
{
    public class ItemDetailsUI : MonoBehaviour
    {
        [SerializeField] bool setDataFromInventory = false;
        [ShowIf("setDataFromInventory", true)]
        [SerializeField] InventoryUI inventoryUI;
        [SerializeField] bool rebuidUIDuringChanges = false;

        [Header("References")]
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text categoryText;
        [SerializeField] TMP_Text descriptionText;
        [SerializeField] CurrencyUI currencyUI;
        [SerializeField] WeightUI weightUI;
        [SerializeField] Transform attributeContainer;
        [SerializeField] AttributeUI attributeUIPrefab;

        RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (setDataFromInventory && inventoryUI != null)
            {
                inventoryUI.OnSelectionChanged += SelectedItemChanged;
            }
        }

        private void Start()
        {
            
        }

        public void SetData(Item item, Item equippedItem=null, float priceModifier = 1)
        {
            nameText.text = item.Name;

            if (categoryText != null )
                categoryText.text = item.category.Name;

            if (currencyUI != null)
                currencyUI.SetData(item.price, priceModifier);

            if (weightUI != null)
                weightUI.SetWeight(item.weight);

            if (descriptionText != null)
                descriptionText.text = item.Description;

            StartCoroutine(AsyncUtil.RunAfterFrames(1, () =>
            {
                // Show Attributes
                if (attributeContainer != null && attributeUIPrefab != null)
                {
                    // Destroy old attributes
                    foreach (Transform child in attributeContainer)
                        Destroy(child.gameObject);

                    foreach (var attribute in item.Attributes)
                    {
                        var equippedItemAttribute = equippedItem?.Attributes?.FirstOrDefault(a => a.attributeName == attribute.attributeName);

                        var attributeObj = Instantiate(attributeUIPrefab, attributeContainer);
                        attributeObj.SetData(attribute, equippedItemAttribute);
                    }
                }

                if (rebuidUIDuringChanges)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }));
        }

        public void ClearData()
        {
            nameText.text = "";

            if (categoryText != null)
                categoryText.text = "";

            if (descriptionText != null)
                descriptionText.text = "";

            if (currencyUI != null)
                currencyUI.ClearData();

            if (weightUI != null)
                weightUI.ClearWeight();

            if (attributeContainer != null)
            {
                // Destroy old attributes
                foreach (Transform child in attributeContainer)
                { 
                    Destroy(child.gameObject);
                }
            }
        }

        void SelectedItemChanged(int selection)
        {
            var item = inventoryUI.GetSelectedItem();
            if (item != null)
                SetData(item);
            else
                ClearData();
        }

        public void Show(Item item, Transform slotTransform, Item equippedItem = null, float priceModifier = 1)
        {
            gameObject.SetActive(true);
            SetData(item, equippedItem, priceModifier);

            float uiWidth = rectTransform.rect.width;
            float uiHeight = rectTransform.rect.height;
            
            Vector3 offset = new Vector3(50, -50);

            if (slotTransform.position.x > Screen.width * 0.75f)
                offset = new Vector3(-uiWidth-50, offset.y);
            
            if (slotTransform.position.y < Screen.height * 0.25f)
                offset = new Vector3(offset.x, uiHeight+50);

            transform.position = slotTransform.position + offset;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
