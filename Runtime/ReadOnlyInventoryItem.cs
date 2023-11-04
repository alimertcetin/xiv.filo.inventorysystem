using UnityEngine;

namespace XIV.Packages.InventorySystem
{
    /// <summary>
    /// Provides a safe way to read values of <see cref="InventoryItem"/>.
    /// </summary>
    [System.Serializable]
    public readonly struct ReadOnlyInventoryItem
    {
        /// <summary>
        /// Invalid <see cref="ReadOnlyInventoryItem"/>
        /// </summary>
        public static readonly ReadOnlyInventoryItem InvalidReadonlyInventoryItem = new ReadOnlyInventoryItem(InventoryItem.InvalidInventoryItem);
        /// <summary>
        /// Points to the index in <see cref="Inventory"/>.
        /// </summary>
        [field: SerializeField] public int Index { get; }
        /// <summary>
        /// The quantity of item.
        /// </summary>
        [field: SerializeField] public int Quantity { get; }
        /// <summary>
        /// The stored item.
        /// </summary>
        [field: SerializeField] public ItemBase Item { get; }
        /// <summary>
        /// Determines if this <see cref="ReadOnlyInventoryItem"/> is empty or not.
        /// </summary>
        public bool IsEmpty => Quantity <= 0;

        public ReadOnlyInventoryItem(InventoryItem inventoryItem)
        {
            this.Index = inventoryItem.Index;
            this.Quantity = inventoryItem.Quantity;
            this.Item = inventoryItem.Item;
        }
    }
}