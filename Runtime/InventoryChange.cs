using System;
using System.Collections.Generic;

namespace XIV.Packages.InventorySystem
{
    /// <summary>
    /// Stores the changes that has been made in <see cref="Inventory"/>.
    /// </summary>
    public readonly struct InventoryChange : IDisposable
    {
        /// <summary>
        /// Stores item changes.
        /// </summary>
        public readonly TempArray<InventoryItemChange> itemChanges;
        /// <summary>
        /// <see cref="Inventory.slotCount"/> before the changes.
        /// </summary>
        public readonly int slotCountBefore;
        /// <summary>
        /// <see cref="Inventory.slotCount"/> after the changes.
        /// </summary>
        public readonly int slotCountAfter;
        /// <summary>
        /// Determines if <see cref="Inventory.slotCount"/> is changed or not.
        /// </summary>
        public bool isSlotCountChanged => slotCountBefore != slotCountAfter;
        
        public InventoryChange(IList<InventoryItemChange> itemChanges, int slotCountBefore, int slotCountAfter)
        {
            this.itemChanges = new TempArray<InventoryItemChange>(itemChanges);
            this.slotCountBefore = slotCountBefore;
            this.slotCountAfter = slotCountAfter;
        }

        void IDisposable.Dispose()
        {
            Dispose(itemChanges);
        }

        static void Dispose<T>(T item) where T : IDisposable
        {
            item.Dispose();
        }
    }
}