using UnityEngine;
using UnityEditor;

namespace AssetLibraryManager
{
    public class PrefabViewer : EditorWindow
    {
        SettingsObject settings;

        Vector2 scrollView;

        static int columns = 5;

        int edgePadding = 5;
        int thumbSpacing = 2;

        int totalWidth;
        int totalHeight;

        public static int bottomHeight = 20;

        Texture2D placeHolder;


        [MenuItem("Tools/Asset Library Manager/Prefab Viewer...", false, 67)]
        public static void ShowWindow()
        {
            GetWindow<PrefabViewer>("Prefab Viewer");
        }

        void CreateGUI()
        {
            settings = Resources.Load("AssetLibrary_Settings") as SettingsObject;

            placeHolder = Resources.Load("icons/alm/loading") as Texture2D;

            // (!HasOpenInstances<PrefabLabels>())
            //{
            if (SearchLabels.selectedLabels.Count == 0 && settings.options.displayAllPrefabs == true)
            {
                ReloadAllPrefabs();
            }
            //}
        }

        void OnDisable()
        {
            if (!HasOpenInstances<PrefabLabels>())
            {
                PrefabLabels.Clear_Labels();

                SearchLabels.Clear_CachePrefabs();
            }

            //Debug.Log($"Disable");
        }


        private void OnGUI()
        {
            //Reload thumbnail if null
            for (int i = 1; i < SearchLabels.cacheThumbs.Count; i++)
            {
                bool isLoading = false;

                if (SearchLabels.cacheThumbs[i] != null)
                {
                    isLoading = AssetPreview.IsLoadingAssetPreview(SearchLabels.cacheThumbs[i].GetInstanceID());
                }

                else if (isLoading == false)
                {
                    SearchLabels.cacheThumbs[i] = AssetPreview.GetAssetPreview(SearchLabels.selectedPrefabs[i]);
                }
            }

            #region PREFAB_GRID_LAYOUT

            float horizontalSpacing = 0;
            float verticalSpacing = 0;

            int rows = 1;

            float scrollRatio = scrollView.y / (totalHeight - 631); //Debug.Log($"{scrollRatio}");

            //float strech = (position.width - totalWidth) / columns;

            scrollView = GUILayout.BeginScrollView(scrollView);
            {
                GUILayout.Label("", GUILayout.Width(totalWidth - 8), GUILayout.Height(totalHeight));

                GUIContent someContent = new GUIContent();

                for (int i = 1; i < SearchLabels.selectedPrefabs.Count; i++)
                {
                    //someContent.tooltip = "";
                    if (SearchLabels.cacheThumbs[i] == null)
                    {
                        someContent.image = placeHolder;
                    }
                    else
                    {
                        someContent.image = SearchLabels.cacheThumbs[i];
                    }

                    GUILayout.BeginArea(new Rect(edgePadding + horizontalSpacing, edgePadding + verticalSpacing, settings.options.thumbnailSize, settings.options.thumbnailSize), someContent);

                    if (Event.current.type == EventType.MouseDown)
                    {
                        DragAndDrop.PrepareStartDrag();
                        DragAndDrop.StartDrag("Dragging");
                        DragAndDrop.objectReferences = new Object[] { SearchLabels.selectedPrefabs[i] };
                    }

                    if (Event.current.type == EventType.MouseDown)
                    {
                        if (settings.options.onClick == Options.OnSelect.selectPrefab)
                        {
                            Selection.activeObject = SearchLabels.selectedPrefabs[i];
                        }

                        if (settings.options.onClick == Options.OnSelect.pingPrefab)
                        {
                            EditorGUIUtility.PingObject(SearchLabels.selectedPrefabs[i]);
                        }

                        if (settings.options.onClick == Options.OnSelect.showPreview)
                        {
                            if (!HasOpenInstances<Preview_Window>())
                            {
                                Preview_Window.ShowWindow();
                            }

                            Selection.activeObject = SearchLabels.selectedPrefabs[i];
                        }
                    }

                    horizontalSpacing += settings.options.thumbnailSize + thumbSpacing; //+ strech;

                    if (i % columns == 0)
                    {
                        horizontalSpacing = 0;

                        verticalSpacing += settings.options.thumbnailSize + thumbSpacing;
                    }

                    if ((i - 1) % columns == 0)
                    {
                        rows++;
                    }

                    GUILayout.EndArea();
                }
            }

            rows--;

            GUILayout.EndScrollView();
            #endregion


            #region AUTOMATIC_GRID_LAYOUT
            if (settings != null)
            {
                //             window padding                  number of thumbnails                       thumbnail padding    
                totalWidth = (edgePadding * 2) + (columns * settings.options.thumbnailSize) + (thumbSpacing * (columns - 1));
                totalHeight = (edgePadding * 2) + (rows * settings.options.thumbnailSize) + (thumbSpacing * (rows - 1));

                if (Event.current.type == EventType.Repaint)
                {
                    if (position.width > totalWidth + settings.options.thumbnailSize)
                    {
                        columns++;
                    }

                    if (position.width < totalWidth && columns != 1)
                    {
                        columns--;
                    }
                }
            }
            #endregion

            //GUILayout.Space(edgePadding);

            #region FOOTER
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();


            GUIContent content = new GUIContent();

            int count = SearchLabels.selectedPrefabs.Count - 1;

            if (SearchLabels.selectedPrefabs.Count == 0)
            {
                count = 0;
            }

            content.image = Resources.Load("icons/alm/reload") as Texture2D;

            if (GUILayout.Button(content, CustomGUIStyles.FooterStyle()))
            {
                ReloadAllPrefabs();
            }

            GUILayout.Label($"Refresh Prefabs: {count}");

            GUILayout.FlexibleSpace();


            content.tooltip = "Select all prefabs in viewer";
            content.image = Resources.Load("icons/alm/select") as Texture2D;

            if (GUILayout.Button(content, CustomGUIStyles.FooterStyle()))
            {
                if (SearchLabels.selectedPrefabs.Count != 0)
                {
                    EditorGUIUtility.PingObject(SearchLabels.selectedPrefabs[0]);
                }

                if (SearchLabels.selectedPrefabs.Count > 500)
                {
                    if (EditorUtility.DisplayDialog("Select Confirmation", $"Selecting {SearchLabels.selectedPrefabs.Count} prefabs.\n\nAre you sure?", "Ok", "Cancel"))
                    {
                        Selection.objects = SearchLabels.selectedPrefabs.ToArray();

                        Debug.Log($"{count} prefabs selected");
                    }
                }
                else
                {
                    Selection.objects = SearchLabels.selectedPrefabs.ToArray();

                    Debug.Log($"{count} prefabs selected");
                }
            }



            GUILayout.EndHorizontal();
            #endregion
        }

        public static void ReloadAllPrefabs()
        {
            SearchLabels.Clear_CachePrefabs();
            SearchLabels.GetGUIDs();
            SearchLabels.LoadAllPrefabs();

            PrefabLabels.SetUp_Labels();

            //Debug.Log($"Prefabs loaded.");
        }


        void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}