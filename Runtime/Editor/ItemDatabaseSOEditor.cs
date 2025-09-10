using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace XIV.Packages.InventorySystem.ScriptableObjects
{
    [CustomEditor(typeof(ItemDatabaseSO))]
    class ItemDatabaseSOEditor : UnityEditor.Editor
    {
        const string BTN_LOAD_STRING = "Load All Items";
        const string PATH = "Assets";
        
        public override void OnInspectorGUI()
        {
            ItemDatabaseSO container = (ItemDatabaseSO)target;

            if (GUILayout.Button(BTN_LOAD_STRING))
            {
                var dataContainers = LoadAssetsOfType<ItemSO>(PATH);
                Undo.RecordObject(container, BTN_LOAD_STRING);
                var items = new ItemSO[dataContainers.Count];
                for (int i = 0; i < dataContainers.Count; i++)
                {
                    items[i] = dataContainers[i];
                }

                typeof(ItemDatabaseSO).GetField("items", GetFlags()).SetValue(container, items);
                EditorUtility.SetDirty(container);
                AssetDatabase.SaveAssetIfDirty(container);
            }
            
            base.OnInspectorGUI();
        }

        static BindingFlags GetFlags()
        {
            return BindingFlags.Instance | BindingFlags.NonPublic;
        }
        
        static List<TAsset> LoadAssetsOfType<TAsset>(string folderPath, 
            SearchOption searchOption = SearchOption.AllDirectories)
            where TAsset : Object
        {
            string[] assetPaths = Directory.GetFiles(folderPath, "*", searchOption);
            
            List<TAsset> list = new List<TAsset>();
            for (int i = 0; i < assetPaths.Length; i++)
            {
                TAsset asset = AssetDatabase.LoadAssetAtPath<TAsset>(assetPaths[i]);
                if (asset == null) continue;
                list.Add(asset);
            }

            return list;
        }
    }
}