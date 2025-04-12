using UnityEditor;
using UnityEngine;
using UnityEditor.Toolbars;
using AssetLibraryManager;

namespace AssetLibraryManager
{
    [EditorToolbarElement(id, typeof(SceneView))]
    class DropToGround : EditorToolbarButton
    {
        public const string id = "EditorTools/DropToGround";

        public DropToGround()
        {
            text = "";
            icon = Resources.Load("Icons/dropToGround") as Texture2D;
            tooltip = "Drop to Ground...";

            clicked += OnClick;
        }

        void OnClick()
        {
            Undo.RecordObjects(Selection.transforms, "Drop Objects");

            foreach (Transform tr in Selection.transforms)
            {
                Renderer[] renderers = tr.GetComponentsInChildren<Renderer>();
                var combinedBounds = renderers[0].bounds;

                foreach (Renderer render in renderers)
                {
                    combinedBounds.Encapsulate(render.bounds);
                }

                float diff = tr.position.y - combinedBounds.center.y;

                RaycastHit hit;

                if (Physics.Raycast(tr.position, Vector3.down, out hit, Mathf.Infinity))
                {
                    tr.position = new Vector3(tr.position.x, combinedBounds.extents.y + diff + hit.point.y, tr.position.z);
                }
                else
                {
                    tr.position = new Vector3(tr.position.x, combinedBounds.extents.y + diff, tr.position.z);
                }
            }
        }
    }


    [EditorToolbarElement(id, typeof(SceneView))]
    class RandomScale : EditorToolbarButton
    {
        public const string id = "EditorTools/RandomScale";

        public RandomScale()
        {
            text = "";
            icon = Resources.Load("Icons/scale") as Texture2D;
            tooltip = "Random Scale...";

            clicked += OnClick;
        }

        void OnClick()
        {
            Undo.RegisterCompleteObjectUndo(Selection.transforms, "RandomScale");

            foreach (GameObject item in Selection.gameObjects)
            {
                float randomScale = Random.Range(0.9f, 1.1f);


                Vector3 itemScale = item.transform.localScale;

                itemScale = new Vector3(itemScale.x * randomScale, itemScale.y * randomScale, itemScale.z * randomScale);

                item.transform.localScale = itemScale;
            }
        }
    }


    [EditorToolbarElement(id, typeof(SceneView))]
    class RandomYRotation : EditorToolbarButton
    {
        public const string id = "EditorTools/RandomYRotation";

        public RandomYRotation()
        {
            text = "";
            icon = Resources.Load("Icons/rotation") as Texture2D;
            tooltip = "Random Y Rotation...";

            clicked += OnClick;
        }

        void OnClick()
        {
            Undo.RegisterCompleteObjectUndo(Selection.transforms, "RandomYRotation");

            foreach (GameObject item in Selection.gameObjects)
            {
                item.transform.Rotate(Vector3.up * Random.Range(0, 360));
            }
        }
    }


    [EditorToolbarElement(id, typeof(SceneView))]
    class Open_ALM : EditorToolbarButton
    {
        public const string id = "EditorTools/Open_ALM";

        public Open_ALM()
        {
            text = "";
            icon = Resources.Load("Icons/alm/assetlibrary") as Texture2D;
            tooltip = "Open Asset Library Manager...";

            clicked += OnClick;
        }

        void OnClick()
        {
            Undo.RegisterCompleteObjectUndo(Selection.transforms, "Open_ALM");

            PrefabLabels.ShowWindow();
        }
    }
}