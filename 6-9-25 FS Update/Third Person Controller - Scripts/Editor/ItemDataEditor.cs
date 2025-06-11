using FS_ThirdPerson;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace FS_Core
{
    public class ItemDataEditor : Editor
    {
        SerializedProperty nameProp, descriptionProp, iconProp, categoryProp;

        Item itemData;
        private bool attributesFoldout;

        public virtual void OnEnable()
        {
            itemData = (Item)target;

            nameProp = serializedObject.FindProperty("name");
            descriptionProp = serializedObject.FindProperty("description");
            iconProp = serializedObject.FindProperty("icon");
            categoryProp = serializedObject.FindProperty("category");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(nameProp);
            EditorGUILayout.PropertyField(descriptionProp);
            EditorGUILayout.PropertyField(iconProp);
            EditorGUILayout.PropertyField(categoryProp);
            if (itemData.category != null && itemData.category.attributes.Count > 0)
            {
                if (itemData.attributeValues == null || itemData.attributeValues.Count != itemData.category.attributes.Count)
                {
                    itemData.attributeValues = new List<ItemAttribute>();
                    for (int i = 0; i < itemData.category.attributes.Count; i++)
                    {
                        itemData.attributeValues.Add(new ItemAttribute(itemData.category.attributes[i].attributeType, itemData.category.attributes[i].attributeName));
                    }
                }

                attributesFoldout = EditorGUILayout.Foldout(attributesFoldout, "Attribute Settings", true);
                if (attributesFoldout)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    foreach (var attribute in itemData.attributeValues)
                    {
                        switch (attribute.attributeType)
                        {
                            case ItemAttributeType.Integer:
                                attribute.intValue = EditorGUILayout.IntField(attribute.attributeName, attribute.intValue);
                                break;
                            case ItemAttributeType.Decimal:
                                attribute.floatValue = EditorGUILayout.FloatField(attribute.attributeName, attribute.floatValue);
                                break;
                            case ItemAttributeType.Text:
                                attribute.stringValue = EditorGUILayout.TextField(attribute.attributeName, attribute.stringValue);
                                break;
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();
        }

    }
}