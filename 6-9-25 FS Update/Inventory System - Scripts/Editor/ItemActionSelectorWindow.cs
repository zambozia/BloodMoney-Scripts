#if UNITY_EDITOR

using FS_Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FS_InventorySystem
{
    public class ItemActionSelectorWindow : EditorWindow
    {
        private static ItemCategory itemCategory;
        private static Item item;
        private static bool openedForCategory;
        private static List<Type> itemActionTypes;

        public static void Show(ItemCategory itemCategory)
        {
            ItemActionSelectorWindow.itemCategory = itemCategory;
            openedForCategory = true;

            itemActionTypes = GetAllItemActionTypes();

            ItemActionSelectorWindow window = GetWindow<ItemActionSelectorWindow>(true, "Select Item Action");
            window.Show();
        }

        public static void Show(Item item)
        {
            ItemActionSelectorWindow.item = item;
            openedForCategory = false;

            itemActionTypes = GetAllItemActionTypes();

            ItemActionSelectorWindow window = GetWindow<ItemActionSelectorWindow>(true, "Select Item Action");
            window.Show();
        }

        private void OnGUI()
        {
            if (itemCategory == null && item?.category == null)
            {
                Close();
                return;
            }

            GUILayout.Label("Select an Item Action:", EditorStyles.boldLabel);

            if (GUILayout.Button("None"))
            {
                if (openedForCategory)
                    AssignActionToCategory(null);
                else
                    AssignActionToItem(null);
                Close();
            }

            foreach (var type in itemActionTypes)
            {
                if (GUILayout.Button(type.Name))
                {
                    if (openedForCategory)
                        AssignActionToCategory(type);
                    else
                        AssignActionToItem(type);

                    Close();
                }
            }
        }

        private static List<Type> GetAllItemActionTypes()
        {
            return Assembly.GetAssembly(typeof(ItemAction))
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(ItemAction)))
                .ToList();
        }

        private void AssignActionToCategory(Type actionType)
        {
            ItemAction newAction = null;
            if (actionType != null)
                newAction = ScriptableObject.CreateInstance(actionType) as ItemAction;
            
            if (newAction != null)
            {
                if (itemCategory.itemAction == newAction) return;

                string path = AssetDatabase.GetAssetPath(itemCategory);
                if (!string.IsNullOrEmpty(path))
                {
                    string directory = System.IO.Path.GetDirectoryName(path);
                    string assetPath = $"{directory}/{actionType.Name}_{itemCategory.Name}.asset";

                    AssetDatabase.CreateAsset(newAction, assetPath);
                    AssetDatabase.SaveAssets();
                }
            }

            var oldAction = itemCategory.itemAction;

            itemCategory.itemAction = newAction;
            EditorUtility.SetDirty(itemCategory);

            if (oldAction != null && oldAction != newAction)
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(oldAction));

            AssetDatabase.Refresh();
        }

        private void AssignActionToItem(Type actionType)
        {
            ItemAction newAction = null;
            if (actionType != null)
                newAction = ScriptableObject.CreateInstance(actionType) as ItemAction;

            if (newAction != null)
            {
                if (item.itemAction == newAction) return;

                string path = AssetDatabase.GetAssetPath(item);
                if (!string.IsNullOrEmpty(path))
                {
                    string directory = System.IO.Path.GetDirectoryName(path);
                    string assetPath = $"{directory}/{actionType.Name}_{item.Name}.asset";

                    AssetDatabase.CreateAsset(newAction, assetPath);
                    AssetDatabase.SaveAssets();
                }
            }

            var oldAction = item.itemAction;

            item.itemAction = newAction;
            EditorUtility.SetDirty(item);

            if (oldAction != null && oldAction != newAction)
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(oldAction));

            AssetDatabase.Refresh();
        }
    }
}

#endif
