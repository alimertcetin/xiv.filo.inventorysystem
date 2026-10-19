using UnityEngine;

namespace XIV.Packages.InventorySystem
{
    /// <summary>
    /// Provides a safe way to read values of <see cref="InventorySlot"/>.
    /// </summary>
    [System.Serializable]
    public readonly struct ReadOnlyInventorySlot
    {
        /// <summary>
        /// Invalid <see cref="ReadOnlyInventorySlot"/>
        /// </summary>
        public static readonly ReadOnlyInventorySlot InvalidReadonlyInventorySlot = new ReadOnlyInventorySlot(InventorySlot.InvalidInventorySlot);
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
        /// Determines if this <see cref="ReadOnlyInventorySlot"/> is empty or not.
        /// </summary>
        public bool IsEmpty => Quantity <= 0;

        public ReadOnlyInventorySlot(InventorySlot inventorySlot)
        {
            this.Index = inventorySlot.Index;
            this.Quantity = inventorySlot.Quantity;
            this.Item = inventorySlot.Item;
        }
        
        public static bool operator ==(ReadOnlyInventorySlot item1, ReadOnlyInventorySlot item2)
        {
            // TODO : Requires better equality comparison
            return item1.Index == item2.Index && item1.Quantity == item2.Quantity && item1.Item.Equals(item2.Item);
        }
        
        public static bool operator !=(ReadOnlyInventorySlot item1, ReadOnlyInventorySlot item2)
        {
            return !(item1 == item2);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(obj, null) == false && obj is ReadOnlyInventorySlot item && this == item;
        }
    }
}