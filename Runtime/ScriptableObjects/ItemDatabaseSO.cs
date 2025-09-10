using System.Collections.Generic;
using UnityEngine;

namespace XIV.Packages.InventorySystem.ScriptableObjects
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
        /// Retrieves the <see cref="ItemSO"/> associated with a given <see cref="ItemBase"/>.
        /// </summary>
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
        /// Retrieves the first <see cref="ItemSO"/> associated with <typeparamref name="T"/>.
        /// </summary>
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

        /// <summary>
        /// Retrieves a list of <see cref="ItemSO"/>s that are of the specified type <typeparamref name="T"/>.
        /// </summary>
        public List<ItemSO> GetItemSOsOfType<T>() where T : ItemBase
        {
            List<ItemSO> list = new List<ItemSO>();
            int length = items.Length;
            for (int i = 0; i < length; i++)
            {
                var itemSO = items[i];
                if (itemSO is ItemSO<T> so)
                {
                    list.Add(so);
                }
            }

            return list;
        }

        /// <summary>
        /// Retrieves a list of items of type <typeparamref name="T"/>, converting them from <see cref="ItemSO"/> to the specified type.
        /// </summary>
        public List<T> GetItemsOfType<T>() where T : ItemBase
        {
            List<T> list = new List<T>();
            int length = items.Length;
            for (int i = 0; i < length; i++)
            {
                var itemSO = items[i];
                if (itemSO is ItemSO<T> so)
                {
                    list.Add(so.GetItemT());
                }
            }

            return list;
        }
    }
}