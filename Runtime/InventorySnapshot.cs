namespace XIV.Packages.InventorySystem
{
    /// <summary>
    /// Stores a snapshot of an <see cref="Inventory"/> state.
    /// Contains the total number of slots and the non-empty items stored in those slots.
    /// </summary>
    [System.Serializable]
    public readonly struct InventorySnapshot
    {
        /// <summary>
        /// The number of slots in the <see cref="Inventory"/> when the snapshot was created.
        /// </summary>
        public readonly int slotCount;

        /// <summary>
        /// The non-empty items stored in the <see cref="Inventory"/> when the snapshot was created.
        /// Each item's <see cref="InventorySlot.Index"/> determines its slot.
        /// </summary>
        public readonly InventorySlot[] occupiedSlots;

        public InventorySnapshot(int slotCount, InventorySlot[] occupiedSlots)
        {
            this.slotCount = slotCount;
            this.occupiedSlots = occupiedSlots;
        }
    }
}