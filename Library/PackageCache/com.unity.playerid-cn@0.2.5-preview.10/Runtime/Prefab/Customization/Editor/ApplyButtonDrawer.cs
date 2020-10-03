#if UNITY_EDITOR
using UnityEditor;
using System;
using TMPro;

namespace UnityEngine.PlayerIdentity.UI.Customizer
{
    [CustomPropertyDrawer(typeof(ApplyButtonAttribute))]
    public class ApplyButtonDrawer : PropertyDrawer
    {
        private const int BUTTON_WIDTH = 50;
        private const int BUTTON_MARGIN = 10;

        ApplyButtonAttribute applyButtonAttribute { get { return ((ApplyButtonAttribute)attribute); } }
        
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(prop);
        }

        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, prop);

            position.width -= (BUTTON_WIDTH + BUTTON_MARGIN);
            
            EditorGUI.PropertyField(position, prop);

            position.x += position.width + BUTTON_MARGIN;
            position.width = BUTTON_WIDTH;

            float propertyHeight = GetPropertyHeight(prop, label);
            position.y += propertyHeight > EditorGUIUtility.singleLineHeight ? propertyHeight - EditorGUIUtility.singleLineHeight : 0;

            position.height = EditorGUIUtility.singleLineHeight;

            if (GUI.Button(position, "Apply"))
            {
                Customizer customizer = prop.serializedObject.targetObject as Customizer;

                if (customizer != null)
                {
                    switch (applyButtonAttribute.CustomizedType.Name)
                    {
                        case nameof(TMP_FontAsset):
                            customizer.ApplyProperty(applyButtonAttribute.CallbackName, applyButtonAttribute.DelegateName, prop.objectReferenceValue as TMP_FontAsset);
                            break;
                        case nameof(Sprite):
                            customizer.ApplyProperty(applyButtonAttribute.CallbackName, applyButtonAttribute.DelegateName, prop.objectReferenceValue as Sprite);
                            break;
                        case nameof(Color):
                            customizer.ApplyProperty(applyButtonAttribute.CallbackName, applyButtonAttribute.DelegateName, prop.colorValue);
                            break;
                        case nameof(Vector2):
                            customizer.ApplyProperty(applyButtonAttribute.CallbackName, applyButtonAttribute.DelegateName, prop.vector2Value);
                            break;
                        case nameof(Single):
                            customizer.ApplyProperty(applyButtonAttribute.CallbackName, applyButtonAttribute.DelegateName, prop.floatValue);
                            break;
                        default:
                            break;
                    }
                }
            }
            
            EditorGUI.EndProperty();
        }
    }
}
#endif