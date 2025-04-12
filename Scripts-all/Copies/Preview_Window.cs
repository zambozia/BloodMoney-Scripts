using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace AssetLibraryManager
{
    public class Preview_Window : EditorWindow
    {
        Object selection;
        Editor gameObjectEditor;

        int totalVertices = 0;
        int totalTris = 0;


        [MenuItem("Tools/Asset Library Manager/Preview Window...", false, 68)]
        public static void ShowWindow()
        {
            Preview_Window window = GetWindow<Preview_Window>("Preview Window");
            window.minSize = new Vector2(20, 20);
        }

        void CreateGUI()
        {
            selection = Selection.activeObject;

            gameObjectEditor = Editor.CreateEditor(selection);

            VertexCount_Labels();

            LabelsManager.OnLabelsChanged += VertexCount_Labels;
        }

        private void OnDisable()
        {
            LabelsManager.OnLabelsChanged -= VertexCount_Labels;
        }


        void OnGUI()
        {
            if (selection != null)
            {
                gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(0, 1920, 0, 1080), new GUIStyle());

                GUILayout.BeginHorizontal();

                for (int i = 0; i < AssetDatabase.GetLabels(Selection.activeObject).Length; i++)
                {
                    GUIContent someContent = new GUIContent();
                    someContent.text = AssetDatabase.GetLabels(Selection.activeObject)[i];

                    GUIStyle someStyle = new GUIStyle();
                    someStyle.normal.background = Resources.Load("icons/alm/lbl") as Texture2D;
                    someStyle.normal.textColor = Color.white;
                    someStyle.alignment = TextAnchor.MiddleCenter;

                    Vector2 contentSize = someStyle.CalcSize(someContent);

                    someStyle.fixedWidth = contentSize.x + 20;
                    someStyle.fixedHeight = contentSize.y + 2;

                    if (i == 5 || i == 10 || i == 15)
                    {
                        GUILayout.EndHorizontal();

                        GUILayout.Space(2);

                        GUILayout.BeginHorizontal();
                    }

                    if (GUILayout.Button(someContent, someStyle)) { }
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.Space(5);

            #region FOOTER
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();

            if (selection != null)
            {
                GUILayout.Label($"Name: {selection.name}");
            }

            else
            {
                GUILayout.Label($"Name:");
            }

            GUILayout.FlexibleSpace();

            if (totalVertices != 0 && totalTris != 0)
            {
                GUILayout.Label($"Vertices: {totalVertices}");

                GUILayout.Label($"Triangles: {totalTris}");
            }


            GUIContent content = new GUIContent();

            content.image = Resources.Load("icons/alm/labels") as Texture2D;
            content.tooltip = "Label Manager";

            if (GUILayout.Button(content, CustomGUIStyles.FooterStyle()))
            {
                LabelsManager.ShowWindow();
            }

            GUILayout.EndHorizontal();
            #endregion
        }

        private void OnSelectionChange()
        {
            VertexCount_Labels();

            Repaint();
        }

        ///<summary>A summary of the method.</summary>
        private void VertexCount_Labels()
        {
            selection = Selection.activeObject;

            DestroyImmediate(gameObjectEditor);

            gameObjectEditor = Editor.CreateEditor(selection);

            totalVertices = 0;
            totalTris = 0;

            if (selection != null && selection is GameObject)
            {
                // check all meshes
                MeshFilter[] meshFilters = Selection.activeGameObject.GetComponentsInChildren<MeshFilter>();

                for (int i = 0, length = meshFilters.Length; i < length; i++)
                {
                    if (meshFilters[i].sharedMesh != null)
                    {
                        int verts = meshFilters[i].sharedMesh.vertexCount;
                        totalVertices += verts;

                        totalTris += meshFilters[i].sharedMesh.triangles.Length / 3;
                    }

                }

                SkinnedMeshRenderer[] skinMeshRenderer = Selection.activeGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();

                for (int i = 0, length = skinMeshRenderer.Length; i < length; i++)
                {
                    if (skinMeshRenderer[i].sharedMesh != null)
                    {
                        int verts = skinMeshRenderer[i].sharedMesh.vertexCount;
                        totalVertices += verts;

                        totalTris += skinMeshRenderer[i].sharedMesh.triangles.Length / 3;
                    }
                }             
            }

            else
            {
                totalTris = 0;
                totalVertices = 0;
            }
        }

        void OnInspectorUpdate()
        {
            //Repaint();
        }
    }
}