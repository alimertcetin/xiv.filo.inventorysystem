using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
#endif

namespace XIV_Packages.InventorySystem.ScriptableObjects
{
    /// <summary>
    /// Stores a collection of <see cref="ItemSO"/> and
    /// provides a way of mapping <see cref="ItemBase"/> to <see cref="ItemSO"/>.
    /// </summary>
    [CreateAssetMenu(menuName = MenuPaths.DATA_MENU + nameof(ItemDatabaseSO))]
    public class ItemDatabaseSO : ScriptableObject
    {
        [SerializeField] ItemSO[] items;

        /// <summary>
        /// Determines the <see cref="ItemSO"/> of a specific <see cref="ItemBase"/> in the <see cref="ItemDatabaseSO"/>.
        /// </summary>
        /// <param name="item">The item to locate in the list</param>
        /// <returns>The corresponding <see cref="ItemSO"/> if found in the collection; otherwise, -1.</returns>
        public ItemSO GetItemSO(ItemBase item)
        {
            int length = items.Length;
            for (int i = 0; i < length; i++)
            {
                var itemSO = items[i];
                if (itemSO.GetItem().Equals(item))
                {
                    return itemSO;
                }
            }

            return null;
        }

        /// <summary>
        /// Determines the <see cref="ItemSO"/> of a specific <typeparamref name="T"/> in the <see cref="ItemDatabaseSO"/>.
        /// </summary>
        /// <typeparam name="T">The item type to locate</typeparam>
        /// <returns>The corresponding <see cref="ItemSO"/> if found in the collection; otherwise, -1.</returns>
        public ItemSO<T> GetItemSO<T>() where T : ItemBase
        {
            int length = items.Length;
            for (int i = 0; i < length; i++)
            {
                var itemSO = items[i];
                if (itemSO is ItemSO<T> so)
                {
                    return so;
                }
            }

            return null;
        }
    }

#if UNITY_EDITOR
    
    [CustomEditor(typeof(ItemDatabaseSO))]
    public class ItemDatabaseSOEditor : UnityEditor.Editor
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
            return BindingFlags.Instance |
            BindingFlags.NonPublic;
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
#endif
}