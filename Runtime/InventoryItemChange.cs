namespace XIV.Packages.InventorySystem
{
    /// <summary>
    /// Stores the changes about the <see cref="ItemBase"/>
    /// </summary>
    /// <seealso cref="InventoryChange"/>
    public readonly struct InventoryItemChange
    {
        /// <summary>
        /// Data before the changes.
        /// </summary>
        public readonly ReadOnlyInventoryItem before;
        /// <summary>
        /// Data after the changes
        /// </summary>
        public readonly ReadOnlyInventoryItem after;
        /// <summary>
        /// Determines if this item is moved or not.
        /// </summary>
        public readonly bool isMoved;
        /// <summary>
        /// Determines if this item is merged or not.
        /// </summary>
        /// <seealso cref="Inventory.Merge"/>
        public readonly bool isMerged;
        /// <summary>
        /// Determines if this item is discarded or not.
        /// This is true only if slots in inventory removed and there is no place to put this item.
        /// </summary>
        /// <seealso cref="Inventory.RemoveSlot"/>
        public readonly bool isDiscarded;
        
        public InventoryItemChange(ReadOnlyInventoryItem before, ReadOnlyInventoryItem after, bool isDiscarded)
        {
            this.before = before;
            this.after = after;
            this.isDiscarded = isDiscarded;
            this.isMoved = this.before.Index != this.after.Index;
            this.isMerged = isMoved && before.Quantity < after.Quantity;
        }
    }
}