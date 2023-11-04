using UnityEngine;

namespace XIV.Packages.InventorySystem
{
    /// <summary>
    /// Used in <see cref="Inventory"/> to store <see cref="ItemBase"/>.
    /// Contains inventory related data.
    /// </summary>
    /// <seealso cref="ReadOnlyInventoryItem"/>
    [System.Serializable]
    public struct InventoryItem
    {
        /// <summary>
        /// Invalid <see cref="InventoryItem"/>.
        /// </summary>
        public static readonly InventoryItem InvalidInventoryItem = new InventoryItem(-1, -1, null);
        /// <summary>
        /// Points to the index in <see cref="Inventory"/>.
        /// </summary>
        [field: SerializeField] public int Index { get; set; }
        /// <summary>
        /// The quantity of item.
        /// </summary>
        [field: SerializeField] public int Quantity { get; set; }
        /// <summary>
        /// The stored item.
        /// </summary>
        [field: SerializeField] public ItemBase Item { get; set; }
        /// <summary>
        /// Determines if this <see cref="InventoryItem"/> is empty or not.
        /// </summary>
        public bool IsEmpty => Quantity <= 0;

        public InventoryItem(int index, int quantity, ItemBase item)
        {
            this.Index = index;
            this.Quantity = quantity;
            this.Item = item;
        }
    }
}