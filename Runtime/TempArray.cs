using System;
using System.Buffers;
using System.Collections.Generic;

namespace XIV.Packages.InventorySystem
{
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
            ArrayPool<T>.Shared.Return(items);
        }
    }
}