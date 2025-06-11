using FS_Core;
using FS_Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace FS_InventorySystem
{
    public class Inventory : MonoBehaviour, ISavable
    {
        [SerializeField] List<ItemStack> defaultItems;
        [SerializeField] bool storeCategoriesSeparately = true;
        [SerializeField] List<CategorySlot> defaultCategorySlots;

        [Space(10)]

        [SerializeField] float weightCapacity = 50;
        [SerializeField] bool overWeightAllowed = true;

        [SerializeField] bool hasFixedNumberOfSlots = false;

        [ShowIf("hasFixedNumberOfSlots", true)]
        [SerializeField] int numberOfSlots = 49;

        [ShowIf("hasFixedNumberOfSlots", false)]
        [SerializeField] int defaultNumberOfSlots = 49;
        
        
        List<CategorySlot> categorySlots = new List<CategorySlot>();

        List<EquipmentSlot> equipmentSlots = new List<EquipmentSlot>();
        float inventoryWeight = 0;
        float equipmentsWeight = 0;

        public float TotalWeight => inventoryWeight + equipmentsWeight;
        public float WeightCapacity => weightCapacity;
        public bool IsOverWeight => TotalWeight > weightCapacity;

        public List<CategorySlot> CategorySlots => categorySlots;

        public event Action OnUpdated;
        public event Action OnEquipmentsUpdated;

        ItemAttacher itemAttacher;

        private void Awake()
        {
            if (hasFixedNumberOfSlots)
                defaultNumberOfSlots = numberOfSlots;

            itemAttacher = GetComponent<ItemAttacher>();

            InitItemsAndSlots();
        }

        void InitItemsAndSlots()
        {
            categorySlots = new List<CategorySlot>();

            if (storeCategoriesSeparately)
            {
                foreach (var categorySlot in defaultCategorySlots)
                {
                    categorySlot.itemSlots = new List<ItemStack>(new ItemStack[defaultNumberOfSlots]);
                    categorySlots.Add(categorySlot);
                }
            }

            // Just add a default slot if there are no categoryslots specified
            if (categorySlots.Count == 0)
            {
                categorySlots.Add(new CategorySlot()
                {
                    categories = new List<ItemCategory>() { null },
                    itemSlots = new List<ItemStack>(new ItemStack[defaultNumberOfSlots])
                });
            }

            foreach (var itemStack in defaultItems)
            {
                AddItem(itemStack.Item, itemStack.Count, invokeUpdateEvent: false);
            }
        }

        public List<ItemStack> GetSlotsByCategory(int categoryIndex)
        {
            return CategorySlots[categoryIndex].itemSlots.ToList();
        }

        public Item GetItem(int itemIndex, int categoryIndex)
        {
            var currenSlots = GetSlotsByCategory(categoryIndex);
            return currenSlots[itemIndex].Item;
        }

        public Item UseItem(int itemIndex, int selectedCategory)
        {
            var item = GetItem(itemIndex, selectedCategory);
            return ConsumeItem(item, itemIndex);
        }

        public Item ConsumeItem(Item item, int itemIndex)
        {
            bool itemUsed = item.Use(gameObject);
            if (itemUsed)
            {
                if (item.Consumable)
                    RemoveItem(item, index: itemIndex);

                return item;
            }

            return null;
        }

        public void AddItem(Item item, int count = 1, int index = -1, bool invokeUpdateEvent = true)
        {
            var itemStack = new ItemStack() 
            { 
                Item = item,
                Count = count
            };
            AddItemStack(itemStack, index, invokeUpdateEvent);
        }

        public void AddItemStack(ItemStack itemStack, int index = -1, bool invokeUpdateEvent=true)
        {
            var item = itemStack.Item;

            var categorySlot = GetCategorySlot(item);

            var itemSlots = categorySlot.itemSlots;

            if (!CanAddItem(item, itemStack.Count))
                return;

            if (index != -1)
            {
                // Add to specified index
                if (index < 0 || index >= itemSlots.Count) return;

                itemSlots[index] = itemStack;
            }
            else
            {
                // Add to empty slot
                if (item.Category.Stackable)
                {
                    var itemSlot = itemSlots.FirstOrDefault(slot => slot != null && slot.Item == item);
                    if (itemSlot != null)
                    {
                        itemSlot.Count += itemStack.Count;
                    }
                    else
                    {
                        AddItemToEmptySlot(itemSlots, itemStack);
                    }
                }
                else
                {
                    for (int i = 0; i < itemStack.Count; i++)
                    {
                        AddItemToEmptySlot(itemSlots, new ItemStack()
                        {
                            Item = item,
                            Count = 1
                        });
                    }
                }
            }

            inventoryWeight += item.weight * itemStack.Count;

            if (invokeUpdateEvent)
                OnUpdated?.Invoke();
        }

        public void RemoveItemStack(ItemStack itemStack, int index = -1)
        {
            RemoveItem(itemStack.Item, itemStack.Count, index);
        }

        public void RemoveWholeStack(int slotIndex, int categoryIndex)
        {
            var itemStack = categorySlots[categoryIndex].itemSlots[slotIndex];
            categorySlots[categoryIndex].itemSlots[slotIndex] = null;

            if (itemStack != null)
                inventoryWeight -= itemStack.Item.weight * itemStack.Count;

            OnUpdated?.Invoke();
        }

        public void RemoveItem(Item item, int countToRemove = 1, int index = -1)
        {
            var categorySlot = GetCategorySlot(item);
            var itemSlots = categorySlot.itemSlots;

            var itemStack = (index >= 0) ? itemSlots[index] : itemSlots.First(slot => slot != null && slot.Item == item);
            if (index == -1)
                index = itemSlots.IndexOf(itemStack);

            if (itemStack.Count - countToRemove <= 0 || !item.Category.Stackable)
                categorySlot.itemSlots[index] = null;
            else
                itemStack.Count -= countToRemove;

            inventoryWeight -= item.weight * countToRemove;

            OnUpdated?.Invoke();
        }

        CategorySlot GetCategorySlot(Item item)
        {
            if (storeCategoriesSeparately)
                return categorySlots.FirstOrDefault(s => s.categories.Contains(item.Category));
            else
                return categorySlots[0];
        }

        CategorySlot AddCategorySlot(ItemCategory category)
        {
            var categorySlot = new CategorySlot()
            {
                categories = new List<ItemCategory>() { category },
                itemSlots = new List<ItemStack>(new ItemStack[defaultNumberOfSlots])
            };
            categorySlots.Add(categorySlot);

            return categorySlot;
        }

        public List<ItemStack> GetAllItemStacks()
        {
            var allItems = new List<ItemStack>();

            if (storeCategoriesSeparately)
            {
                foreach (var categorySlot in categorySlots)
                    allItems.AddRange(categorySlot.itemSlots.Where(s => s != null && s.Item != null));
            }
            else
                allItems = categorySlots[0].itemSlots;

            return allItems;
        }

        void AddItemToEmptySlot(List<ItemStack> itemSlots, ItemStack itemToAdd)
        {
            // Try to add to the first empty slot
            for (int i = 0; i < itemSlots.Count; i++)
            {
                if (itemSlots[i] == null)
                {
                    itemSlots[i] = itemToAdd;
                    return;
                }
            }

            // If no slots are empty, then add a new one
            if (!hasFixedNumberOfSlots)
                itemSlots.Add(itemToAdd);
        }

        public int GetItemCount(Item item)
        {
            var itemSlots = GetCategorySlot(item).itemSlots.ToList();

            var itemSlot = itemSlots.FirstOrDefault(slot => slot.Item == item);

            if (itemSlot != null)
                return itemSlot.Count;
            else
                return 0;
        }

        public bool CanAddItem(Item item, int count=1)
        {
            if (!overWeightAllowed)
                if (item.weight > (weightCapacity - TotalWeight)) return false;

            var itemSlots = GetCategorySlot(item).itemSlots;
            if (hasFixedNumberOfSlots && itemSlots.Count == numberOfSlots)
            {
                // return true only if a slot is free
                return itemSlots.Any(slot => slot == null ||slot.Item == null || (item.Stackable && slot.Item == item));
            }

            return true;
        }

        public void RemoveEmptySlots(int categoryIndex)
        {
            var itemSlots = CategorySlots[categoryIndex].itemSlots;
            itemSlots.RemoveAll(s => s == null || s.Item == null);
        }

        public void RemoveSlotsTillIndex(int slotIndex, int categoryIndex)
        {
            var itemSlots = CategorySlots[categoryIndex].itemSlots;
            for (int i = itemSlots.Count - 1; i > slotIndex; i--)
                itemSlots.RemoveAt(i);
        }

        public ItemStack GetItemStack(Item item)
        {
            var itemStacks = GetCategorySlot(item)?.itemSlots?.ToList();
            return itemStacks?.FirstOrDefault(slot => slot != null && slot.Item == item);
        }

        public bool HasItem(Item item)
        {
            var itemStacks = GetCategorySlot(item)?.itemSlots?.ToList();
            if (itemStacks == null) return false;

            return itemStacks.Exists(slot => slot.Item == item);
        }

        public bool HasRequiredItemCount(Item item, int count)
        {
            var itemStack = GetItemStack(item);
            return itemStack != null && itemStack.Count >= count;
        }

        public bool HasRequiredItemsCount(List<ItemStack> items)
        {
            return items.All(i => HasRequiredItemCount(i.Item, i.Count));
        }

        public void AddEquipmentSlot(EquipmentSlot equipmentSlot)
        {
            equipmentSlots.Add(equipmentSlot);
        }

        public void SetItemToEquipmentSlot(ItemStack itemStack, int index, bool invokeEvent=true)
        {
            equipmentSlots[index].ItemStack = itemStack;
            itemStack.Item.Equip(gameObject, itemAttacher);

            if (invokeEvent)
                EquipmentsUpdated();
        }

        public void RemoveItemFromEquipmentSlot(int index, bool invokeEvent = true)
        {
            equipmentSlots[index].ItemStack?.Item?.Unequip(gameObject, itemAttacher);
            equipmentSlots[index].ItemStack = null;

            if (invokeEvent)
                EquipmentsUpdated();
        }

        void EquipmentsUpdated()
        {
            ReCalculateEquipmentWeight();
            OnEquipmentsUpdated?.Invoke();
        }

        public Item GetEquippedItem(int equipmentSlotIndex)
        {
            return equipmentSlots.FirstOrDefault(e => e.equipmentSlotIndex == equipmentSlotIndex)?.ItemStack?.Item;
        }

        public List<EquipmentSlot> EquipmentSlots => equipmentSlots;

        public void ReCalculateEquipmentWeight()
        {
            float weight = 0f;
            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                if (equipmentSlots[i] != null && equipmentSlots[i].ItemStack != null)
                    weight += equipmentSlots[i].ItemStack.Item.weight * equipmentSlots[i].ItemStack.Count;
            }

            equipmentsWeight = weight;
        }

        public void ReCalculateInventoryWeight()
        {
            float weight = 0f;
            foreach (var categorySlot in categorySlots)
            {
                foreach (var itemSlot in categorySlot.itemSlots)
                {
                    if (itemSlot != null)
                        weight += itemSlot.Item.weight * itemSlot.Count;
                }
            }

            inventoryWeight = weight;
        }

        public object CaptureState()
        {
            var saveData = new InventorySaveData();

            // capture items
            saveData.categorySlots = new List<CategorySlotSaveData>();
            for (int index = 0; index < categorySlots.Count; index++)
            {
                saveData.categorySlots.Add(new CategorySlotSaveData()
                {
                    itemsStacks = new List<ItemStackSaveData>()
                });

                foreach (var itemStack in categorySlots[index].itemSlots)
                {
                    if (itemStack != null)
                    {
                        saveData.categorySlots[index].itemsStacks.Add(new ItemStackSaveData()
                        {
                            itemId = itemStack.Item.id,
                            name = itemStack.Item.name,
                            count = itemStack.Count
                        });
                    }
                    else
                        saveData.categorySlots[index].itemsStacks.Add(null);
                }
            }

            // capture equipment slots
            saveData.equipmentSlots = new List<EquipmentSlotSaveData>();
            for (int index = 0; index < equipmentSlots.Count; index++)
            {
                if (equipmentSlots[index]?.ItemStack != null)
                {
                    saveData.equipmentSlots.Add(new EquipmentSlotSaveData()
                    {
                        itemStack = new ItemStackSaveData()
                        {
                            itemId = equipmentSlots[index].ItemStack.Item.id,
                            name = equipmentSlots[index].ItemStack.Item.name,
                            count = equipmentSlots[index].ItemStack.Count
                        }
                    });
                }
                else
                    saveData.equipmentSlots.Add(null);
            }

            return saveData;
        }

        public void RestoreState(object state)
        {
            var saveData = (InventorySaveData)state;

            // Restore Items
            for (int index = 0; index < categorySlots.Count; index++)
            {
                categorySlots[index].itemSlots = new List<ItemStack>();
                foreach (var itemStack in saveData.categorySlots[index].itemsStacks)
                {
                    if (itemStack != null)
                    {
                        var item = ItemDatabase.i.GetObjectById(itemStack.itemId);
                        if (item == null)
                        {
                            Debug.LogError("Cannot Find Item " + itemStack.name);
                            continue;
                        }

                        categorySlots[index].itemSlots.Add(new ItemStack()
                        {
                            Item = item,
                            Count = itemStack.count
                        });
                    }
                    else
                        categorySlots[index].itemSlots.Add(null);
                }
            }

            // Restore Equipments Slots
            for (int index = 0; index < saveData.equipmentSlots.Count; index++)
            {
                var itemStackData = saveData.equipmentSlots[index]?.itemStack;
                if (itemStackData != null)
                {
                    var item = ItemDatabase.i.GetObjectById(itemStackData.itemId);
                    if (item == null)
                    {
                        Debug.LogError("Cannot Find Item " + itemStackData.name);
                        continue;
                    }

                    var itemStack = new ItemStack()
                    {
                        Item = item,
                        Count = itemStackData.count
                    };

                    SetItemToEquipmentSlot(itemStack, index, invokeEvent: false);
                }
                else
                {
                    RemoveItemFromEquipmentSlot(index, invokeEvent: false);
                }
            }


            ReCalculateInventoryWeight();
            ReCalculateEquipmentWeight();

            OnUpdated?.Invoke();
            OnEquipmentsUpdated?.Invoke();
        }

        public Type GetSavaDataType() => typeof(InventorySaveData);
    }

    [System.Serializable]
    public class InventorySaveData
    {
        public List<CategorySlotSaveData> categorySlots = new List<CategorySlotSaveData>();
        public List<EquipmentSlotSaveData> equipmentSlots = new List<EquipmentSlotSaveData>(); 
    }

    [System.Serializable]
    public class CategorySlotSaveData
    {
        public List<ItemStackSaveData> itemsStacks = new List<ItemStackSaveData>();
    }

    [System.Serializable]
    public class EquipmentSlotSaveData
    {
        public ItemStackSaveData itemStack;
    }

    [System.Serializable]
    public class ItemStackSaveData
    {
        public string itemId;
        public string name;
        public int count;
    }

    [Serializable]
    public class CategorySlot
    {
        public List<ItemCategory> categories = new List<ItemCategory>();
        public ItemCategory category => categories.FirstOrDefault();
        public List<ItemStack> itemSlots { get; set; }

        [SerializeField] bool multipleCategories;
        [SerializeField] string slotName = "";
        [SerializeField] Image slotImage;
        public Image SlotImage => slotImage;
        public string Name => multipleCategories && !String.IsNullOrEmpty(slotName)? slotName : categories.FirstOrDefault()?.Name;
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(CategorySlot))]
    public class CategorySlotDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty multipleCategories = property.FindPropertyRelative("multipleCategories");
            SerializedProperty categories = property.FindPropertyRelative("categories");
            SerializedProperty slotName = property.FindPropertyRelative("slotName");
            SerializedProperty slotImage = property.FindPropertyRelative("slotImage");

            // If we have a custom slot name and multiple categories enabled, use the slot name
            string displayName = "";
            if (!string.IsNullOrEmpty(slotName.stringValue) && multipleCategories.boolValue)
            {
                displayName = slotName.stringValue;
            }
            else if (categories.arraySize > 0)
            {
                // If we have categories and first category is valid, use its name
                var firstCategory = categories.GetArrayElementAtIndex(0).objectReferenceValue as ItemCategory;
                if (firstCategory != null)
                {
                    displayName = firstCategory.name;
                }
            }

            label = string.IsNullOrEmpty(displayName) ? label : new GUIContent(displayName);

            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                Rect toggleRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(toggleRect, multipleCategories);

                float nextY = position.y + EditorGUIUtility.singleLineHeight * 2;

                if (multipleCategories.boolValue)
                {
                    Rect slotNameRect = new Rect(position.x, nextY, position.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(slotNameRect, slotName);
                    nextY += EditorGUIUtility.singleLineHeight;

                    Rect slotImageRect = new Rect(position.x, nextY, position.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(slotImageRect, slotImage);
                    nextY += EditorGUIUtility.singleLineHeight;

                    Rect categoriesRect = new Rect(position.x, nextY, position.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(categoriesRect, categories, true);
                }
                else
                {
                    if (categories.arraySize > 0)
                    {
                        SerializedProperty firstCategory = categories.GetArrayElementAtIndex(0);
                        Rect categoryRect = new Rect(position.x, nextY, position.width, EditorGUIUtility.singleLineHeight);
                        EditorGUI.PropertyField(categoryRect, firstCategory, new GUIContent("Category"));
                    }
                    else
                    {
                        categories.arraySize++;
                    }
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                height += EditorGUIUtility.singleLineHeight; // For the toggle
                SerializedProperty multipleCategories = property.FindPropertyRelative("multipleCategories");
                if (multipleCategories.boolValue)
                {
                    height += EditorGUIUtility.singleLineHeight * 2; // For slotName and slotImage
                    height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("categories"), true);
                }
                else
                {
                    SerializedProperty categories = property.FindPropertyRelative("categories");
                    if (categories.arraySize > 0)
                    {
                        height += EditorGUIUtility.singleLineHeight;
                    }
                }
            }
            return height;
        }
    }

#endif
}
