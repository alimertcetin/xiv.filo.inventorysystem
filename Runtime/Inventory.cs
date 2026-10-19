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
        public ReadOnlyInventorySlot this[int index] => index < 0 || index >= slotCount ? 
            ReadOnlyInventorySlot.InvalidReadonlyInventorySlot : new ReadOnlyInventorySlot(slots[index]);

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
                if (informListenersState == value) return;
                informListenersState = value;
                if (value) InformListeners();
            }
        }

        readonly List<IInventoryListener> listeners;
        readonly List<InventorySlotChange> slotChanges;
        
        InventorySlot[] slots;
        int arrayCapacity => slots.Length;
        int previousSlotCount;
        bool informListenersState;

        public Inventory(int slotCount = 8)
        {
            this.slotCount = slotCount;
            this.previousSlotCount = slotCount;
            int pow2 = slotCount < 2 ? 2 : InventoryHelper.NextPowerOfTwo(slotCount);
            informListenersState = true;
            listeners = new List<IInventoryListener>(4);
            slotChanges = new List<InventorySlotChange>(pow2);
            slots = new InventorySlot[pow2];
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
        public void ClearRecordedChanges() => slotChanges.Clear();

        /// <summary>
        /// Returns <see langword="true"/> if <see cref="Inventory"/> contains the item
        /// </summary>
        public bool Contains(ItemBase item)
        {
            for (var i = 0; i < slotCount; i++)
            {
                if (InventoryHelper.IsEqual(slots[i], item) == false) continue;
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
                if (InventoryHelper.IsEqual(slots[i], item) == false) continue;
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
                if (InventoryHelper.IsEqual(slots[i], item) == false) continue;
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
                    Array.Resize(ref slots, capacity);
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
            ref InventorySlot slot1 = ref slots[index1];
            ref InventorySlot slot2 = ref slots[index2];
            return slot1.IsEmpty == false && slot2.IsEmpty == false && slot1.Item.Equals(slot2.Item);
        }
        
        /// <summary>
        /// Creates a snapshot of the current <see cref="Inventory"/> state.
        /// The snapshot stores the slot count and all non-empty slots while preserving their indices.
        /// </summary>
        /// <returns>A snapshot representing the current state of the <see cref="Inventory"/>.</returns>
        public InventorySnapshot GetSnapshot()
        {
            int nonEmptySlotCount = 0;
            for (int i = 0; i < slotCount; i++)
            {
                nonEmptySlotCount = slots[i].IsEmpty ? nonEmptySlotCount : nonEmptySlotCount + 1;
            }
            var snapshotItems = new InventorySlot[nonEmptySlotCount];
            for (int i = 0, j = 0; i < slotCount; i++)
            {
                if (slots[i].IsEmpty == false)
                {
                    snapshotItems[j++] = slots[i];
                }
            }
            return new InventorySnapshot(slotCount, snapshotItems);
        }

        /// <summary>
        /// Restores the <see cref="Inventory"/> state from the specified <see cref="InventorySnapshot"/>.
        /// </summary>
        /// <param name="snapshot">The snapshot used to restore the <see cref="Inventory"/>.</param>
        public void RestoreSnapshot(InventorySnapshot snapshot)
        {
            RemoveSlot_Internal(0);
            AddSlot(snapshot.slotCount);
            for (int i = 0; i < snapshot.occupiedSlots.Length; i++)
            {
                var slotSnapshot = GetSlotSnapshot(snapshot.occupiedSlots[i].Index);
                slots[snapshot.occupiedSlots[i].Index] = snapshot.occupiedSlots[i];
                SaveChange(slotSnapshot, snapshot.occupiedSlots[i].Index, false);
            }
            InformListeners();
        }

        void Swap_Internal(int index1, int index2)
        {
            var snapshot1 = GetSlotSnapshot(index1);
            var snapshot2 = GetSlotSnapshot(index2);
            
            ref InventorySlot slot1 = ref slots[index1];
            ref InventorySlot slot2 = ref slots[index2];

            InventorySlot temp = slot2;
            slot2.Quantity = slot1.Quantity;
            slot2.Item = slot1.Item;
            slot1.Quantity = temp.Quantity;
            slot1.Item = temp.Item;
            
            SaveChange(snapshot1, index1, false);
            SaveChange(snapshot2, index2, false);
        }
        
        int Merge_Internal(int idxSource, int idxDestination)
        {
            ref InventorySlot source = ref slots[idxSource];
            ref InventorySlot destination = ref slots[idxDestination];

            int sourceQuantity = source.Quantity;
            int destinationSpace = destination.Item.stackableQuantity - destination.Quantity;
            int transferAmount = Math.Min(sourceQuantity, destinationSpace);
            if (transferAmount <= 0) return sourceQuantity;

            var sourceSnapshot = GetSlotSnapshot(idxSource);
            var destinationSnapshot = GetSlotSnapshot(idxDestination);
            source.Quantity -= transferAmount;
            destination.Quantity += transferAmount;

            if (source.Quantity == 0) source.Item = null;

            SaveChange(destinationSnapshot, idxSource, idxDestination, idxDestination, false);
            SaveChange(sourceSnapshot, idxSource, idxDestination, idxSource, false);

            // The remaining quantity at index1
            return source.Quantity;
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
                    if (slots[i].IsEmpty) return i;
                }
                return -1;
            }

            for (int i = slotCount - 1; i >= newSlotCount; i--)
            {
                // if we completely distributed the item no need to swap or anything
                var snapshot = GetSlotSnapshot(i);
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
            ref InventorySlot inventorySlot = ref slots[index];
            int stackLeft = inventorySlot.Item.stackableQuantity - inventorySlot.Quantity;
            if (stackLeft == 0) return;

            var snapshot = GetSlotSnapshot(index);
            
            int addAmount = Math.Min(stackLeft, amount);
            inventorySlot.Quantity += addAmount;
            amount -= addAmount;
            
            SaveChange(snapshot, index, false);
        }

        void AddNew_Internal(ItemBase item, ref int amount)
        {
            for (int i = 0; i < slotCount && amount > 0; i++)
            {
                ref InventorySlot inventorySlot = ref slots[i];
                if (inventorySlot.IsEmpty == false) continue;

                var snapshot = GetSlotSnapshot(i);
                
                int addAmount = Math.Min(item.stackableQuantity, amount);
                inventorySlot.Quantity = addAmount;
                inventorySlot.Item = item;
                amount -= addAmount;
                
                SaveChange(snapshot, i, false);
            }
        }

        void RemoveAt_Internal(int index, ref int quantity)
        {
            var snapshot = GetSlotSnapshot(index);
            
            ref InventorySlot inventorySlot = ref slots[index];
            var removeQuantity = Math.Min(inventorySlot.Quantity, quantity);
            inventorySlot.Quantity -= removeQuantity;
            quantity -= removeQuantity;

            if (inventorySlot.Quantity == 0) inventorySlot.Item = null;
            
            SaveChange(snapshot, index, false);
        }

        void InformListeners()
        {
            if (informListenersState == false || (slotChanges.Count == 0 && previousSlotCount == slotCount)) return;

            int count = listeners.Count;
            var inventoryChange = new InventoryChange(slotChanges, previousSlotCount, slotCount);
            for (int i = 0; i < count; i++)
            {
                listeners[i].OnInventoryChanged(inventoryChange);
            }
            previousSlotCount = slotCount;
            InventoryHelper.Dispose(ref inventoryChange);
            slotChanges.Clear();
        }

        void InitializeArrayValues(int startIndex, int endIndex)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                slots[i] = new InventorySlot(i, 0, null);
            }
        }

        ReadOnlyInventorySlot GetSlotSnapshot(int index) => new ReadOnlyInventorySlot(slots[index]);
        
        void SaveChange(ReadOnlyInventorySlot before, int currentIndex, bool isDiscarded)
        {
            slotChanges.Add(new InventorySlotChange(before, GetSlotSnapshot(currentIndex), default, isDiscarded));
        }
        
        void SaveChange(ReadOnlyInventorySlot before, int mergeSource, int mergeDestination, int currentIndex, bool isDiscarded)
        {
            var otherIndex = currentIndex == mergeSource ? mergeDestination : mergeSource;
            var mergeState = mergeSource == currentIndex && mergeDestination != currentIndex ? ItemMergeState.ConsumedByMerge : ItemMergeState.ReceivedMerge;
            var itemMergeData = new ItemMergeData(mergeState, otherIndex);
            slotChanges.Add(new InventorySlotChange(before, GetSlotSnapshot(currentIndex), itemMergeData, isDiscarded));
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
        /// Check if <paramref name="inventorySlot"/> contains the <paramref name="item"/>
        /// </summary>
        public static bool IsEqual(InventorySlot inventorySlot, ItemBase item)
        {
            return inventorySlot.IsEmpty == false && inventorySlot.Item.Equals(item);
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