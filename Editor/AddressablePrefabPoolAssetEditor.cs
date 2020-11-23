using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Lab5Games.Editor
{
    [CustomEditor(typeof(AddressablePrefabPoolAsset))]
    public class AddressablePrefabPoolAssetEditor : UnityEditor.Editor
    {
        ReorderableList _rList;
        SerializedProperty _spPools;


        private void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Pools");
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = _rList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;

            /*SerializedProperty spPrefab = element.FindPropertyRelative("prefab");
            GameObject refObject = (GameObject)spPrefab.objectReferenceValue;
            string elementTitle = refObject == null ? "None" : refObject.name;*/

            string elementTitle = "Pool " + index;

            EditorGUI.PropertyField(new Rect(rect.x += 10, rect.y, Screen.width * .8f, EditorGUIUtility.singleLineHeight),
                element, new GUIContent(elementTitle), true);
        }

        private float ElementHeightCallback(int index)
        {
            float propertyHeight = EditorGUI.GetPropertyHeight(_rList.serializedProperty.GetArrayElementAtIndex(index), true);

            float spacing = EditorGUIUtility.singleLineHeight / 2;

            return propertyHeight + spacing;
        }

        private void OnAddCallback(ReorderableList list)
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;

            var element = list.serializedProperty.GetArrayElementAtIndex(index);
        }

        private void OnEnable()
        {
            _spPools = serializedObject.FindProperty("pools");

            _rList = new ReorderableList(serializedObject, _spPools, true, true, true, true);

            _rList.drawHeaderCallback = DrawHeaderCallback;
            _rList.drawElementCallback = DrawElementCallback;
            _rList.elementHeightCallback += ElementHeightCallback;
            _rList.onAddCallback += OnAddCallback;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _rList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
