#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using AnimatorController = UnityEditor.Animations.AnimatorController;

namespace FS_ThirdPerson
{
    public partial class FSSystemsSetup : MonoBehaviour
    {
        public static string welcomeWindowOpenKey = "FS_WelcomeWindow_Opened";
        static FSSystemInfo ThirdPersonControllerSystemSetup = new FSSystemInfo
        (
            characterType: CharacterType.Player,
            enabled: true,
            name: "Locomotion System",

            systemProjectSettings: new SystemProjectSettingsData
            (
                layers: new List<string> { "Player", "FootTrigger", "Hotspot" }
            ),

            prefabName: "Locomotion Controller",
            welcomeEditorShowKey: "LocomotionSystem_WelcomeWindow_Opened_1"
        );

        static string LocomotionSystemWelcomeEditorKey => ThirdPersonControllerSystemSetup.welcomeEditorShowKey;

        [InitializeOnLoadMethod]
        public static void LoadLocomotionSystem()
        {
            if (!string.IsNullOrEmpty(LocomotionSystemWelcomeEditorKey) && !EditorPrefs.GetBool(LocomotionSystemWelcomeEditorKey, false))
            {
                SessionState.SetBool(welcomeWindowOpenKey, false);
                EditorPrefs.SetBool(LocomotionSystemWelcomeEditorKey, true);
                FSSystemsSetupEditorWindow.OnProjectLoad();
            }
        }

        private void Awake()
        {
            this.enabled = false;
        }


        public Dictionary<string, FSSystemInfo> FSSystems = new Dictionary<string, FSSystemInfo>();


        public void FindSystem()
        {
            FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);

            foreach (var field in fields)
            {
                // Check if the field's type is FSSystem
                if (field.FieldType == typeof(FSSystemInfo))
                {
                    // Get the value of the field and cast it to FSSystem
                    FSSystemInfo system = field.GetValue(this) as FSSystemInfo;

                    if (system != null)
                    {
                        FSSystems.TryAdd(field.Name, system);
                    }
                }
            }
        }

        /// <summary>
        /// Loads the prefab from Resources and copies all its components to this GameObject.
        /// </summary>
        public GameObject CopyComponentsAndAnimControllerFromPrefab(string prefabName, AnimatorMergerUtility animatorMergerUtility, GameObject playerObj)
        {
            if (!string.IsNullOrEmpty(prefabName))
            {
                // Load the prefab from Resources
                GameObject prefab = Resources.Load<GameObject>(prefabName);

                if (prefab != null)
                {
                    var prefabPlayer = prefab.GetComponentInChildren<PlayerController>().gameObject;

                    var animatorController = prefabPlayer.GetComponent<Animator>().runtimeAnimatorController as AnimatorController;

                    animatorMergerUtility.MergeAnimatorControllers(animatorController);
                    // Get all components from the prefab
                    Component[] components = prefabPlayer.GetComponents<Component>();

                    foreach (Component sourceComp in components)
                    {
                        Type componentType = sourceComp.GetType(); 
                        if (playerObj.GetComponent(componentType) != null) continue; 

                        System.Type type = sourceComp.GetType();
                        Component targetComp = playerObj.GetComponent(type);

                        if (targetComp == null)
                        {
                            targetComp = playerObj.AddComponent(type);
                            ComponentUtility.CopyComponent(sourceComp);
                            ComponentUtility.PasteComponentValues(targetComp);
                            //EditorUtility.CopySerializedIfDifferent(sourceComp, targetComp);
                        }

                    }
                    var managedScript = playerObj.GetComponents<SystemBase>().ToList();
                    managedScript.Sort((x, y) => x.Priority.CompareTo(y.Priority));
                    playerObj.GetComponent<PlayerController>().managedScripts = managedScript;
                    //playerObj.layer = prefabPlayer.layer;
                    //playerObj.name = "FS Player";
                }
                //else
                //{
                //    Debug.LogError($"Prefab '{prefabName}' not found in Resources.");
                //}

                
                return prefab;
            }
            else
            {
                Debug.LogWarning("Prefab name is not specified.");
            }
            return null;
        }


        public void ImportProjectSettings()
        {
            FindSystem();
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            foreach (var systemProjectSettingsData in FSSystems.Values)
            {
                if (systemProjectSettingsData.systemProjectSettings == null) 
                    continue;
                for (int i = 0; i < systemProjectSettingsData.systemProjectSettings.layers.Count; i++)
                {
                    string layerName = systemProjectSettingsData.systemProjectSettings.layers[i];

                    if (!string.IsNullOrEmpty(layerName))
                    {
                        bool layerExists = false;
                        for (int j = 0; j < layersProp.arraySize; j++)
                        {
                            SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(j);
                            if (layerProp.stringValue == layerName)
                            {
                                layerExists = true;
                                break;
                            }
                        }

                        if (!layerExists)
                        {
                            for (int j = 8; j < layersProp.arraySize; j++)
                            {
                                SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(j);
                                if (string.IsNullOrEmpty(layerProp.stringValue))
                                {
                                    layerProp.stringValue = layerName;
                                    break;
                                }
                            }
                        }
                    }
                }
                tagManager.ApplyModifiedProperties();

                if (systemProjectSettingsData.systemProjectSettings.tags != null)
                {
                    foreach (var tag in systemProjectSettingsData.systemProjectSettings.tags)
                    {
                        if (!InternalEditorUtility.tags.ToList().Contains(tag))
                            InternalEditorUtility.AddTag(tag);
                    }
                }

                systemProjectSettingsData.systemProjectSettings.extraSetupAction?.Invoke();
            }
        }

    }
    public enum CharacterType
    {
        Player,
        AI,
        None
    }
    public class FSSystemInfo
    {
        //public bool isSystemBase;
        public CharacterType characterType;
        public bool enabled;
        public string name;
        public string prefabName;
        public string mobileControllerPrefabName;
        public Action<GameObject, GameObject, GameObject> setupExtraActions;
        public SystemProjectSettingsData systemProjectSettings;
        public string welcomeEditorShowKey;
        public FSSystemInfo(CharacterType characterType, string name, string prefabName = "", bool enabled = false, string welcomeEditorShowKey = "", SystemProjectSettingsData systemProjectSettings = null, string mobileControllerPrefabName = "", Action<GameObject, GameObject, GameObject> setupExtraActions = null)
        {
            //this.isSystemBase = isSystemBase;
            this.characterType = characterType;
            this.enabled = enabled;
            this.name = name;
            this.prefabName = prefabName;
            this.systemProjectSettings = systemProjectSettings;
            this.mobileControllerPrefabName = mobileControllerPrefabName;

            if (setupExtraActions != null)
                this.setupExtraActions = setupExtraActions;

            if(!string.IsNullOrEmpty(welcomeEditorShowKey))
                this.welcomeEditorShowKey = welcomeEditorShowKey;
            else
                this.welcomeEditorShowKey = FSSystemsSetup.welcomeWindowOpenKey;
        }

    }

    public class SystemProjectSettingsData 
    {
        public List<string> layers = new List<string>();
        public List<string> tags = new List<string>();

        public Action extraSetupAction;

        public SystemProjectSettingsData(List<string> layers = null, List<string> tags = null, Action extraSetupAction = null)
        {
            if (layers == null)
                this.layers = new List<string>();
            else
                this.layers = layers;
            this.tags = tags;
            this.extraSetupAction = extraSetupAction;
        }
    }
}
#endif