using UnityEditor;
using UnityEngine;

namespace FS_CombatSystem
{
    public class RagdollUtility : EditorWindow
    {
        GameObject character;

        [MenuItem("Tools/FS Combat System/Setup Ragdoll")]
        public static void ShowWindow()
        {
            GetWindow<RagdollUtility>("Ragdoll Setup");
        }

        void OnGUI()
        {
            GUILayout.Label("Setup Ragdoll for Character", EditorStyles.boldLabel);
            character = (GameObject)EditorGUILayout.ObjectField("Character", character, typeof(GameObject), true);

            if (GUILayout.Button("Add Ragdoll Components") && character != null)
            {
                SetupRagdoll(character);
            }
        }

        void SetupRagdoll(GameObject root)
        {
            Animator animator = root.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator not found on character.");
                return;
            }

            foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
            {
                if (bone == HumanBodyBones.LastBone) continue;

                var t = animator.GetBoneTransform(bone);
                if (t == null) continue;

                if (!t.TryGetComponent(out Collider _))
                    t.gameObject.AddComponent<CapsuleCollider>();

                if (!t.TryGetComponent(out Rigidbody rb))
                    rb = t.gameObject.AddComponent<Rigidbody>();

                rb.isKinematic = true;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
            }

            Debug.Log("Ragdoll components added! Adjust colliders manually if needed.");
        }
    }
}
