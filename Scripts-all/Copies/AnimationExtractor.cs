using UnityEditor;
using UnityEngine;

public class AnimationExtractor : Editor
{
    [MenuItem("Tools/Extract Animations From Selected Models")]
    static void ExtractAnimations()
    {
        // Create a folder to store extracted animations if it doesn't exist
        string folderPath = "Assets/ExtractedAnimations";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "ExtractedAnimations");
        }

        // Loop through all selected objects in the Project window
        foreach (Object obj in Selection.objects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            ModelImporter modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;

            if (modelImporter != null)
            {
                // Keep the original Animation Type to avoid changing it
                ModelImporterAnimationType originalAnimationType = modelImporter.animationType;

                // Reimport the model to refresh any necessary settings
                modelImporter.SaveAndReimport();

                // Extract all animation clips from the model file
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                foreach (Object asset in assets)
                {
                    if (asset is AnimationClip clip && !clip.name.Contains("__preview__"))
                    {
                        // Use the object name and clip name to create a unique file name
                        string clipPath = $"{folderPath}/{obj.name}_{clip.name}.anim";
                        AssetDatabase.CreateAsset(Object.Instantiate(clip), clipPath);
                    }
                }

                // Restore the original Animation Type
                modelImporter.animationType = originalAnimationType;
                modelImporter.SaveAndReimport();
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Animations extracted successfully to Assets/ExtractedAnimations.");
    }
}
