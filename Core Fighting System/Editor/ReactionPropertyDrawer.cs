using UnityEditor;
using UnityEngine;

namespace FS_CombatSystem
{
    [CustomPropertyDrawer(typeof(Reaction), true)]
    public class ReactionPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);



            var animationClipPos = new Rect(position.x, position.y + 2, position.width, EditorGUIUtility.singleLineHeight);
            var willBeKnockedDownPos = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 4, position.width, EditorGUIUtility.singleLineHeight);
            var directionPos = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 2 + 6, position.width, EditorGUIUtility.singleLineHeight);
            var timeRangePos = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 3 + 8, position.width, EditorGUIUtility.singleLineHeight);
            var overridePos = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 4 + 10, position.width, EditorGUIUtility.singleLineHeight);
            var lyingAnimationPos = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 5 + 12, position.width, EditorGUIUtility.singleLineHeight);
            var getupAnimationPos = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 6 + 14, position.width, EditorGUIUtility.singleLineHeight);

            var animationClip = property.FindPropertyRelative("animationClip");
            var willBeKnockedDown = property.FindPropertyRelative("willBeKnockedDown");
            var knockDownDirection = property.FindPropertyRelative("knockDownDirection");
            var lyingDownTimeRange = property.FindPropertyRelative("lyingDownTimeRange");
            var overrideLyingDownAnimation = property.FindPropertyRelative("overrideLyingDownAnimation");
            var lyingDownAnimation = property.FindPropertyRelative("lyingDownAnimation");
            var getUpAnimation = property.FindPropertyRelative("getUpAnimation");

            EditorGUI.PropertyField(animationClipPos, animationClip);
            EditorGUI.PropertyField(willBeKnockedDownPos, willBeKnockedDown);
            if (willBeKnockedDown.boolValue)
            {
                EditorGUI.PropertyField(directionPos, knockDownDirection);
                EditorGUI.PropertyField(timeRangePos, lyingDownTimeRange);
                EditorGUI.PropertyField(overridePos, overrideLyingDownAnimation);

                if (overrideLyingDownAnimation.boolValue)
                {
                    EditorGUI.PropertyField(lyingAnimationPos, lyingDownAnimation);
                    EditorGUI.PropertyField(getupAnimationPos, getUpAnimation);
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty willBeKnockedDownProp = property.FindPropertyRelative("willBeKnockedDown");
            SerializedProperty overrideLyingAnimProp = property.FindPropertyRelative("overrideLyingDownAnimation");

            float height = EditorGUIUtility.singleLineHeight * 2 + 4;
            if (willBeKnockedDownProp.boolValue)
            {
                height = EditorGUIUtility.singleLineHeight * 5 + 12;
                if (overrideLyingAnimProp.boolValue)
                    height = EditorGUIUtility.singleLineHeight * 7 + 16;
            }

            return height;
        }
    }
}