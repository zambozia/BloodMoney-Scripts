using UnityEditor;
using UnityEditor.Overlays;

namespace AssetLibraryManager
{
    public class BonusTools_Layout : ToolbarOverlay
    {

        [Overlay(typeof(SceneView), "Bonus Tools")]
        public class Custom_EditorTools : ToolbarOverlay
        {
            Custom_EditorTools() : base(

                DropToGround.id,
                RandomScale.id,
                RandomYRotation.id,
                Open_ALM.id
                )
            { }
        }

        [Overlay(typeof(SceneView), "Scene Scale Rotate")]
        public class OnScenGUI_Rotate : ToolbarOverlay
        {
            OnScenGUI_Rotate() : base(

                ToggleSceneGUI.id,
                ScaleSceneGUI.id,
                ScaleSnapSceneGUI.id,
                RotateSceneGUI.id,
                RotateSnapSceneGUI.id

                )
            { }
        }
    }
}
