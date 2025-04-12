using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetLibraryManager
{
    [Serializable]
    public class CreateNewSection
    {
        [HideInInspector]
        public int labelIndex, oldLabelIndex;

        [HideInInspector]
        public bool fold = false;


        [Space(10)]
        public string sectionName = string.Empty;


        [Space(10)]
        [Tooltip("Hides this section in the Prefab Labels Window.")]
        public bool hideSection = false;


        [Tooltip("Hides these labels from the Labels section in the Prefab Labels Window - Clear filters for the changes to take effect.")]
        public bool hideFromLabels = true;

        [Tooltip("Hides this section from the Label Manager Window.")]
        public bool hideFromLabelManager = false;


        [Space(10)]
        public List<string> sectionLabels = new List<string>();
    }

    [Serializable]
    public class IncludedFolder
    {
        public UnityEngine.Object includedFolder;
    }

    [Serializable]
    public class ExcludedFolder
    {
        public UnityEngine.Object excludedFolder;
    }

    [Serializable]
    public class Options
    {
        [Space(10)]
        [Range(32, 128)]
        public int thumbnailSize = 128;

        public enum OnSelect { doNothing, pingPrefab, selectPrefab, showPreview };
        [Space(10)]
        [Tooltip("Choose what to do when you click on a prefab in the Viewer.")]
        public OnSelect onClick = OnSelect.selectPrefab;

        [Space(10)]
        [Tooltip("When nothing is selected in Prefab Labels, the Viewer will display all the prefabs. (Can be slow if you have many prefabs)")]
        public bool displayAllPrefabs = false;

        [Space(10)]
        [Tooltip("When set to true, all prefabs with the Ignore label will be excluded.")]
        public bool useIgnoreTag = true;

        [Space(10)]
        [Tooltip("When set to true, it will show the path of the folders below. (included - excluded)")]
        public bool showFolderPath = false;
    }
}