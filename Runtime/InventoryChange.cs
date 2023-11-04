using System;
using System.Collections.Generic;

namespace XIV.Packages.InventorySystem
{
    public readonly struct InventoryChange : IDisposable
    {
        public readonly TempArray<InventoryItemChange> itemChanges;
        /// <summary>
        /// Before the changes
        /// </summary>
        public readonly int slotCountBefore;
        /// <summary>
        /// After the changes
        /// </summary>
        public readonly int slotCountAfter;
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