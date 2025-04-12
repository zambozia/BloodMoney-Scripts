using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AssetLibraryManager
{
    //[CreateAssetMenu(fileName = "AssetLibrary_Settings", menuName = "Asset Library Settings", order = 1)]
    public class SettingsObject : ScriptableObject
    {
        [Space(10)]
        public Options options;

        [Space(30)]
        [Tooltip("Include all prefabs in these folders, if left empty it will search the whole project for prefabs.")]
        public List<IncludedFolder> includedFolders = new List<IncludedFolder>();

        [Space(10)]
        [Tooltip("Exclude all prefabs in these folders, particles and canvas have bad or no thumbails, you can remove them here.")]
        public List<ExcludedFolder> excludedFolders = new List<ExcludedFolder>();

        [Space(30)]
        [Tooltip("Include individual Prefabs not already in Included Folders")]
        public List<Object> includedPrefabs = new List<Object>();
        
        [Space(10)]
        [Tooltip("Exclude individual Prefabs not already in Excluded Folders")]
        public List<Object> excludedPrefabs = new List<Object>();

        [Space(30)]
        public List<CreateNewSection> createNewSection = new List<CreateNewSection>();
    }

    [CustomPropertyDrawer(typeof(IncludedFolder))]
    public class IncludedFolderDrawer : PropertyDrawer
    {
        public static SettingsObject settings;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = -9;


            GUIContent someContent = new GUIContent();

            if (settings == null)
            {
                settings = Resources.Load("AssetLibrary_Settings") as SettingsObject;
            }

            // Calculate rects
            var folderRect = new Rect(position.x + 125, position.y, position.width - 125, position.height);

            if (property.FindPropertyRelative("includedFolder").objectReferenceValue != null)
            {
                //Get whatever is in the object field
                string path = AssetDatabase.GetAssetPath(property.FindPropertyRelative("includedFolder").objectReferenceValue);

                if (settings.options.showFolderPath == true)
                {
                    someContent.text = path.Replace("Assets/", "").Replace(property.FindPropertyRelative("includedFolder").objectReferenceValue.name, "");
                }
                else
                {
                    someContent.tooltip = path.Replace("Assets/", "").Replace(property.FindPropertyRelative("includedFolder").objectReferenceValue.name, "");
                }
            }

            EditorGUI.PropertyField(folderRect, property.FindPropertyRelative("includedFolder"), someContent);


            // Set indent back to what it was
            EditorGUI.indentLevel = indent;
        }
    }

    [CustomPropertyDrawer(typeof(ExcludedFolder))]
    public class ExcludedFolderDrawer : PropertyDrawer
    {
        public static SettingsObject settings;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = -9;


            GUIContent someContent = new GUIContent();

            if (settings == null)
            {
                settings = Resources.Load("AssetLibrary_Settings") as SettingsObject;
            }

            // Calculate rects
            var folderRect = new Rect(position.x + 125, position.y, position.width - 125, position.height);

            if (property.FindPropertyRelative("excludedFolder").objectReferenceValue != null)
            {
                //Get whatever is in the object field
                string path = AssetDatabase.GetAssetPath(property.FindPropertyRelative("excludedFolder").objectReferenceValue);

                if (PrefabLabels.settings.options.showFolderPath == true)
                {
                    someContent.text = path.Replace("Assets/", "").Replace(property.FindPropertyRelative("excludedFolder").objectReferenceValue.name, "");
                }
                else
                {
                    someContent.tooltip = path.Replace("Assets/", "").Replace(property.FindPropertyRelative("excludedFolder").objectReferenceValue.name, "");
                }              
            }

            EditorGUI.PropertyField(folderRect, property.FindPropertyRelative("excludedFolder"), someContent);


            // Set indent back to what it was
            EditorGUI.indentLevel = indent;
        }
    }

    public class SelectSettingsObject : EditorWindow
    {
        [MenuItem("Tools/Asset Library Manager/Settings...", false, 100)]
        public static void SelectSettings()
        {
            Selection.activeObject = Resources.Load("AssetLibrary_Settings");
        }
    }
}