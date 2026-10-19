using UnityEngine;

namespace XIV.Packages.InventorySystem
{
    /// <summary>
    /// Used in <see cref="Inventory"/> to store <see cref="ItemBase"/>.
    /// Contains inventory related data.
    /// </summary>
    /// <seealso cref="ReadOnlyInventorySlot"/>
    [System.Serializable]
    public struct InventorySlot
    {
        /// <summary>
        /// Invalid <see cref="InventorySlot"/>.
        /// </summary>
        public static readonly InventorySlot InvalidInventorySlot = new InventorySlot(-1, -1, null);
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
        /// Determines if this <see cref="InventorySlot"/> is empty or not.
        /// </summary>
        public bool IsEmpty => Quantity <= 0;

        public InventorySlot(int index, int quantity, ItemBase item)
        {
            this.Index = index;
            this.Quantity = quantity;
            this.Item = item;
        }
    }
}