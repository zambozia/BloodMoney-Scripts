using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace AssetLibraryManager
{
    [InitializeOnLoad]
    public class SceneGUI_Methods : EditorWindow
    {
        public static bool disableSceneGUI = true;

        public static bool useScale = true;
        public static bool useScaleSnap = false;

        public static bool useRotation = true;
        public static bool useRotationSnap = false;

        public static int interval = 0;

        static SceneGUI_Methods()
        {
            SceneView.duringSceneGui += SceneGUI;
        }

        public static void SceneGUI(SceneView sceneView)
        {
            if (disableSceneGUI == false)
            {
                //Rotate object when ctrl and shift are pressed.
                if (Event.current.control && Event.current.shift)
                {
                    interval++;

                    if (interval > 100)
                    {
                        interval = 0;
                    }

                    Undo.RegisterCompleteObjectUndo(Selection.transforms, "Scale");

                    Vector2 delta = Event.current.delta;

                    foreach (GameObject item in Selection.gameObjects)
                    {
                        if (useScale == true)
                        {
                            if (useScaleSnap == true)
                            {
                                if (interval % 5 == 0)
                                {

                                    Vector3 scale = item.transform.localScale;

                                    float xyRatio = scale.x / scale.y;
                                    float xzRatio = scale.x / scale.z;

                                    if (delta.y < -0.1f)
                                    {
                                        scale.x += EditorSnapSettings.scale;
                                    }

                                    if (delta.y > 0.1f)
                                    {
                                        scale.x -= EditorSnapSettings.scale;
                                    }

                                    scale.y = scale.x / xyRatio;
                                    scale.z = scale.x / xzRatio;

                                    if (scale.x > 0 && scale.y > 0 && scale.z > 0)
                                    {
                                        item.transform.localScale = scale;
                                    }
                                }
                            }

                            else
                            {
                                Vector3 scale = item.transform.localScale;

                                float xyRatio = scale.x / scale.y;
                                float xzRatio = scale.x / scale.z;

                                scale.x -= delta.y * 0.02f;

                                scale.y = scale.x / xyRatio;
                                scale.z = scale.x / xzRatio;

                                if (scale.x > 0 && scale.y > 0 && scale.z > 0)
                                {
                                    item.transform.localScale = scale;
                                }
                            }
                        }

                        if (useRotation == true)
                        {
                            if (useRotationSnap == true)
                            {
                                if (interval % 10 == 0)
                                {
                                    float rot = item.transform.localEulerAngles.y;
                                    Vector3 angle = item.transform.localEulerAngles;

                                    if (delta.x > 0.1f)
                                    {
                                        rot += EditorSnapSettings.rotate;
                                    }

                                    if (delta.x < -0.1f)
                                    {
                                        rot -= EditorSnapSettings.rotate;
                                    }

                                    angle.y = rot;
                                    item.transform.localEulerAngles = angle;
                                }

                            }
                            else
                            {
                                item.transform.Rotate(Vector3.up * delta.x);
                            }
                        }
                    }
                }

                sceneView.Repaint();
            }
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    public class ToggleSceneGUI : EditorToolbarButton
    {
        public const string id = "Utilities/OnSceneGUI_Toggle";

        public ToggleSceneGUI()
        {
            SceneGUI_Methods.disableSceneGUI = false;

            text = "";

            icon = EditorGUIUtility.IconContent("d_Record On").image as Texture2D;

            tooltip = "Toggle OnSceneGui Scale Rotate";

            clicked += () => ToggleIcon();
        }

        private void ToggleIcon()
        {
            SceneGUI_Methods.disableSceneGUI = !SceneGUI_Methods.disableSceneGUI;

            if (SceneGUI_Methods.disableSceneGUI == true)
            {
                icon = EditorGUIUtility.IconContent("d_Record Off").image as Texture2D;
            }
            else
            {
                icon = EditorGUIUtility.IconContent("d_Record On").image as Texture2D;
            }
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    public class ScaleSceneGUI : EditorToolbarButton
    {
        public const string id = "Utilities/OnSceneGUI_UseScale";

        public ScaleSceneGUI()
        {
            text = "";

            if (SceneGUI_Methods.useScale == true)
            {
                icon = Resources.Load("Icons/scenegui/scale_on") as Texture2D;
            }
            else
            {
                icon = Resources.Load("Icons/scenegui/scale_off") as Texture2D;
            }

            tooltip = "Use Scale";

            clicked += () => ToggleIcon();
        }

        private void ToggleIcon()
        {
            SceneGUI_Methods.useScale = !SceneGUI_Methods.useScale;

            if (SceneGUI_Methods.useScale == true)
            {
                icon = Resources.Load("Icons/scenegui/scale_on") as Texture2D;
            }
            else
            {
                icon = Resources.Load("Icons/scenegui/scale_off") as Texture2D;
            }
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    public class ScaleSnapSceneGUI : EditorToolbarButton
    {
        public const string id = "Utilities/OnSceneGUI_UseScaleSnap";

        public ScaleSnapSceneGUI()
        {
            text = "";

            if (SceneGUI_Methods.useScaleSnap == true)
            {
                icon = EditorGUIUtility.IconContent("SceneViewSnap On@2x").image as Texture2D;
            }
            else
            {
                icon = Resources.Load("Icons/scenegui/rotation_snap_off") as Texture2D;
            }

            tooltip = "Use snap settings for scaling";

            clicked += () => ToggleIcon();
        }

        private void ToggleIcon()
        {
            SceneGUI_Methods.useScaleSnap = !SceneGUI_Methods.useScaleSnap;

            if (SceneGUI_Methods.useScaleSnap == true)
            {
                icon = EditorGUIUtility.IconContent("SceneViewSnap On@2x").image as Texture2D;
            }
            else
            {
                icon = Resources.Load("Icons/scenegui/rotation_snap_off") as Texture2D;
            }
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    public class RotateSceneGUI : EditorToolbarButton
    {
        public const string id = "Utilities/OnSceneGUI_UseRotation";

        public RotateSceneGUI()
        {
            text = "";

            if (SceneGUI_Methods.useRotation == true)
            {
                icon = Resources.Load("Icons/scenegui/rotation_on") as Texture2D;
            }
            else
            {
                icon = Resources.Load("Icons/scenegui/rotation_off") as Texture2D;
            }

            tooltip = "Use Rotation";

            clicked += () => ToggleIcon();
        }

        private void ToggleIcon()
        {
            SceneGUI_Methods.useRotation = !SceneGUI_Methods.useRotation;

            if (SceneGUI_Methods.useRotation == true)
            {
                icon = Resources.Load("Icons/scenegui/rotation_on") as Texture2D;
            }
            else
            {
                icon = Resources.Load("Icons/scenegui/rotation_off") as Texture2D;
            }
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    public class RotateSnapSceneGUI : EditorToolbarButton
    {
        public const string id = "Utilities/OnSceneGUI_UseRotationSnap";

        public RotateSnapSceneGUI()
        {
            text = "";

            if (SceneGUI_Methods.useRotationSnap == true)
            {
                icon = EditorGUIUtility.IconContent("SceneViewSnap On@2x").image as Texture2D;
            }
            else
            {
                icon = Resources.Load("Icons/scenegui/rotation_snap_off") as Texture2D;
            }

            tooltip = "Use snap settings for rotation";

            clicked += () => ToggleIcon();
        }

        private void ToggleIcon()
        {
            SceneGUI_Methods.useRotationSnap = !SceneGUI_Methods.useRotationSnap;

            if (SceneGUI_Methods.useRotationSnap == true)
            {
                icon = EditorGUIUtility.IconContent("SceneViewSnap On@2x").image as Texture2D;
            }
            else
            {
                icon = Resources.Load("Icons/scenegui/rotation_snap_off") as Texture2D;
            }
        }
    }
}