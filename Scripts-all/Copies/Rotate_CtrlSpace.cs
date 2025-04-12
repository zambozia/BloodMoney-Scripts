using UnityEditor;
using UnityEngine;

namespace AssetLibraryManager
{
    [InitializeOnLoad]
    public class Rotate_CtrlSpace : MonoBehaviour
    {
        [MenuItem("Tools/Asset Library Manager/Rotate with Space... %SPACE", false, 1044)]
        // Update is called once per frame
        public static void Rotate_Space()
        {
            Undo.RegisterCompleteObjectUndo(Selection.transforms, "Rotation");

            foreach (GameObject item in Selection.gameObjects)
            {
                float rot = item.transform.localEulerAngles.y;
                rot += EditorSnapSettings.rotate;

                item.transform.localEulerAngles = new Vector3(0, rot, 0);
            }
        }
    }
}
