namespace XIV.Packages.InventorySystem
{
    /// <summary>
    /// Determines how this item participated in a merge.
    /// </summary>
    public enum ItemMergeState
    {
        None,
        ReceivedMerge,
        ConsumedByMerge,
    }
    /// <summary>
    /// Stores information about a merge involving this item.
    /// </summary>
    public readonly struct ItemMergeData
    {
        /// <summary>
        /// Determines how this item participated in the merge.
        /// </summary>
        public readonly ItemMergeState mergeState;
        /// <summary>
        /// The slot index of the other item involved in the merge.
        /// </summary>
        public readonly int otherSlotIndex;

        public ItemMergeData(ItemMergeState mergeState, int otherSlotIndex)
        {
            this.mergeState = mergeState;
            this.otherSlotIndex = otherSlotIndex;
        }
    }
    /// <summary>
    /// Stores the changes about the <see cref="ItemBase"/>
    /// </summary>
    /// <seealso cref="InventoryChange"/>
    public readonly struct InventorySlotChange
    {
        /// <summary>
        /// Slot's snapshot before the change.
        /// </summary>
        public readonly ReadOnlyInventorySlot before;
        /// <summary>
        /// Slot's snapshot after the change.
        /// </summary>
        public readonly ReadOnlyInventorySlot after;
        /// <summary>
        /// Determines if this slot's index has changed or not.
        /// </summary>
        public readonly bool isMoved;
        /// <summary>
        /// Determines if an item newly added to this slot. Occurs only if slot is previously empty.
        /// </summary>
        public readonly bool isNew;
        /// <summary>
        /// Determines if slot's item quantity is increased.
        /// </summary>
        public readonly bool isNewlyAddedToStack;
        /// <summary>
        /// Determines if a merge operation happened in this change.
        /// </summary>
        /// <seealso cref="mergeData"/>
        public readonly bool isMerged;
        /// <summary>
        /// Stores merge related information. Relevant only if <see cref="isMerged"/> == true
        /// </summary>
        public readonly ItemMergeData mergeData;
        /// <summary>
        /// Determines if this item is discarded or not.
        /// This is true only if slots in inventory removed and there is no place to put this item.
        /// </summary>
        /// <seealso cref="Inventory.RemoveSlot"/>
        public readonly bool isDiscarded;

        public InventorySlotChange(ReadOnlyInventorySlot before, ReadOnlyInventorySlot after, ItemMergeData itemMergeData, bool isDiscarded)
        {
            this.before = before;
            this.after = after;
            this.isDiscarded = isDiscarded;
            // TODO: InventorySlotChange -> We never modify the slot index, thus isMoved is always false. Should swap modify the index?
            this.isMoved = before.Index != after.Index;
            this.isNew = before.IsEmpty && after.IsEmpty == false;
            this.isNewlyAddedToStack = before.IsEmpty == false && before.Quantity < after.Quantity;

            this.isMerged = itemMergeData.mergeState != ItemMergeState.None;
            this.mergeData = itemMergeData;
        }
    }
}