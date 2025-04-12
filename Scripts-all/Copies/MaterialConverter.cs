using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class MaterialConverterWindow : EditorWindow
{
    private bool includeSubfolders = true;
    private string selectedFolder = "Assets";
    private Dictionary<string, string> shaderMappings = new Dictionary<string, string>();
    private List<string> conversionLog = new List<string>();

    [MenuItem("Tools/Material Converter/Open Material Converter")]
    public static void ShowWindow()
    {
        GetWindow<MaterialConverterWindow>("Material Converter");
    }

    private void OnEnable()
    {
        // Initialize default shader mappings
        shaderMappings.Clear();
        shaderMappings.Add("Standard", "Universal Render Pipeline/Lit");
        shaderMappings.Add("Unlit/Texture", "Universal Render Pipeline/Unlit");
        shaderMappings.Add("Unlit/Color", "Universal Render Pipeline/Unlit");
    }

    private void OnGUI()
    {
        GUILayout.Label("Material Converter", EditorStyles.boldLabel);

        // Folder selection
        GUILayout.Label("Select Folder to Convert:");
        EditorGUILayout.BeginHorizontal();
        selectedFolder = EditorGUILayout.TextField(selectedFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            selectedFolder = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
            if (selectedFolder.StartsWith(UnityEngine.Application.dataPath))
            {
                selectedFolder = "Assets" + selectedFolder.Substring(UnityEngine.Application.dataPath.Length);
            }
            else
            {
                selectedFolder = "Assets";
            }
        }
        EditorGUILayout.EndHorizontal();

        includeSubfolders = EditorGUILayout.Toggle("Include Subfolders", includeSubfolders);

        // Shader mappings
        GUILayout.Label("Shader Mappings:");
        EditorGUI.indentLevel++;
        foreach (var mapping in new Dictionary<string, string>(shaderMappings))
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(mapping.Key, GUILayout.Width(150));
            shaderMappings[mapping.Key] = EditorGUILayout.TextField(shaderMappings[mapping.Key]);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel--;

        // Convert button
        if (GUILayout.Button("Convert Materials"))
        {
            ConvertMaterials();
        }

        // Logs
        GUILayout.Label("Conversion Log:");
        EditorGUILayout.BeginVertical("box");
        foreach (var logEntry in conversionLog)
        {
            EditorGUILayout.LabelField(logEntry);
        }
        EditorGUILayout.EndVertical();
    }

    private void ConvertMaterials()
    {
        conversionLog.Clear();
        string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { selectedFolder });
        int materialsConverted = 0;
        int totalMaterials = materialGuids.Length;

        for (int i = 0; i < totalMaterials; i++)
        {
            string guid = materialGuids[i];
            string materialPath = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

            if (material != null)
            {
                string originalShaderName = material.shader.name;
                if (shaderMappings.ContainsKey(originalShaderName))
                {
                    string newShaderName = shaderMappings[originalShaderName];
                    Shader newShader = Shader.Find(newShaderName);

                    if (newShader != null)
                    {
                        // Record undo operation
                        Undo.RecordObject(material, "Convert Material to URP");

                        // Convert the material
                        material.shader = newShader;
                        EditorUtility.SetDirty(material);
                        materialsConverted++;

                        // Log success
                        conversionLog.Add($"Converted: {materialPath} (Shader: {originalShaderName} -> {newShaderName})");
                    }
                    else
                    {
                        // Log error: Shader not found
                        conversionLog.Add($"Error: Shader not found for {materialPath} (Shader: {newShaderName})");
                    }
                }
                else
                {
                    // Log warning: Shader not mapped
                    conversionLog.Add($"Warning: Shader not mapped for {materialPath} (Shader: {originalShaderName})");
                }
            }

            // Display progress bar
            float progress = (float)i / totalMaterials;
            bool cancel = EditorUtility.DisplayCancelableProgressBar("Converting Materials", $"Converting {materialPath}...", progress);

            if (cancel)
            {
                UnityEngine.Debug.Log("Material conversion canceled by user.");
                break;
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        UnityEngine.Debug.Log($"Converted {materialsConverted} materials to URP.");
    }
}