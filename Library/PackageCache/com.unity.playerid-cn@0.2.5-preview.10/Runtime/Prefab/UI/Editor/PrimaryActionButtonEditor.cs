#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;

namespace UnityEngine.PlayerIdentity.UI
{
    [CustomEditor(typeof(PrimaryActionButton))]
    [CanEditMultipleObjects]
    public class PrimaryActionButtonEditor : ButtonEditor
    {
        SerializedProperty actionType;
        SerializedProperty activeButtonTextColor;
        SerializedProperty inactiveButtonTextColor;
        SerializedProperty buttonText;

        protected override void OnEnable()
        {
            base.OnEnable();

            actionType = serializedObject.FindProperty("actionType");
            buttonText = serializedObject.FindProperty("buttonText");
            activeButtonTextColor = serializedObject.FindProperty("activeButtonTextColor");
            inactiveButtonTextColor = serializedObject.FindProperty("inactiveButtonTextColor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(actionType);
            EditorGUILayout.PropertyField(buttonText);
            EditorGUILayout.PropertyField(activeButtonTextColor);
            EditorGUILayout.PropertyField(inactiveButtonTextColor);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            base.OnInspectorGUI();
        }
    }
}
#endif