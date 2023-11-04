namespace XIV.Packages.InventorySystem.Extensions
{
    /// <summary>
    /// Contains extension methods for <see cref="InventoryItem"/> and <see cref="ReadOnlyInventoryItem"/>.
    /// </summary>
    public static class InventoryItemExtensions
    {
        /// <summary>
        /// Creates a new <see cref="InventoryItem"/> using <see cref="ReadOnlyInventoryItem"/>
        /// </summary>
        public static InventoryItem NewInventoryItem(this ReadOnlyInventoryItem readOnlyInventoryItem)
        {
            return new InventoryItem(readOnlyInventoryItem.Index, 
                readOnlyInventoryItem.Quantity, readOnlyInventoryItem.Item);
        }
        
        /// <summary>
        /// Creates a new <see cref="ReadOnlyInventoryItem"/> using <see cref="InventoryItem"/>
        /// </summary>
        public static ReadOnlyInventoryItem ToReadOnly(this InventoryItem inventoryItem)
        {
            return new ReadOnlyInventoryItem(inventoryItem);
        }
    }
}