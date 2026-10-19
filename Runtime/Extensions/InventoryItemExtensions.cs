namespace XIV.Packages.InventorySystem.Extensions
{
    /// <summary>
    /// Contains extension methods for <see cref="InventorySlot"/> and <see cref="ReadOnlyInventorySlot"/>.
    /// </summary>
    public static class InventoryItemExtensions
    {
        /// <summary>
        /// Creates a new <see cref="InventorySlot"/> using <see cref="ReadOnlyInventorySlot"/>
        /// </summary>
        public static InventorySlot NewInventoryItem(this ReadOnlyInventorySlot readOnlyInventorySlot)
        {
            return new InventorySlot(readOnlyInventorySlot.Index, 
                readOnlyInventorySlot.Quantity, readOnlyInventorySlot.Item);
        }
        
        /// <summary>
        /// Creates a new <see cref="ReadOnlyInventorySlot"/> using <see cref="InventorySlot"/>
        /// </summary>
        public static ReadOnlyInventorySlot ToReadOnly(this InventorySlot inventorySlot)
        {
            return new ReadOnlyInventorySlot(inventorySlot);
        }
    }
}