using XIV.Packages.InventorySystem.Editor;

namespace XIV.Packages.InventorySystem.xiv.filo.inventorysystem.Runtime.Editor
{
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(XIVSerializedDictionary<,>), true)]
    public class XIVSerializedDictionaryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var keyList = property.FindPropertyRelative("keys");
            var valueList = property.FindPropertyRelative("values");

            if (keyList == null || valueList == null) return;

            EditorGUI.PropertyField(position, keyList, new GUIContent("Keys"));
            position.y += EditorGUI.GetPropertyHeight(keyList) + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.PropertyField(position, valueList, new GUIContent("Values"));
            position.y += EditorGUI.GetPropertyHeight(valueList) + EditorGUIUtility.standardVerticalSpacing;

            // Optional: Add buttons for adding/removing entries
            if (GUI.Button(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), "Add Entry"))
            {
                keyList.arraySize++;
                valueList.arraySize++;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var keyList = property.FindPropertyRelative("keys");
            var valueList = property.FindPropertyRelative("values");
            
            if (keyList == null || valueList == null) return 0f;
            
            float height = EditorGUI.GetPropertyHeight(keyList) + EditorGUI.GetPropertyHeight(valueList) + 3 * EditorGUIUtility.standardVerticalSpacing;
            return height;
        }
    }

}