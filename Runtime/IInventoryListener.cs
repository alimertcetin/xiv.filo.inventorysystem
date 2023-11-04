namespace XIV.Packages.InventorySystem
{
    /// <summary>
    /// Provides a mechanism for receiving <see cref="InventoryChange"/> information.
    /// </summary>
    public interface IInventoryListener
    {
        /// <summary>
        /// Provides the <see cref="InventoryChange"/> data.
        /// </summary>
        /// <param name="inventoryChange">The current change information.</param>
        void OnInventoryChanged(InventoryChange inventoryChange);
    }
}