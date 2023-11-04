using System;
using System.Buffers;
using System.Collections.Generic;

namespace XIV_Packages.InventorySystem
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
    
    public readonly struct TempArray<T> : IDisposable
    {
        public T this[int index] => items[index];
        public readonly int Length;
        
        readonly T[] items;

        public TempArray(IList<T> items)
        {
            this.Length = items.Count;
            this.items = ArrayPool<T>.Shared.Rent(Length);
            for (int i = 0; i < Length; i++)
            {
                this.items[i] = items[i];
            }
        }

        void IDisposable.Dispose()
        {
            if (Length == 0) return;
            ArrayPool<T>.Shared.Return(items);
        }
    }
}