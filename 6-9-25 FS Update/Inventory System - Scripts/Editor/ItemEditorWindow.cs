#if UNITY_EDITOR
using FS_Core;
using FS_Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace FS_InventorySystem
{
    public class ItemEditorWindow : EditorWindow
    {
        private enum Tab { Category, Item, EquipmentSlots, Currency, Setup }
        private Tab selectedTab = Tab.Category;

        private Vector2 scrollPosition;

        FSObjectListView<ItemCategory> categoryListView = new FSObjectListView<ItemCategory>();
        FSObjectListView<Item> itemListView = new FSObjectListView<Item>();

        EquipmentSlotsDatabase equipmentSlots;
        CurrencyDatabase currencies;

        SerializedObject currencySO;
        SerializedObject equipmentSlotsSO;

        SerializedObject itemSO;
        Item currentItem;

        //string duplicateAttributName;

        GUIStyle containerStyle, subContainerStyle, headerStyle;

        //string[] tabNames = { "Categories", "Items", "Equipment Slots", "Currencies", "Merchant" };

        [MenuItem("Tools/Inventory System/Inventory Editor", false, 2)]
        public static void ShowWindow()
        {
            var window = GetWindow<ItemEditorWindow>("Item Editor");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void OnEnable()
        {
            categoryListView = new FSObjectListView<ItemCategory>();
            itemListView = new FSObjectListView<Item>();

            categoryListView.LoadObjects();
            itemListView.LoadObjects();

            equipmentSlots = Resources.LoadAll<EquipmentSlotsDatabase>("").FirstOrDefault(); 
            currencies = Resources.LoadAll<CurrencyDatabase>("").FirstOrDefault();

            currencySO = new SerializedObject(currencies);
            equipmentSlotsSO = new SerializedObject(equipmentSlots);
        }

        private void OnGUI()
        {
            // Initialize styles
            containerStyle = new GUIStyle();
            containerStyle.margin = new RectOffset(5, 5, 10, 5);

            subContainerStyle = new GUIStyle();

            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;

            categoryListView.headerStyle = headerStyle;
            categoryListView.containerStyle = subContainerStyle;

            itemListView.headerStyle = headerStyle;
            itemListView.containerStyle = subContainerStyle;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                float windowWidth = position.width;

                // Create custom toolbar button style
                GUIStyle toolbarButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
                {
                    fixedHeight = 28,
                    fontSize = 14,  // Slightly larger for readability
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(15, 15, 6, 6)  // Adjusted padding for better centering
                };

                // Define colors
                Color defaultColor = GUI.backgroundColor;
                Color selectedColor = EditorGUIUtility.isProSkin
                    ? new Color(0.15f, 0.45f, 0.75f, 1f)  // Deeper blue for dark theme
                    : new Color(0.4f, 0.65f, 0.9f, 1f);   // Softer blue for light theme

                // Create a toolbar layout
                using (new EditorGUILayout.HorizontalScope())
                {
                    var tabValues = Enum.GetValues(typeof(Tab));

                    for (int i = 0; i < tabValues.Length; i++)
                    {
                        Tab currentTab = (Tab)tabValues.GetValue(i);
                        bool isSelected = selectedTab == currentTab;
                        GUI.backgroundColor = isSelected ? selectedColor : defaultColor;

                        // Convert enum value to display string
                        string displayName = ObjectNames.NicifyVariableName(currentTab.ToString());

                        if (GUILayout.Button(displayName, toolbarButtonStyle,
                            GUILayout.Height(28),
                            GUILayout.Width((windowWidth - 10) / tabValues.Length)))
                        {
                            selectedTab = currentTab;
                        }
                    }
                }

                GUI.backgroundColor = defaultColor;
                EditorGUILayout.Space(2);




                using (new EditorGUILayout.HorizontalScope(containerStyle))
                {
                    switch (selectedTab)
                    {
                        case Tab.Category:
                            categoryListView.ListObjects(position);
                            break;
                        case Tab.Item:
                            itemListView.ListObjects(position);
                            break;
                    }

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                    switch (selectedTab)
                    {
                        case Tab.Category:
                            DrawCategoryEditor();
                            break;
                        case Tab.Item:
                            DrawItemEditor();
                            break;
                        case Tab.EquipmentSlots:
                            DrawEquipmentSlots();
                            break;
                        case Tab.Currency:
                            DrawCurrency();
                            break;
                        case Tab.Setup:
                            DrawSetup();
                            break;
                    }

                    EditorGUILayout.EndScrollView();
                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void DrawCategoryEditor()
        {
            using (new EditorGUILayout.HorizontalScope(containerStyle))
            {
                var selectedCategory = categoryListView.SelectedObject;

                // Categories Details
                using (new EditorGUILayout.VerticalScope(subContainerStyle, GUILayout.Width(position.width * (2 / 3))))
                {
                    // Header with background
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(5);
                    EditorGUILayout.LabelField("Category Details", headerStyle);
                    GUILayout.Space(5);
                    EditorGUILayout.EndVertical();

                    if (selectedCategory != null)
                    {
                        // Create serialized object for property fields
                        SerializedObject serializedCategory = new SerializedObject(selectedCategory);
                        serializedCategory.Update();

                        // Create a section style
                        GUIStyle sectionStyle = new GUIStyle(EditorStyles.helpBox);

                        // Basic Info Section
                        EditorGUILayout.BeginVertical(sectionStyle);
                        EditorGUILayout.LabelField("Basic Information", EditorStyles.boldLabel);
                        GUILayout.Space(5);

                        // Using PropertyField for better serialization
                        SerializedProperty nameProp = serializedCategory.FindProperty("name");
                        SerializedProperty descProp = serializedCategory.FindProperty("description");
                        SerializedProperty iconProp = serializedCategory.FindProperty("icon");

                        EditorGUILayout.PropertyField(nameProp, new GUIContent("Name"));
                        EditorGUILayout.PropertyField(descProp, new GUIContent("Description"));
                        selectedCategory.icon = (Sprite)EditorGUILayout.ObjectField("Icon", selectedCategory.icon, typeof(Sprite), false);

                        EditorGUILayout.EndVertical();

                        // Item Properties Section
                        EditorGUILayout.BeginVertical(sectionStyle);
                        EditorGUILayout.LabelField("Item Properties", EditorStyles.boldLabel);
                        GUILayout.Space(5);

                        SerializedProperty stackableProp = serializedCategory.FindProperty("stackable");
                        SerializedProperty equippableProp = serializedCategory.FindProperty("equippable");
                        SerializedProperty consumableProp = serializedCategory.FindProperty("consumable");
                        SerializedProperty sellableProp = serializedCategory.FindProperty("sellable");
                        SerializedProperty droppableProp = serializedCategory.FindProperty("droppable");

                        EditorGUILayout.PropertyField(stackableProp);
                        EditorGUILayout.PropertyField(equippableProp);

                        if (equippableProp.boolValue)
                        {
                            EditorGUI.indentLevel++;
                            if (equipmentSlots != null)
                            {
                                var slots = equipmentSlots.EquipmentSlotList;

                                SerializedProperty slotIndexProp = serializedCategory.FindProperty("equipmentSlotIndex");
                                int newSelectedIndex = EditorGUILayout.Popup("Default Equipment Slot",
                                    slotIndexProp.intValue, slots.ToArray());

                                if (newSelectedIndex != slotIndexProp.intValue)
                                {
                                    slotIndexProp.intValue = newSelectedIndex;
                                }
                            }
                            else
                            {
                                EditorGUILayout.HelpBox("Equipment slots missing", MessageType.Error);
                            }
                            EditorGUI.indentLevel--;
                            GUILayout.Space(5);
                        }

                        EditorGUILayout.PropertyField(consumableProp);
                        EditorGUILayout.PropertyField(sellableProp);
                        EditorGUILayout.PropertyField(droppableProp);
                        EditorGUILayout.EndVertical();

                        // Attributes Section
                        EditorGUILayout.BeginVertical(sectionStyle);

                        EditorGUILayout.LabelField("Attributes", EditorStyles.boldLabel);
                        //GUILayout.Space(5);

                        // Get the attributes property
                        SerializedProperty attributesProp = serializedCategory.FindProperty("attributes");

                        // Initialize attributes list if needed
                        if (attributesProp.arraySize == 0 && selectedCategory.attributes == null)
                        {
                            selectedCategory.attributes = new List<ItemAttribute>();
                            serializedCategory.Update();
                        }

                        // Display each attribute with property fields
                        for (int i = 0; i < attributesProp.arraySize; i++)
                        {
                            SerializedProperty attributeProp = attributesProp.GetArrayElementAtIndex(i);
                            SerializedProperty attributeNameProp = attributeProp.FindPropertyRelative("attributeName");
                            SerializedProperty attributeTypeProp = attributeProp.FindPropertyRelative("attributeType");

                            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                            EditorGUILayout.PropertyField(attributeNameProp, GUIContent.none, GUILayout.ExpandWidth(true));
                            EditorGUILayout.PropertyField(attributeTypeProp, GUIContent.none, GUILayout.Width(100));

                            GUIStyle deleteButtonStyle = new GUIStyle(EditorStyles.miniButtonRight)
                            {
                                normal = { textColor = Color.red },
                                fontStyle = FontStyle.Bold,
                                padding = new RectOffset(0, 0, 0, 0),
                                alignment = TextAnchor.MiddleCenter,
                                fixedHeight = EditorGUIUtility.singleLineHeight
                            };

                            if (GUILayout.Button(new GUIContent("×", "Remove Attribute"),
                                deleteButtonStyle,
                                GUILayout.Width(24),
                                GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                            {
                                attributesProp.DeleteArrayElementAtIndex(i);
                                EditorGUILayout.EndHorizontal();
                                break; // Break to prevent modifying collection during iteration
                            }

                            EditorGUILayout.EndHorizontal();
                        }
                        if (!string.IsNullOrEmpty(selectedCategory.duplicateAttributName))
                            EditorGUILayout.HelpBox($"Duplicate attributes found in category {selectedCategory.duplicateAttributName}. Please ensure all attributes are unique, otherwise it may cause unexpected issues when updating attribute values.", MessageType.Error);
                        // Add button
                        EditorGUILayout.Space();
                        if (GUILayout.Button("Add Attribute", GUILayout.Width(120)))
                        {
                            int newIndex = attributesProp.arraySize;
                            attributesProp.InsertArrayElementAtIndex(newIndex);

                            SerializedProperty newAttribute = attributesProp.GetArrayElementAtIndex(newIndex);
                            SerializedProperty newAttributeName = newAttribute.FindPropertyRelative("attributeName");
                            SerializedProperty newAttributeType = newAttribute.FindPropertyRelative("attributeType");

                            newAttributeName.stringValue = $"New Attribute {newIndex}";
                            newAttributeType.enumValueIndex = (int)ItemAttributeType.Integer;
                            serializedCategory.ApplyModifiedProperties();
                        }
                        EditorGUILayout.EndVertical();

                        foreach (var item in itemListView.allObjects)
                        {
                            selectedCategory.UpdateItemAttributeBasedOnCategory(item, () => Repaint());
                        }
                        // Action Section
                        EditorGUILayout.BeginVertical(sectionStyle);
                        EditorGUILayout.LabelField("Action", EditorStyles.boldLabel);
                        GUILayout.Space(5);

                        SerializedProperty actionProp = serializedCategory.FindProperty("itemAction");

                        if (actionProp.objectReferenceValue != null)
                        {
                            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                            GUILayout.Label(EditorGUIUtility.IconContent("d_RotateTool"), GUILayout.Width(20), GUILayout.Height(20));
                            EditorGUILayout.LabelField(selectedCategory.itemAction.name, EditorStyles.boldLabel);

                            if (GUILayout.Button("Change", EditorStyles.miniButton, GUILayout.Width(70)))
                            {
                                ItemActionSelectorWindow.Show(selectedCategory);
                            }
                            EditorGUILayout.EndHorizontal();

                            // Show Action Properties in a sub-section
                            if (selectedCategory.itemAction != null)
                            {
                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                GUILayout.Space(2);
                                EditorGUILayout.LabelField("Action Properties", EditorStyles.miniLabel);
                                GUILayout.Space(2);

                                SerializedObject actionSO = new SerializedObject(selectedCategory.itemAction);
                                actionSO.Update();

                                SerializedProperty prop = actionSO.GetIterator();
                                prop.NextVisible(true);

                                while (prop.NextVisible(false))
                                {
                                    EditorGUILayout.PropertyField(prop, true);
                                }

                                if (actionSO.ApplyModifiedProperties())
                                {
                                    EditorUtility.SetDirty(selectedCategory.itemAction);
                                }

                                EditorGUILayout.EndVertical();
                            }
                        }
                        else
                        {
                            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                            GUILayout.Label(EditorGUIUtility.IconContent("d_console.warnicon"), GUILayout.Width(20), GUILayout.Height(20));
                            EditorGUILayout.LabelField("No Action Assigned", EditorStyles.boldLabel);

                            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                            buttonStyle.normal.textColor = Color.blue;

                            if (GUILayout.Button("Assign Action", buttonStyle, GUILayout.Width(100)))
                            {
                                ItemActionSelectorWindow.Show(selectedCategory);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();

                        // Apply all modified properties
                        if (serializedCategory.ApplyModifiedProperties())
                        {
                            EditorUtility.SetDirty(selectedCategory);
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Select a category to edit its properties", MessageType.Info);
                    }
                }
            }
        }

        private void DrawItemEditor()
        {
            using (new EditorGUILayout.HorizontalScope(containerStyle))
            {
                var selectedItem = itemListView.SelectedObject;

                // Item Details Panel
                using (new EditorGUILayout.VerticalScope(subContainerStyle, GUILayout.Width(position.width * (2 / 3))))
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(5);
                    EditorGUILayout.LabelField("Item Details", headerStyle);
                    GUILayout.Space(5);
                    EditorGUILayout.EndVertical();

                    if (selectedItem != null)
                    {
                        if (currentItem != selectedItem)
                        {
                            itemSO = new SerializedObject(selectedItem);
                            currentItem = selectedItem;
                        }
                        else if (itemSO == null)
                            itemSO = new SerializedObject(selectedItem);

                        itemSO.Update();
                        Undo.RecordObject(selectedItem, "Modify Item");

                        // Basic Properties Section
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.LabelField("General Properties", EditorStyles.boldLabel);

                        EditorGUILayout.PropertyField(itemSO.FindProperty("name"), new GUIContent("Name"));

                        EditorGUI.BeginChangeCheck();
                        var categoryProp = itemSO.FindProperty("category");

                        EditorGUILayout.PropertyField(categoryProp, new GUIContent("Category"));
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (categoryProp.objectReferenceValue == null)
                                categoryProp.objectReferenceValue = Resources.Load<ItemCategory>("Category/Default Category");

                            selectedItem.category = (ItemCategory)categoryProp.objectReferenceValue;
                        }

                        EditorGUILayout.PropertyField(itemSO.FindProperty("description"), new GUIContent("Description"));
                        selectedItem.icon = (Sprite)EditorGUILayout.ObjectField("Icon", selectedItem.icon, typeof(Sprite), false);
                        EditorGUILayout.PropertyField(itemSO.FindProperty("weight"), new GUIContent("Weight"));
                        EditorGUILayout.EndVertical();

                        GUILayout.Space(10);

                        if (categoryProp.objectReferenceValue != null)
                        {
                            // Equipment Section
                            if (selectedItem.category != null && selectedItem.category.equippable)
                            {
                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                EditorGUILayout.LabelField("Equipment Properties", EditorStyles.boldLabel);

                                if (equipmentSlots != null)
                                {
                                    if (selectedItem.equipmentSlotIndex == -1)
                                        selectedItem.equipmentSlotIndex = selectedItem.category.equipmentSlotIndex;

                                    var slots = equipmentSlots.EquipmentSlotList;
                                    string[] slotNames = slots.ToArray();

                                    EditorGUI.BeginChangeCheck();
                                    int newSelectedIndex = EditorGUILayout.Popup("Equipment Slot", selectedItem.equipmentSlotIndex, slotNames);
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        Undo.RecordObject(selectedItem, "Changed Equipment Slot");
                                        selectedItem.equipmentSlotIndex = newSelectedIndex;
                                        EditorUtility.SetDirty(selectedItem);
                                    }
                                }
                                else
                                {
                                    EditorGUILayout.HelpBox("Equipment slots configuration missing", MessageType.Error);
                                    if (GUILayout.Button("Create Equipment Slots Asset"))
                                    {
                                        // Add functionality to create equipment slots
                                    }
                                }

                                var attachModelProp = itemSO.FindProperty("attachModel");
                                EditorGUILayout.PropertyField(attachModelProp);

                                if (attachModelProp.boolValue)
                                {
                                    EditorGUILayout.PropertyField(itemSO.FindProperty("modelPrefab"));
                                    var isSkinnedMesh = itemSO.FindProperty("isSkinnedMesh");
                                    EditorGUILayout.PropertyField(isSkinnedMesh);
                                    if (!isSkinnedMesh.boolValue)
                                    {
                                        EditorGUILayout.PropertyField(itemSO.FindProperty("localPosition"));
                                        EditorGUILayout.PropertyField(itemSO.FindProperty("localRotation"));
                                    }
                                }

                                EditorGUILayout.EndVertical();
                            }

                            GUILayout.Space(10);

                            // Category Override Section
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.LabelField("Category Settings", EditorStyles.boldLabel);

                            SerializedProperty overrideCategorySettingsProp = itemSO.FindProperty("overrideCategorySettings");
                            EditorGUILayout.PropertyField(overrideCategorySettingsProp, new GUIContent("Override Settings"));

                            if (selectedItem.overrideCategorySettings)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(itemSO.FindProperty("sellable"), new GUIContent("Sellable"));
                                EditorGUILayout.PropertyField(itemSO.FindProperty("droppable"), new GUIContent("Droppable"));
                                EditorGUI.indentLevel--;
                            }
                            EditorGUILayout.EndVertical();

                            GUILayout.Space(10);

                            // Price Section
                            if (selectedItem.Sellable)
                            {
                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                EditorGUILayout.LabelField("Pricing", EditorStyles.boldLabel);

                                ShowCurrencyAndAmount(selectedItem.price, selectedItem);

                                //SerializedProperty requiresMultipleCurrenciesProp = itemSO.FindProperty("requiresMultipleCurrencies");
                                //EditorGUILayout.PropertyField(requiresMultipleCurrenciesProp);

                                //if (selectedItem.requiresMultipleCurrencies)
                                //{
                                //    // For future implementation
                                //    EditorGUILayout.HelpBox("Multiple currency support coming soon", MessageType.Info);
                                //}
                                //else
                                //{
                                //    ShowCurrencyAndAmount(selectedItem.price, selectedItem);
                                //}
                                EditorGUILayout.EndVertical();
                            }

                            GUILayout.Space(10);

                            // Attributes Section
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.LabelField("Attributes", EditorStyles.boldLabel);

                            SerializedProperty overrideCategoryAttributesProp = itemSO.FindProperty("overrideCategoryAttributes");
                            EditorGUILayout.PropertyField(overrideCategoryAttributesProp, new GUIContent("Override Attributes", "Override Category Attributes"));

                            if (selectedItem.overrideCategoryAttributes)
                            {
                                if (selectedItem.overrideAttributes != null)
                                {
                                    SerializedProperty overrideAttributesProp = itemSO.FindProperty("overrideAttributes");
                                    EditorGUILayout.Space(5);

                                    for (int i = 0; i < overrideAttributesProp.arraySize; i++)
                                    {
                                        SerializedProperty attributeProp = overrideAttributesProp.GetArrayElementAtIndex(i);

                                        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                                        {
                                            // Name field with fixed width
                                            EditorGUILayout.PropertyField(attributeProp.FindPropertyRelative("attributeName"),
                                                GUIContent.none, GUILayout.Width(150));

                                            //GUILayout.Space(5);

                                            // Type field with fixed width
                                            EditorGUILayout.PropertyField(attributeProp.FindPropertyRelative("attributeType"),
                                                GUIContent.none, GUILayout.Width(100));

                                            //GUILayout.Space(5);

                                            // Value field based on type
                                            SerializedProperty typeProp = attributeProp.FindPropertyRelative("attributeType");
                                            ItemAttributeType attrType = (ItemAttributeType)typeProp.enumValueIndex;

                                            switch (attrType)
                                            {
                                                case ItemAttributeType.Integer:
                                                    EditorGUILayout.PropertyField(attributeProp.FindPropertyRelative("intValue"),
                                                        GUIContent.none, GUILayout.ExpandWidth(true));
                                                    break;
                                                case ItemAttributeType.Decimal:
                                                    EditorGUILayout.PropertyField(attributeProp.FindPropertyRelative("floatValue"),
                                                        GUIContent.none, GUILayout.ExpandWidth(true));
                                                    break;
                                                case ItemAttributeType.Text:
                                                    EditorGUILayout.PropertyField(attributeProp.FindPropertyRelative("stringValue"),
                                                        GUIContent.none, GUILayout.ExpandWidth(true));
                                                    break;
                                            }

                                            GUILayout.FlexibleSpace();

                                            // Delete button
                                            GUIStyle deleteButtonStyle = new GUIStyle(EditorStyles.miniButtonRight)
                                            {
                                                normal = { textColor = Color.red },
                                                fontStyle = FontStyle.Bold,
                                                padding = new RectOffset(0, 0, 0, 0),
                                                alignment = TextAnchor.MiddleCenter,
                                                fixedHeight = EditorGUIUtility.singleLineHeight
                                            };

                                            if (GUILayout.Button(new GUIContent("×", "Delete Attribute"),
                                                deleteButtonStyle, GUILayout.Width(24)))
                                            {
                                                Undo.RecordObject(selectedItem, "Removed Attribute");
                                                overrideAttributesProp.DeleteArrayElementAtIndex(i);
                                                break; // Prevent modifying the list while iterating
                                            }
                                        }

                                        GUILayout.Space(2);
                                    }

                                    if (GUILayout.Button("Add Attribute", GUILayout.Width(120)))
                                    {
                                        Undo.RecordObject(selectedItem, "Added Attribute");
                                        overrideAttributesProp.arraySize++;
                                        SerializedProperty newAttr = overrideAttributesProp.GetArrayElementAtIndex(overrideAttributesProp.arraySize - 1);
                                        newAttr.FindPropertyRelative("attributeName").stringValue = "New Attribute";
                                        newAttr.FindPropertyRelative("attributeType").enumValueIndex = (int)ItemAttributeType.Integer;
                                        newAttr.FindPropertyRelative("intValue").intValue = 0;
                                    }
                                }

                            }
                            else
                            {
                                if (selectedItem.category.attributes != null)
                                {
                                    //test
                                    selectedItem.category.UpdateItemAttributeBasedOnCategory(selectedItem);
                                    foreach (var attribute in selectedItem.attributeValues)
                                    {
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField(attribute.attributeName, GUILayout.Width(150));

                                        switch (attribute.attributeType)
                                        {
                                            case ItemAttributeType.Integer:
                                                attribute.intValue = EditorGUILayout.IntField(attribute.intValue);
                                                break;
                                            case ItemAttributeType.Decimal:
                                                attribute.floatValue = EditorGUILayout.FloatField(attribute.floatValue);
                                                break;
                                            case ItemAttributeType.Text:
                                                attribute.stringValue = EditorGUILayout.TextField(attribute.stringValue);
                                                break;
                                        }
                                        EditorGUILayout.EndHorizontal();
                                    }
                                }
                            }

                            EditorGUILayout.EndVertical();

                            GUILayout.Space(10);

                            // Crafting Section
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.LabelField("Crafting", EditorStyles.boldLabel);

                            // Ingredients List Header
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                            EditorGUILayout.LabelField("Ingredients", EditorStyles.boldLabel);
                            SerializedProperty ingredientsProp = itemSO.FindProperty("ingredients");
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            // Column Headers
                            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                            {
                                GUILayout.Space(15); // Indent space
                                GUILayout.Label("Item", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
                                GUILayout.FlexibleSpace();
                                GUILayout.Label("Count", EditorStyles.boldLabel, GUILayout.Width(50));
                                GUILayout.Space(24); // Space for delete button
                            }
                            // Display each ingredient
                            for (int i = 0; i < ingredientsProp.arraySize; i++)
                            {
                                SerializedProperty ingredientProp = ingredientsProp.GetArrayElementAtIndex(i);
                                SerializedProperty itemProp = ingredientProp.FindPropertyRelative("item");
                                SerializedProperty countProp = ingredientProp.FindPropertyRelative("count");

                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    // Item field
                                    EditorGUILayout.PropertyField(itemProp, GUIContent.none, GUILayout.ExpandWidth(true));

                                    GUILayout.Space(5);

                                    // Count field
                                    EditorGUILayout.PropertyField(countProp, GUIContent.none, GUILayout.Width(50));

                                    GUILayout.Space(5);

                                    // Delete button
                                    GUIStyle deleteButtonStyle = new GUIStyle(EditorStyles.miniButtonRight)
                                    {
                                        normal = { textColor = Color.red },
                                        fontStyle = FontStyle.Bold,
                                        padding = new RectOffset(0, 0, 0, 0),
                                        alignment = TextAnchor.MiddleCenter,
                                        fixedHeight = EditorGUIUtility.singleLineHeight
                                    };

                                    if (GUILayout.Button(new GUIContent("×", "Remove Ingredient"),
                                        deleteButtonStyle,
                                        GUILayout.Width(24),
                                        GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                                    {
                                        ingredientsProp.DeleteArrayElementAtIndex(i);
                                        break;
                                    }
                                }

                                if (i < ingredientsProp.arraySize - 1)
                                {
                                    GUILayout.Space(2);
                                }
                            }

                            EditorGUILayout.EndVertical();



                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            GUIStyle addButtonStyle = new GUIStyle(EditorStyles.miniButton)
                            {
                                padding = new RectOffset(8, 8, 2, 2)
                            };
                            if (GUILayout.Button("Add Ingredient", addButtonStyle, GUILayout.Width(120)))
                            {
                                ingredientsProp.arraySize++;
                                SerializedProperty newIngredient = ingredientsProp.GetArrayElementAtIndex(ingredientsProp.arraySize - 1);
                                newIngredient.FindPropertyRelative("count").intValue = 1;
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();


                            SerializedProperty canCraftProp = itemSO.FindProperty("canCraft");
                            EditorGUILayout.PropertyField(canCraftProp);

                            if (selectedItem.canCraft)
                            {
                                ShowCurrencyAndAmount(selectedItem.priceToCraft, selectedItem, "Price to craft");
                            }

                            SerializedProperty canDismantleProp = itemSO.FindProperty("canDismantle");
                            EditorGUILayout.PropertyField(canDismantleProp);

                            if (selectedItem.canDismantle)
                            {
                                ShowCurrencyAndAmount(selectedItem.priceToDismantle, selectedItem, "Price to dismantle");
                            }

                            EditorGUILayout.EndVertical();
                            GUILayout.Space(10);

                            // Action Section
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.LabelField("Item Actions", EditorStyles.boldLabel);

                            SerializedProperty overrideActionProp = itemSO.FindProperty("overrideAction");
                            EditorGUILayout.PropertyField(overrideActionProp);

                            if (selectedItem.overrideAction)
                            {
                                SerializedProperty itemActionProp = itemSO.FindProperty("itemAction");

                                if (itemActionProp.objectReferenceValue != null)
                                {
                                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                                    GUILayout.Label(EditorGUIUtility.IconContent("d_RotateTool"), GUILayout.Width(20), GUILayout.Height(20));
                                    EditorGUILayout.LabelField(selectedItem.itemAction.name, EditorStyles.boldLabel);

                                    if (GUILayout.Button("Change", EditorStyles.miniButton, GUILayout.Width(70)))
                                    {
                                        ItemActionSelectorWindow.Show(selectedItem);
                                    }
                                    EditorGUILayout.EndHorizontal();

                                    // Show Action Properties in a sub-section
                                    if (selectedItem.itemAction != null)
                                    {
                                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                        GUILayout.Space(2);
                                        EditorGUILayout.LabelField("Action Properties", EditorStyles.miniLabel);
                                        GUILayout.Space(2);

                                        SerializedObject actionSO = new SerializedObject(selectedItem.itemAction);
                                        actionSO.Update();

                                        SerializedProperty prop = actionSO.GetIterator();
                                        prop.NextVisible(true);

                                        while (prop.NextVisible(false))
                                        {
                                            EditorGUILayout.PropertyField(prop, true);
                                        }

                                        if (actionSO.ApplyModifiedProperties())
                                        {
                                            EditorUtility.SetDirty(selectedItem.itemAction);
                                        }

                                        EditorGUILayout.EndVertical();
                                    }
                                }
                                else
                                {
                                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                                    GUILayout.Label(EditorGUIUtility.IconContent("d_console.warnicon"), GUILayout.Width(20), GUILayout.Height(20));
                                    EditorGUILayout.LabelField("No Action Assigned", EditorStyles.boldLabel);

                                    GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                                    buttonStyle.normal.textColor = Color.blue;

                                    if (GUILayout.Button("Assign Action", buttonStyle, GUILayout.Width(100)))
                                    {
                                        ItemActionSelectorWindow.Show(selectedItem);
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                            EditorGUILayout.EndVertical();
                        }

                        itemSO.ApplyModifiedProperties();
                        EditorUtility.SetDirty(selectedItem);
                    }
                    else
                    {
                        // No item selected
                        EditorGUILayout.HelpBox("Select an item to view and edit its properties", MessageType.Info);
                    }
                }
            }
        }

        private void DrawEquipmentSlots()
        {
            using (new EditorGUILayout.VerticalScope(containerStyle))
            {
                //EditorGUILayout.LabelField("Equipment Slots", headerStyle);
                if (equipmentSlots != null)
                {
                    // Make sure to create the reorderableList if not created yet
                    if (reorderableEquipmentSlots == null)
                    {
                        InitializeReorderableList();
                    }

                    equipmentSlotsSO.Update();

                    // Draw the reorderable list
                    reorderableEquipmentSlots.DoLayoutList();

                    equipmentSlotsSO.ApplyModifiedProperties();
                }
                else
                {
                    EditorGUILayout.HelpBox("Equipment Slots Object is missing. Please create a new one.", MessageType.Warning);
                    if (GUILayout.Button("Create Equipment Slots Object"))
                    {
                        CreateEquipmentSlotsObject();
                    }
                }
            }
        }

        private void DrawCurrency()
        {
            using (new EditorGUILayout.VerticalScope(containerStyle))
            {
                EditorGUILayout.LabelField("Currencies", headerStyle);

                if (currencies != null)
                {
                    currencySO.Update();
                    SerializedProperty currencyListProp = currencySO.FindProperty("currencyList");

                    // Custom header for the currency list
                    // Header row with consistent widths
                    // Header row
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                    {
                        float totalWidth = EditorGUIUtility.currentViewWidth - 40f; // Account for margins
                        float nameWidth = totalWidth * 0.4f;
                        float symbolWidth = totalWidth * 0.25f;
                        float iconWidth = totalWidth * 0.25f;
                        float deleteWidth = 24f;

                        GUILayout.Label("Name", EditorStyles.boldLabel, GUILayout.Width(nameWidth));
                        GUILayout.Label("Symbol", EditorStyles.boldLabel, GUILayout.Width(symbolWidth));
                        GUILayout.Label("Icon", EditorStyles.boldLabel, GUILayout.Width(iconWidth));
                        GUILayout.Label("", GUILayout.ExpandWidth(true)); // Placeholder for delete button
                    }

                    // Display each currency
                    for (int i = 0; i < currencyListProp.arraySize; i++)
                    {
                        SerializedProperty currencyProp = currencyListProp.GetArrayElementAtIndex(i);
                        SerializedProperty nameProp = currencyProp.FindPropertyRelative("name");
                        SerializedProperty symbolProp = currencyProp.FindPropertyRelative("symbol");
                        SerializedProperty iconProp = currencyProp.FindPropertyRelative("icon");

                        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                        {
                            float totalWidth = EditorGUIUtility.currentViewWidth - 40f; // Account for margins
                            float padding = 5f;
                            float nameWidth = (totalWidth * 0.4f) - padding;
                            float symbolWidth = (totalWidth * 0.25f) - padding;
                            float iconWidth = (totalWidth * 0.25f) - padding;
                            float deleteWidth = 24f;

                            // Name field
                            EditorGUILayout.PropertyField(nameProp, GUIContent.none, GUILayout.Width(nameWidth));
                            GUILayout.Space(padding);

                            // Symbol field
                            EditorGUILayout.PropertyField(symbolProp, GUIContent.none, GUILayout.Width(symbolWidth));
                            GUILayout.Space(padding);

                            // Icon field
                            EditorGUILayout.PropertyField(iconProp, GUIContent.none, GUILayout.ExpandWidth(true));
                            GUILayout.Space(padding);

                            // Delete button
                            GUIStyle deleteButtonStyle = new GUIStyle(EditorStyles.miniButtonRight)
                            {
                                normal = { textColor = Color.red },
                                fontStyle = FontStyle.Bold,
                                padding = new RectOffset(0, 0, 0, 0),
                                alignment = TextAnchor.MiddleCenter,
                                fixedHeight = EditorGUIUtility.singleLineHeight
                            };

                            if (GUILayout.Button(new GUIContent("×", "Delete Currency"),
                                deleteButtonStyle,
                                GUILayout.Width(deleteWidth),
                                GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                            {
                                if (EditorUtility.DisplayDialog("Delete Currency",
                                    "Are you sure you want to delete this currency?", "Delete", "Cancel"))
                                {
                                    currencyListProp.DeleteArrayElementAtIndex(i);
                                    i--;
                                }
                            }
                        }

                        GUILayout.Space(2);
                    }




                    // Add new currency button
                    EditorGUILayout.Space(10);
                    if (GUILayout.Button("Add New Currency", GUILayout.Height(30)))
                    {
                        currencyListProp.arraySize++;
                        SerializedProperty newCurrency = currencyListProp.GetArrayElementAtIndex(currencyListProp.arraySize - 1);

                        // Set default values for the new currency
                        SerializedProperty newNameProp = newCurrency.FindPropertyRelative("name");
                        if (newNameProp != null)
                            newNameProp.stringValue = "New Currency";
                    }

                    // Note about symbols and icons
                    EditorGUILayout.Space(15);
                    EditorGUILayout.HelpBox("Symbol and Icon of the currency are optional. Only use them if you want to show them in your game.", MessageType.Info);

                    currencySO.ApplyModifiedProperties();
                }
                else
                {
                    EditorGUILayout.HelpBox("Currency list is missing. Please create a new one.", MessageType.Warning);
                    if (GUILayout.Button("Create Currency List", GUILayout.Height(30)))
                    {
                        CreateCurrenciesObject();
                    }
                }
            }
        }

        private GameObject playerObject;

        private void DrawSetup()
        {
            using (new EditorGUILayout.VerticalScope(containerStyle))
            {
                EditorGUILayout.LabelField("Inventory System Setup", headerStyle);
                EditorGUILayout.Space(10);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Player Setup", EditorStyles.boldLabel);
                EditorGUILayout.Space(5);

                // Player object field with Undo
                EditorGUI.BeginChangeCheck();
                GameObject newPlayerObject = EditorGUILayout.ObjectField(
                    new GUIContent("Player GameObject", "Assign your player GameObject here"),
                    playerObject, typeof(GameObject), true) as GameObject;

                if (EditorGUI.EndChangeCheck())
                {
                    if (newPlayerObject != null && PrefabUtility.GetPrefabAssetType(newPlayerObject) != PrefabAssetType.NotAPrefab)
                    {
                        EditorUtility.DisplayDialog("Invalid Selection",
                            "Please select a GameObject from the scene, not a prefab.", "OK");
                    }
                    else
                    {
                        Undo.RecordObject(this, "Change Player Object");
                        playerObject = newPlayerObject;
                    }
                }

                if (playerObject == null)
                {
                    EditorGUILayout.HelpBox("Please assign a player GameObject to setup the inventory system.",
                        MessageType.Info);
                }
                else
                {
                    bool hasInventory = playerObject.GetComponent<Inventory>();
                    bool hasWallet = playerObject.GetComponent<Wallet>();
                    bool hasInput = playerObject.GetComponent<InventoryInputManager>();
                    bool hasAllComponents = hasInventory && hasWallet;
                    bool hasCanvas = playerObject.transform.parent?.GetComponentsInChildren<Canvas>()?.FirstOrDefault(c => c.gameObject.name == "Inventory Canvas") != null;

                    if (!hasCanvas)
                        hasCanvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None).FirstOrDefault(c => c.gameObject.name == "Inventory Canvas") != null;

                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Component Status:", EditorStyles.boldLabel);

                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        GUI.enabled = false;
                        EditorGUILayout.Toggle("Inventory Component", hasInventory);
                        EditorGUILayout.Toggle("Wallet Component", hasWallet);
                        EditorGUILayout.Toggle("Inventory Canvas", hasCanvas);
                        EditorGUILayout.Toggle("Inventory Canvas", hasInput);
                        GUI.enabled = true;
                    }

                    EditorGUILayout.Space(10);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUI.enabled = !hasAllComponents || !hasCanvas;

                        if (GUILayout.Button("Setup Inventory System", GUILayout.Height(30)))
                        {
                            // Create Undo group to combine all operations
                            Undo.IncrementCurrentGroup();
                            Undo.SetCurrentGroupName("Setup Inventory System");
                            int undoGroup = Undo.GetCurrentGroup();

                            SetupInventorySystem();

                            // Collapse all operations into one undo step
                            Undo.CollapseUndoOperations(undoGroup);
                        }

                        GUI.enabled = true;
                    }

                    if (hasInventory && hasWallet && hasCanvas)
                    {
                        EditorGUILayout.HelpBox("Inventory system is already set up on this player.",
                            MessageType.Info);
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void SetupInventorySystem()
        {
            if (playerObject == null) return;

            // Register complete object for undo
            Undo.RegisterFullObjectHierarchyUndo(playerObject, "Setup Inventory System");

            // Add components with undo support
            if (!playerObject.GetComponent<Inventory>())
            {
                Undo.AddComponent<Inventory>(playerObject);
            }

            if (!playerObject.GetComponent<Wallet>())
            {
                Undo.AddComponent<Wallet>(playerObject);
            }

            if (!playerObject.GetComponent<InventoryInputManager>())
            {
                Undo.AddComponent<InventoryInputManager>(playerObject);
            }

            // Check for existing canvas
            var existingCanvas = playerObject.GetComponentInChildren<Canvas>()?.gameObject;
            if (existingCanvas?.name == "Inventory Canvas")
            {
                Debug.LogWarning("Inventory Canvas already exists under the player.");
                return;
            }

            // Load and instantiate canvas with undo support
            var canvasPrefab = Resources.Load<GameObject>("Inventory Canvas");
            if (canvasPrefab != null)
            {
                var canvas = PrefabUtility.InstantiatePrefab(canvasPrefab) as GameObject;
                if (canvas != null)
                {
                    // Register canvas creation for undo
                    Undo.RegisterCreatedObjectUndo(canvas, "Create Inventory Canvas");

                    // Set parent with undo support
                    var parent = playerObject.transform.parent.name == "FS Player" ? playerObject.transform.parent : playerObject.transform;

                    Undo.SetTransformParent(canvas.transform, parent, "Parent Inventory Canvas");

                    // Set transform values
                    canvas.transform.localPosition = Vector3.zero;
                    canvas.transform.localRotation = Quaternion.identity;
                    canvas.transform.localScale = Vector3.one;
                }
                else
                {
                    Debug.LogError("Failed to instantiate Inventory Canvas prefab.");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Setup Failed",
                    "Could not find 'Inventory Canvas' prefab in Resources folder.", "OK");
            }

            // Mark scene as dirty
            EditorSceneManager.MarkSceneDirty(playerObject.scene);
        }


        void ShowCurrencyAndAmount(CurrencyAmount price, Item item, string name = "Price")
        {
            using (new EditorGUILayout.HorizontalScope())
            {

                price.amount = EditorGUILayout.FloatField(name, price.amount);
                if (currencies != null)
                {
                    var currencyList = currencies.currencyList;

                    int newSelectedIndex = EditorGUILayout.Popup(price.currencyIndex, currencyList.Select(c => c.name).ToArray());
                    if (newSelectedIndex != price.currencyIndex)
                    {
                        Undo.RecordObject(item, "Changed Currency");
                        price.currencyIndex = newSelectedIndex;
                        EditorUtility.SetDirty(item);
                    }
                }
            }
        }

        private ReorderableList reorderableEquipmentSlots;
        private void InitializeReorderableList()
        {
            if (equipmentSlots != null)
            {
                equipmentSlotsSO = new SerializedObject(equipmentSlots);
                SerializedProperty slotListProp = equipmentSlotsSO.FindProperty("equipmentSlotList");

                reorderableEquipmentSlots = new ReorderableList(
                    equipmentSlotsSO,
                    slotListProp,
                    true, // draggable
                    true, // display header
                    true, // add button
                    true  // remove button
                );

                // Header drawing
                reorderableEquipmentSlots.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Equipment Slots");
                };

                // Element height (slightly taller than default)
                reorderableEquipmentSlots.elementHeight = EditorGUIUtility.singleLineHeight + 6;

                // Element drawing
                reorderableEquipmentSlots.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    SerializedProperty element = reorderableEquipmentSlots.serializedProperty.GetArrayElementAtIndex(index);

                    // Adjust rect for element display
                    rect.y += 2;
                    rect.height = EditorGUIUtility.singleLineHeight;

                    // Draw the slot name field
                    EditorGUI.PropertyField(rect, element, GUIContent.none);
                };

                // Add element callback
                reorderableEquipmentSlots.onAddCallback = (ReorderableList list) =>
                {
                    int index = list.serializedProperty.arraySize;
                    list.serializedProperty.arraySize++;
                    list.index = index;
                    SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                    element.stringValue = "New Slot";
                    equipmentSlotsSO.ApplyModifiedProperties();
                };

                // Remove element callback with confirmation dialog
                reorderableEquipmentSlots.onRemoveCallback = (ReorderableList list) =>
                {
                    if (EditorUtility.DisplayDialog("Delete Slot",
                            "Are you sure you want to delete this slot?", "Delete", "Cancel"))
                    {
                        ReorderableList.defaultBehaviours.DoRemoveButton(list);
                        equipmentSlotsSO.ApplyModifiedProperties();
                    }
                };
            }
        }

        void CreateCurrenciesObject()
        {
            string path = EditorUtility.SaveFilePanelInProject($"Create currency list", "currency list", "asset", $"Specify a location to save the currencies object");
            if (!string.IsNullOrEmpty(path))
            {
                var newObject = ScriptableObject.CreateInstance<CurrencyDatabase>();
                newObject.name = "Currency List";

                AssetDatabase.CreateAsset(newObject, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                currencies = newObject;
                currencySO = new SerializedObject(currencies);
            }
        }

        void CreateEquipmentSlotsObject()
        {
            string path = EditorUtility.SaveFilePanelInProject($"Create equipment slots list", "equipment slots list", "asset", $"Specify a location to save the equipment slots object");
            if (!string.IsNullOrEmpty(path))
            {
                var newObject = ScriptableObject.CreateInstance<EquipmentSlotsDatabase>();
                newObject.name = "Currency List";

                AssetDatabase.CreateAsset(newObject, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                equipmentSlots = newObject;
                equipmentSlotsSO = new SerializedObject(equipmentSlots);
            }
        }
    }
}

#endif
