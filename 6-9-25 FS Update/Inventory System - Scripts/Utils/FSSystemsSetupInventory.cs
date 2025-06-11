#if UNITY_EDITOR
using FS_InventorySystem;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FS_ThirdPerson
{
    public partial class FSSystemsSetup
    {
        static FSSystemInfo InventorySystemSetup = new FSSystemInfo
        (
            characterType: CharacterType.Player,
            enabled: false,
            name: "Inventory System",
            prefabName: "",
            welcomeEditorShowKey: "InventorySystem_WelcomeWindow_Opened",

           setupExtraActions: (GameObject playerModel, GameObject systemPrefabObj, GameObject systemControllerParentObj) =>
           {
               SetInventoryUI(systemControllerParentObj.transform);
           }
        );

        static string InventorySystemWelcomeEditorKey => InventorySystemSetup.welcomeEditorShowKey;


        [InitializeOnLoadMethod]
        public static void LoadInventorySystem()
        {
            if (!string.IsNullOrEmpty(InventorySystemWelcomeEditorKey) && !EditorPrefs.GetBool(InventorySystemWelcomeEditorKey, false))
            {
                SessionState.SetBool(welcomeWindowOpenKey, false);
                EditorPrefs.SetBool(InventorySystemWelcomeEditorKey, true);
                FSSystemsSetupEditorWindow.OnProjectLoad();
            }
        }

        static void SetInventoryUI(Transform FSParent)
        {
            PlayerController playerController = FSParent.GetComponentInChildren<PlayerController>();
            if (playerController == null) return;

            // Add components with undo support
            if (!playerController.GetComponent<Inventory>())
            {
                var inventory = Undo.AddComponent<Inventory>(playerController.gameObject);
            }

            if (!playerController.GetComponent<Wallet>())
            {
                var wallet = Undo.AddComponent<Wallet>(playerController.gameObject);
            }

            if (!playerController.GetComponent<InventoryInputManager>())
            {
                var wallet = Undo.AddComponent<InventoryInputManager>(playerController.gameObject);
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
                    Undo.SetTransformParent(canvas.transform, FSParent, "Parent Inventory Canvas");

                    // Set transform values
                    canvas.transform.localPosition = Vector3.zero;
                    canvas.transform.localRotation = Quaternion.identity;
                    canvas.transform.localScale = Vector3.one;

                    canvas.GetComponent<InventorySettings>().playerInventory = playerController.GetComponent<Inventory>();
                    canvas.GetComponent<InventorySettings>().playerWallet = playerController.GetComponent<Wallet>();
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
            EditorSceneManager.MarkSceneDirty(playerController.gameObject.scene);
        }
    }
}
#endif