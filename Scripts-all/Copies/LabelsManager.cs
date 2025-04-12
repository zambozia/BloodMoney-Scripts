using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace AssetLibraryManager
{
    public class LabelsManager : EditorWindow
    {
        SettingsObject settings;

        private int buttonWidth = 160;

        Vector2 scrollView;

        private string textField = "Seperate labels with a space...";

        public enum Options { Add, Remove, Toggle };
        public static Options op = Options.Toggle;

        public delegate void ChangeLabels();
        public static event ChangeLabels OnLabelsChanged;

        public static bool foldGroup = false;

        public static List<string> projectLabels;


        [MenuItem("Tools/Asset Library Manager/Labels Manager...", false, 80)]
        public static void ShowWindow()
        {
            GetWindow<LabelsManager>("Labels Manager");
        }

        void CreateGUI()
        {
            settings = Resources.Load("AssetLibrary_Settings") as SettingsObject;

            PrefabLabels.SetUp_Labels();

            if (projectLabels == null)
            {
                projectLabels = new List<string>(PrefabLabels.customLabels);
            }
        }

        void OnGUI()
        {
            EditorGUILayout.Space(10);

            op = (Options)EditorGUILayout.EnumPopup("Options:", op);

            EditorGUILayout.Space(10);

            GUIStyle someGuiStyle = new GUIStyle(EditorStyles.largeLabel);
            someGuiStyle.alignment = TextAnchor.MiddleCenter;
            someGuiStyle.fontSize = 16;

            foldGroup = EditorGUILayout.BeginFoldoutHeaderGroup(foldGroup, "Enter labels manually");

            if (foldGroup == true)
            {
                EditorGUILayout.LabelField("Enter asset labels", someGuiStyle, GUILayout.Height(25));

                EditorGUILayout.Space(10);

                textField = EditorGUILayout.TextField("", textField);

                EditorGUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();

                //Set Labels-------------------------------------------------------------------------------
                if (GUILayout.Button("Set Labels"))
                {
                    List<string> newLabels = textField.Split(' ').ToList();

                    newLabels = newLabels.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

                    foreach (var label in newLabels)
                    {
                        foreach (var obj in Selection.objects)
                        {
                            List<string> existingLabels = new List<string>();

                            existingLabels.AddRange(AssetDatabase.GetLabels(obj));

                            if (op == Options.Add)
                            {
                                if (!existingLabels.Contains(label))
                                {
                                    existingLabels.Add(label);

                                    AssetDatabase.SetLabels(obj, existingLabels.ToArray());
                                }
                            }

                            if (op == Options.Remove)
                            {
                                if (existingLabels.Contains(label))
                                {
                                    existingLabels.Remove(label);

                                    AssetDatabase.SetLabels(obj, existingLabels.ToArray());
                                }
                            }

                            if (op == Options.Toggle)
                            {
                                if (!existingLabels.Contains(label))
                                {
                                    existingLabels.Add(label);

                                    AssetDatabase.SetLabels(obj, existingLabels.ToArray());
                                }

                                else
                                {
                                    existingLabels.Remove(label);

                                    AssetDatabase.SetLabels(obj, existingLabels.ToArray());
                                }
                            }
                        }
                    }


                    if (HasOpenInstances<Preview_Window>())
                    {
                        OnLabelsChanged();
                    }

                    PrefabLabels.SetUp_Labels();

                    Debug.Log($"Labels added.");
                }

                //Clear Labels-----------------------------------------------------------------------------


                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();


            EditorGUILayout.Space(10);


            EditorGUILayout.LabelField("Add existing labels", someGuiStyle, GUILayout.Height(25));

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Clear Prefab Labels", GUILayout.Width(200)))
            {
                foreach (var obj in Selection.objects)
                {
                    AssetDatabase.ClearLabels(obj);
                }

                if (HasOpenInstances<Preview_Window>())
                {
                    OnLabelsChanged();
                }

                Debug.Log($"Labels cleared");

                PrefabLabels.SetUp_Labels();
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);


            EditorGUILayout.BeginHorizontal();

            foreach (var item in settings.createNewSection)
            {
                EditorGUILayout.BeginVertical();

                if (item.hideFromLabelManager == false)
                {
                    foreach (var label in item.sectionLabels)
                    {
                        if (label != "Clear")
                        {
                            if (GUILayout.Button(label, GUILayout.Width(buttonWidth)))
                            {
                                foreach (var obj in Selection.objects)
                                {
                                    List<string> existingLabels = new List<string>();

                                    existingLabels.AddRange(AssetDatabase.GetLabels(obj));

                                    if (op == Options.Add)
                                    {
                                        if (!existingLabels.Contains(label))
                                        {
                                            existingLabels.Add(label);

                                            AssetDatabase.SetLabels(obj, existingLabels.ToArray());
                                        }
                                    }

                                    if (op == Options.Remove)
                                    {
                                        if (existingLabels.Contains(label))
                                        {
                                            existingLabels.Remove(label);

                                            AssetDatabase.SetLabels(obj, existingLabels.ToArray());
                                        }
                                    }

                                    if (op == Options.Toggle)
                                    {
                                        if (!existingLabels.Contains(label))
                                        {
                                            existingLabels.Add(label);

                                            AssetDatabase.SetLabels(obj, existingLabels.ToArray());
                                        }

                                        else
                                        {
                                            existingLabels.Remove(label);

                                            AssetDatabase.SetLabels(obj, existingLabels.ToArray());
                                        }
                                    }
                                }

                                if (HasOpenInstances<Preview_Window>())
                                {
                                    OnLabelsChanged();
                                }

                                PrefabLabels.SetUp_Labels();
                            }
                        }
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginVertical();


            scrollView = GUILayout.BeginScrollView(scrollView);
            {
                foreach (var label in projectLabels)
                {
                    if (label != "Clear")
                    {
                        if (GUILayout.Button(label, GUILayout.Width(buttonWidth)))
                        {
                            foreach (var obj in Selection.objects)
                            {
                                List<string> existingLabels = new List<string>();

                                existingLabels.AddRange(AssetDatabase.GetLabels(obj));

                                if (op == Options.Add)
                                {
                                    if (!existingLabels.Contains(label))
                                    {
                                        existingLabels.Add(label);

                                        AssetDatabase.SetLabels(obj, existingLabels.ToArray());
                                    }
                                }

                                if (op == Options.Remove)
                                {
                                    if (existingLabels.Contains(label))
                                    {
                                        existingLabels.Remove(label);

                                        AssetDatabase.SetLabels(obj, existingLabels.ToArray());
                                    }
                                }

                                if (op == Options.Toggle)
                                {
                                    if (!existingLabels.Contains(label))
                                    {
                                        existingLabels.Add(label);

                                        AssetDatabase.SetLabels(obj, existingLabels.ToArray());
                                    }

                                    else
                                    {
                                        existingLabels.Remove(label);

                                        AssetDatabase.SetLabels(obj, existingLabels.ToArray());
                                    }
                                }
                            }

                            if (HasOpenInstances<Preview_Window>())
                            {
                                OnLabelsChanged();
                            }

                            PrefabLabels.SetUp_Labels();
                        }
                    }
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            GUILayout.Space(10);

            GUILayout.FlexibleSpace();

            ////Clear All Project Labels--------------------------------------------------------------------
            if (GUILayout.Button("Clear all project Labels"))
            {
                if (EditorUtility.DisplayDialog("Delete Confirmation", "This will delete all asset labels on every prefab in your project.\n\nAre you sure?", "Ok", "Cancel"))
                {
                    string[] getGUIDs = AssetDatabase.FindAssets("t:prefab");

                    for (int i = 0; i < getGUIDs.Length; i++)
                    {
                        string getPath = AssetDatabase.GUIDToAssetPath(getGUIDs[i]);

                        Object getName = AssetDatabase.LoadAssetAtPath<Object>(getPath);

                        AssetDatabase.ClearLabels(getName);
                    }

                    if (HasOpenInstances<Preview_Window>())
                    {
                        OnLabelsChanged();
                    }

                    PrefabLabels.SetUp_Labels();

                    Debug.Log($"Project labels cleared.");
                }
            }

            EditorGUILayout.Space(10);
        }
    }
}
