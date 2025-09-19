using System;
using System.Collections.Generic;

namespace XIV.Packages.InventorySystem
{
    public class Inventory
    {
        /// <summary>
        /// Gets the number of slots available in the <see cref="Inventory"/>
        /// </summary>
        public int slotCount { get; private set; }
        public ReadOnlyInventoryItem this[int index] => index < 0 || index >= slotCount ? 
            ReadOnlyInventoryItem.InvalidReadonlyInventoryItem : new ReadOnlyInventoryItem(items[index]);

        /// <summary>
        /// Defines should the listeners be informed about the changes or not.
        /// Inventory will still record the changes when flag is disabled.
        /// And when you enable the flag, it will inform listeners about previous changes too.
        /// If you want to avoid that, call <see cref="ClearRecordedChanges"/> BEFORE enabling the flag.
        /// </summary>
        public bool informListeners
        {
            get => informListenersState;
            set
            {
                informListenersState = value;
                InformListeners();
            }
        }

        readonly List<IInventoryListener> listeners;
        readonly List<InventoryItemChange> itemChanges;
        
        InventoryItem[] items;
        int arrayCapacity => items.Length;
        int previousSlotCount;
        bool informListenersState;

        public Inventory(int slotCount = 8)
        {
            this.slotCount = slotCount;
            this.previousSlotCount = slotCount;
            int pow2 = slotCount < 2 ? 2 : InventoryHelper.NextPowerOfTwo(slotCount);
            informListenersState = true;
            listeners = new List<IInventoryListener>(4);
            itemChanges = new List<InventoryItemChange>(pow2);
            items = new InventoryItem[pow2];
            InitializeArrayValues(0, slotCount - 1);
        }

        public void Register(IInventoryListener listener)
        {
            if (listeners.Contains(listener)) return;
            listeners.Add(listener);
        }

        public void Unregister(IInventoryListener listener)
        {
            listeners.Remove(listener);
        }

        /// <summary>
        /// Clears the recorded changes
        /// </summary>
        public void ClearRecordedChanges() => itemChanges.Clear();

        /// <summary>
        /// Returns <see langword="true"/> if <see cref="Inventory"/> contains the item
        /// </summary>
        public bool Contains(ItemBase item)
        {
            for (var i = 0; i < slotCount; i++)
            {
                if (InventoryHelper.IsEqual(items[i], item) == false) continue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds as much as possible item at giving <paramref name="quantity"/>
        /// </summary>
        /// <returns>The quantity that can't be added</returns>
        public int Add(ItemBase item, int quantity)
        {
            for (var i = 0; i < slotCount && quantity > 0; i++)
            {
                if (InventoryHelper.IsEqual(items[i], item) == false) continue;
                AddExisting_Internal(i, ref quantity);
            }
            if (quantity > 0) AddNew_Internal(item, ref quantity);
            InformListeners();
            return quantity;
        }

        /// <summary>
        /// Removes as much as possible item at giving <paramref name="quantity"/>
        /// </summary>
        /// <returns>The quantity that can't be removed</returns>
        public int Remove(ItemBase item, int quantity)
        {
            for (var i = 0; i < slotCount && quantity > 0; i++)
            {
                if (InventoryHelper.IsEqual(items[i], item) == false) continue;
                RemoveAt_Internal(i, ref quantity);
            }

            InformListeners();
            return quantity;
        }

        /// <summary>
        /// Clears all slots
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < slotCount; i++)
            {
                int quantity = int.MaxValue;
                RemoveAt_Internal(i, ref quantity);
            }

            InformListeners();
        }

        /// <summary>
        /// Adds new slots for items
        /// </summary>
        /// <param name="count">How many slots will be added</param>
        public void AddSlot(int count)
        {
            if (count <= 0) return;

            var newSlotCount = slotCount + count;
            var capacity = arrayCapacity;
            if (capacity < newSlotCount)
            {
                while (capacity < newSlotCount)
                {
                    capacity *= 2;
                    Array.Resize(ref items, capacity);
                }
                InitializeArrayValues(slotCount - 1, newSlotCount - 1);
            }

            ChangeSlotCount(newSlotCount);
            InformListeners();
        }

        /// <summary>
        /// Removes slots
        /// </summary>
        /// <param name="count">How many slots will be removed</param>
        public void RemoveSlot(int count)
        {
            if (count <= 0) return;
            int newSlotCount = slotCount - count;
            if (newSlotCount > 0)
            {
                RemoveSlot_Internal(count);
                // This will only inform them about the item changes
                InformListeners();
                return;
            }

            // No need to move items
            Clear();
            ChangeSlotCount(0);
            InformListeners();
        }

        /// <summary>
        /// Swaps the item indices. No matter if indices are empty or not.
        /// </summary>
        /// <seealso cref="Merge"/>
        public void Swap(int index1, int index2)
        {
            if (index1 == index2) return;
            Swap_Internal(index1, index2);
            InformListeners();
        }

        /// <summary>
        /// Merges the items at <paramref name="index1"/> and <paramref name="index2"/> if both items are same.
        /// Doesn't do anything if one of the indices are empty.
        /// </summary>
        /// <returns>The remaining quantity at <paramref name="index1"/> if merged. -1 otherwise</returns>
        /// <seealso cref="Swap"/>
        public int Merge(int index1, int index2)
        {
            if (CanMerge(index1, index2) == false) return -1;
            int remainingAtIndex1 = Merge_Internal(index1, index2);
            InformListeners();
            return remainingAtIndex1;
        }

        /// <summary>
        /// This will compare items at <paramref name="index1"/> and <paramref name="index2"/> and
        /// will return true if they are same.
        /// </summary>
        public bool CanMerge(int index1, int index2)
        {
            if (index1 == index2) return false;
            ref InventoryItem item1 = ref items[index1];
            ref InventoryItem item2 = ref items[index2];
            return item1.IsEmpty == false && item2.IsEmpty == false && item1.Item.Equals(item2.Item);
        }

        void Swap_Internal(int index1, int index2)
        {
            var snapshot1 = GetSnapshot(index1);
            var snapshot2 = GetSnapshot(index2);
            
            ref InventoryItem item1 = ref items[index1];
            ref InventoryItem item2 = ref items[index2];

            InventoryItem temp = item2;
            item2.Quantity = item1.Quantity;
            item2.Item = item1.Item;
            item1.Quantity = temp.Quantity;
            item1.Item = temp.Item;
            
            SaveChange(snapshot1, index1, false);
            SaveChange(snapshot2, index2, false);
        }

        int Merge_Internal(int index1, int index2)
        {
            int item1Quantity = items[index1].Quantity;
            int addAmount = item1Quantity;
            AddExisting_Internal(index2, ref addAmount);
            int removeAmount = item1Quantity - addAmount;
            RemoveAt_Internal(index1, ref removeAmount);
            // The remaining quantity at index1
            return addAmount;
        }

        void RemoveSlot_Internal(int newSlotCount)
        {
            // Distributes the item at itemIndex to the rest of items by starting from the last
            // returns the remaining quantity at itemIndex
            int DistributeItem(int itemIndex, int slotCount)
            {
                int remaining = int.MaxValue;
                for (int j = slotCount - 1; j >= 0 && remaining > 0; j--)
                {
                    if (CanMerge(itemIndex, j))
                    {
                        remaining = Merge_Internal(itemIndex, j);
                    }
                }

                return remaining;
            }

            int FindEmptyIndexFromLast(int slotCount)
            {
                for (int i = slotCount - 1; i >= 0; i--)
                {
                    if (items[i].IsEmpty) return i;
                }
                return -1;
            }

            for (int i = slotCount - 1; i >= newSlotCount; i--)
            {
                // if we completely distributed the item no need to swap or anything
                var snapshot = GetSnapshot(i);
                if (DistributeItem(i, newSlotCount) == 0) continue;

                int emptyIndexFromLast = FindEmptyIndexFromLast(newSlotCount);
                if (emptyIndexFromLast != -1)
                {
                    Swap_Internal(i, emptyIndexFromLast);
                    continue;
                }

                SaveChange(snapshot, i, true);
            }

            ChangeSlotCount(newSlotCount);
        }

        void AddExisting_Internal(int index, ref int amount)
        {
            ref InventoryItem inventoryItem = ref items[index];
            int stackLeft = inventoryItem.Item.stackableQuantity - inventoryItem.Quantity;
            if (stackLeft == 0) return;

            var snapshot = GetSnapshot(index);
            
            int addAmount = Math.Min(stackLeft, amount);
            inventoryItem.Quantity += addAmount;
            amount -= addAmount;
            
            SaveChange(snapshot, index, false);
        }

        void AddNew_Internal(ItemBase item, ref int amount)
        {
            for (int i = 0; i < slotCount && amount > 0; i++)
            {
                ref InventoryItem inventoryItem = ref items[i];
                if (inventoryItem.IsEmpty == false) continue;

                var snapshot = GetSnapshot(i);
                
                int addAmount = Math.Min(item.stackableQuantity, amount);
                inventoryItem.Quantity = addAmount;
                inventoryItem.Item = item;
                amount -= addAmount;
                
                SaveChange(snapshot, i, false);
            }
        }

        void RemoveAt_Internal(int index, ref int quantity)
        {
            var snapshot = GetSnapshot(index);
            
            ref InventoryItem inventoryItem = ref items[index];
            var removeQuantity = Math.Min(inventoryItem.Quantity, quantity);
            inventoryItem.Quantity -= removeQuantity;
            quantity -= removeQuantity;
            
            SaveChange(snapshot, index, false);
        }

        void InformListeners()
        {
            if (informListenersState == false || (itemChanges.Count == 0 && previousSlotCount == slotCount)) return;

            int count = listeners.Count;
            var inventoryChange = new InventoryChange(itemChanges, previousSlotCount, slotCount);
            for (int i = 0; i < count; i++)
            {
                listeners[i].OnInventoryChanged(inventoryChange);
            }
            InventoryHelper.Dispose(ref inventoryChange);
            itemChanges.Clear();
        }

        void InitializeArrayValues(int startIndex, int endIndex)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                items[i] = new InventoryItem(i, 0, null);
            }
        }

        ReadOnlyInventoryItem GetSnapshot(int index) => new ReadOnlyInventoryItem(items[index]);
        void SaveChange(ReadOnlyInventoryItem before, int currentIndex, bool isDiscarded)
        {
            itemChanges.Add(new InventoryItemChange(before, GetSnapshot(currentIndex), isDiscarded));
        }

        void ChangeSlotCount(int newCount)
        {
            previousSlotCount = slotCount;
            slotCount = newCount;
        }
    }

    public static class InventoryHelper
    {
        public static int NextPowerOfTwo(int v)
        {
            // https://stackoverflow.com/a/466242/15200285
            // https://graphics.stanford.edu/%7Eseander/bithacks.html#RoundUpPowerOf2
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

        /// <summary>
        /// Check if <paramref name="inventoryItem"/> contains the <paramref name="item"/>
        /// </summary>
        public static bool IsEqual(InventoryItem inventoryItem, ItemBase item)
        {
            return inventoryItem.IsEmpty == false && inventoryItem.Item.Equals(item);
        }

        /// <summary>
        /// This will avoid boxing of structs
        /// </summary>
        public static void Dispose<T>(ref T disposable) where T : IDisposable
        {
            disposable.Dispose();
        }
    }
}