using System;
using System.Buffers;

namespace XIV_Packages.InventorySystem.Extensions
{
    /// <summary>
    /// Contains extension methods for <see cref="Inventory"/>.
    /// </summary>
    public static class InventoryExtensions
    {
        /// <summary>
        /// Moves the <paramref name="index1"/> over <paramref name="index2"/>.
        /// Merges as much as possible if items are same.
        /// Swaps them if they are different.
        /// <seealso cref="Inventory.Merge"/>
        /// <seealso cref="Inventory.Swap"/>
        /// </summary>
        public static void Move(this Inventory inventory, int index1, int index2)
        {
            if (inventory.CanMerge(index1, index2)) inventory.Merge(index1, index2);
            else inventory.Swap(index1, index2);
        }

        /// <summary>
        /// Returns true if we can add all giving <paramref name="quantity"/>
        /// </summary>
        public static bool CanAddItem<T>(this Inventory inventory, int stackableAmount, int quantity) where T : ItemBase
        {
            return CalculateRemainingAfterAdd<T>(inventory, stackableAmount, quantity) == 0;
        }
        
        /// <summary>
        /// Returns true if we can add all giving <paramref name="quantity"/>
        /// </summary>
        public static bool CanAddItem(this Inventory inventory, ItemBase item, int quantity)
        {
            return CalculateRemainingAfterAdd(inventory, item, quantity) == 0;
        }
        
        /// <summary>
        /// Returns true if <typeparamref name="T"/> can be removed from <paramref name="inventory"/> completely
        /// </summary>
        public static bool CanRemoveItem<T>(this Inventory inventory, int quantity) where T : ItemBase
        {
            return CalculateRemainingAfterRemove<T>(inventory, quantity) == 0;
        }
        
        /// <summary>
        /// Returns true if <paramref name="item"/> can be removed from <paramref name="inventory"/> completely
        /// </summary>
        public static bool CanRemoveItem(this Inventory inventory, ItemBase item, int quantity)
        {
            return CalculateRemainingAfterRemove(inventory, item, quantity) == 0;
        }

        /// <summary>
        /// Returns the quantity that will remain after <see cref="Inventory.Add"/>
        /// </summary>
        public static int CalculateRemainingAfterAdd<T>(this Inventory inventory, int stackableAmount, int quantity) where T : ItemBase
        {
            var readonlyInventoryItemBuffer = ArrayPool<ReadOnlyInventoryItem>.Shared.Rent(inventory.slotCount);
            int length = GetItemsOfTypeNonAlloc<T>(inventory, readonlyInventoryItemBuffer);
            quantity = CalculateRemainingAfterAdd(inventory, stackableAmount, quantity, readonlyInventoryItemBuffer, length);
            ArrayPool<ReadOnlyInventoryItem>.Shared.Return(readonlyInventoryItemBuffer);
            return quantity;
        }
        
        /// <summary>
        /// Returns the quantity that will remain after <see cref="Inventory.Add"/>
        /// </summary>
        public static int CalculateRemainingAfterAdd(this Inventory inventory, ItemBase item, int quantity)
        {
            var readonlyInventoryItemBuffer = ArrayPool<ReadOnlyInventoryItem>.Shared.Rent(inventory.slotCount);
            int length = GetItemsOfTypeNonAlloc(inventory, item, readonlyInventoryItemBuffer);
            quantity = CalculateRemainingAfterAdd(inventory, item.stackableQuantity, quantity, readonlyInventoryItemBuffer, length);
            ArrayPool<ReadOnlyInventoryItem>.Shared.Return(readonlyInventoryItemBuffer);
            return quantity;
        }
        
        /// <summary>
        /// Returns the quantity that will remain after <see cref="Inventory.Remove"/>
        /// </summary>
        public static int CalculateRemainingAfterRemove<T>(this Inventory inventory, int quantity) where T : ItemBase
        {
            var readonlyInventoryItemBuffer = ArrayPool<ReadOnlyInventoryItem>.Shared.Rent(inventory.slotCount);
            int length = GetItemsOfTypeNonAlloc<T>(inventory, readonlyInventoryItemBuffer);
            var remaining = CalculateRemainingAfterRemove(quantity, readonlyInventoryItemBuffer, length);
            ArrayPool<ReadOnlyInventoryItem>.Shared.Return(readonlyInventoryItemBuffer);
            return remaining;
        }
        
        /// <summary>
        /// Returns the quantity that will remain after <see cref="Inventory.Remove"/>
        /// </summary>
        public static int CalculateRemainingAfterRemove(this Inventory inventory, ItemBase item, int quantity)
        {
            var readonlyInventoryItemBuffer = ArrayPool<ReadOnlyInventoryItem>.Shared.Rent(inventory.slotCount);
            int length = GetItemsOfTypeNonAlloc(inventory, item, readonlyInventoryItemBuffer);
            var remaining = CalculateRemainingAfterRemove(quantity, readonlyInventoryItemBuffer, length);
            ArrayPool<ReadOnlyInventoryItem>.Shared.Return(readonlyInventoryItemBuffer);
            return remaining;
        }

        public static int GetOccupiedIndices(this Inventory inventory, int[] indicesBuffer)
        {
            return GetIndices(inventory, indicesBuffer, false);
        }

        public static int GetEmptyIndices(this Inventory inventory, int[] indicesBuffer)
        {
            return GetIndices(inventory, indicesBuffer, true);
        }

        public static int GetOccupiedSlotCount(this Inventory inventory)
        {
            var buffer = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            int count = GetOccupiedIndices(inventory, buffer);
            ArrayPool<int>.Shared.Return(buffer);
            return count;
        }

        public static int GetEmptySlotCount(this Inventory inventory)
        {
            var buffer = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            int count = GetEmptyIndices(inventory, buffer);
            ArrayPool<int>.Shared.Return(buffer);
            return count;
        }

        public static ReadOnlyInventoryItem FirstOrDefault<T>(this Inventory inventory) where T : ItemBase
        {
            var buffer = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            if (GetIndicesOf<T>(inventory, buffer) == 0)
            {
                ArrayPool<int>.Shared.Return(buffer);
                return default;
            }
            var readOnlyInventoryItem = inventory[buffer[0]];
            ArrayPool<int>.Shared.Return(buffer);
            return readOnlyInventoryItem;
        }

        public static ReadOnlyInventoryItem FirstOrDefault(this Inventory inventory, ItemBase item)
        {
            var buffer = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            if (GetIndicesOf(inventory, item, buffer) == 0)
            {
                ArrayPool<int>.Shared.Return(buffer);
                return default;
            }
            var readOnlyInventoryItem = inventory[buffer[0]];
            ArrayPool<int>.Shared.Return(buffer);
            return readOnlyInventoryItem;
        }
        
        /// <summary>
        /// Returns a <see cref="ReadOnlyInventoryItem"/> array
        /// while making sure all <see cref="ReadOnlyInventoryItem.Item"/> is type of <typeparamref name="T"/>
        /// </summary>
        public static ReadOnlyInventoryItem[] GetItemsOfType<T>(this Inventory inventory) where T : ItemBase
        {
            var indicesBuffer = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            int length = GetIndicesOf<T>(inventory, indicesBuffer);
            var itemsArr = new ReadOnlyInventoryItem[length];
            Fill(inventory, itemsArr, length, indicesBuffer, length);
            ArrayPool<int>.Shared.Return(indicesBuffer);
            return itemsArr;
        }
        
        /// <summary>
        /// Returns a <see cref="ReadOnlyInventoryItem"/> array
        /// while making sure all <see cref="ReadOnlyInventoryItem.Item"/> is type of <typeparamref name="T"/>
        /// </summary>
        public static ReadOnlyInventoryItem[] GetItemsOfType(this Inventory inventory, ItemBase item)
        {
            var indicesBuffer = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            int length = GetIndicesOf(inventory, item, indicesBuffer);
            var itemsArr = new ReadOnlyInventoryItem[length];
            Fill(inventory, itemsArr, length, indicesBuffer, length);
            ArrayPool<int>.Shared.Return(indicesBuffer);
            return itemsArr;
        }
        
        /// <summary>
        /// Returns a <see cref="ReadOnlyInventoryItem"/> array
        /// while making sure all <see cref="ReadOnlyInventoryItem.Item"/> is type of <typeparamref name="T"/>
        /// and they all match with <paramref name="match"/>
        /// </summary>
        public static ReadOnlyInventoryItem[] GetItemsOfType<T>(this Inventory inventory, Predicate<T> match) where T : ItemBase
        {
            var indicesBuffer = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            int length = GetIndicesOf<T>(inventory, indicesBuffer, match);
            var itemsArr = new ReadOnlyInventoryItem[length];
            Fill(inventory, itemsArr, length, indicesBuffer, length);
            ArrayPool<int>.Shared.Return(indicesBuffer);
            return itemsArr;
        }
        
        public static ReadOnlyInventoryItem[] GetItemsOfType(this Inventory inventory, ItemBase item, Predicate<ItemBase> match)
        {
            var indicesBuffer = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            int length = GetIndicesOf(inventory, item, indicesBuffer, match);
            var itemsArr = new ReadOnlyInventoryItem[length];
            Fill(inventory, itemsArr, length, indicesBuffer, length);
            ArrayPool<int>.Shared.Return(indicesBuffer);
            return itemsArr;
        }
        
        /// <summary>
        /// Fills the <paramref name="inventoryItemBuffer"/> and returns the filled count
        /// while making sure all <see cref="ReadOnlyInventoryItem.Item"/> is type of <typeparamref name="T"/>
        /// </summary>
        public static int GetItemsOfTypeNonAlloc<T>(this Inventory inventory, ReadOnlyInventoryItem[] inventoryItemBuffer) where T : ItemBase
        {
            int[] indicesBuffer = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            int indicesBufferLength = GetIndicesOf<T>(inventory, indicesBuffer);
            int count = Fill(inventory, inventoryItemBuffer, inventoryItemBuffer.Length, indicesBuffer, indicesBufferLength);
            ArrayPool<int>.Shared.Return(indicesBuffer);
            return count;
        }
        
        public static int GetItemsOfTypeNonAlloc(this Inventory inventory, ItemBase item, ReadOnlyInventoryItem[] inventoryItemBuffer)
        {
            int[] indicesBuffer = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            int indicesBufferLength = GetIndicesOf(inventory, item, indicesBuffer);
            int count = Fill(inventory, inventoryItemBuffer, inventoryItemBuffer.Length, indicesBuffer, indicesBufferLength);
            ArrayPool<int>.Shared.Return(indicesBuffer);
            return count;
        }
        
        /// <summary>
        /// Fills the <paramref name="inventoryItemBuffer"/> and returns the filled count
        /// while making sure all <see cref="ReadOnlyInventoryItem.Item"/> is type of <typeparamref name="T"/>
        /// and they all match with <paramref name="match"/>
        /// </summary>
        public static int GetItemsOfTypeNonAlloc<T>(this Inventory inventory, ReadOnlyInventoryItem[] inventoryItemBuffer, Predicate<T> match) where T : ItemBase
        {
            int[] indicesBuffer = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            int indicesBufferLength = GetIndicesOf<T>(inventory, indicesBuffer, match);
            int count = Fill(inventory, inventoryItemBuffer, inventoryItemBuffer.Length, indicesBuffer, indicesBufferLength);
            ArrayPool<int>.Shared.Return(indicesBuffer);
            return count;
        }
        
        public static int GetItemsOfTypeNonAlloc(this Inventory inventory, ItemBase item, ReadOnlyInventoryItem[] inventoryItemBuffer, Predicate<ItemBase> match)
        {
            int[] indicesBuffer = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            int indicesBufferLength = GetIndicesOf(inventory, item, indicesBuffer, match);
            int count = Fill(inventory, inventoryItemBuffer, inventoryItemBuffer.Length, indicesBuffer, indicesBufferLength);
            ArrayPool<int>.Shared.Return(indicesBuffer);
            return count;
        }
        
        /// <summary>
        /// Returns how many slots are occupied by type <typeparamref name="T"/>
        /// </summary>
        public static int GetCountOf<T>(this Inventory inventory) where T : ItemBase
        {
            var indicesBuffer = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            int length = GetIndicesOf<T>(inventory, indicesBuffer);
            ArrayPool<int>.Shared.Return(indicesBuffer);
            return length;
        }
        
        /// <summary>
        /// Returns how many slots are occupied by the type of <paramref name="item"/>
        /// </summary>
        public static int GetCountOf(this Inventory inventory, ItemBase item)
        {
            var indicesBuffer = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            int length = GetIndicesOf(inventory, item, indicesBuffer);
            ArrayPool<int>.Shared.Return(indicesBuffer);
            return length;
        }

        /// <summary>
        /// Returns the amount of items that matches with <paramref name="match"/>
        /// and also makes sure they are of type <typeparamref name="T"/>
        /// </summary>
        public static int GetCountOf<T>(this Inventory inventory, Predicate<T> match) where T : ItemBase
        {
            var indicesBuffer = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            int length = GetIndicesOf<T>(inventory, indicesBuffer, match);
            ArrayPool<int>.Shared.Return(indicesBuffer);
            return length;
        }
        
        /// <summary>
        /// Returns the amount of items that matches with <paramref name="match"/>
        /// and also makes sure they are the same type as <paramref name="item"/>
        /// </summary>
        public static int GetCountOf(this Inventory inventory, ItemBase item, Predicate<ItemBase> match)
        {
            var indicesBuffer = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            int length = GetIndicesOf(inventory, item, indicesBuffer, match);
            ArrayPool<int>.Shared.Return(indicesBuffer);
            return length;
        }
        
        /// <summary>
        /// Returns the total quantity of type <typeparamref name="T"/>
        /// </summary>
        public static int GetQuantityOf<T>(this Inventory inventory) where T : ItemBase
        {
            var indicesBuffer = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            int length = GetIndicesOf<T>(inventory, indicesBuffer);
            int quantity = GetQuantity(inventory, indicesBuffer, length);
            ArrayPool<int>.Shared.Return(indicesBuffer);
            return quantity;
        }
        
        public static int GetQuantityOf(this Inventory inventory, ItemBase item)
        {
            var indicesBuffer = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            int length = GetIndicesOf(inventory, item, indicesBuffer);
            int quantity = GetQuantity(inventory, indicesBuffer, length);
            ArrayPool<int>.Shared.Return(indicesBuffer);
            return quantity;
        }

        /// <summary>
        /// Returns the item that has minimum quantity
        /// </summary>
        public static ReadOnlyInventoryItem GetItemHasMinQuantity<T>(this Inventory inventory) where T : ItemBase
        {
            return GetReadOnlyItemMinMax<T>(inventory, false);
        }
        
        /// <summary>
        /// Returns the item that has minimum quantity
        /// </summary>
        public static ReadOnlyInventoryItem GetItemHasMinQuantity(this Inventory inventory, ItemBase item)
        {
            return GetReadOnlyItemMinMax(inventory, item, false);
        }

        /// <summary>
        /// Returns the item that has maximum quantity
        /// </summary>
        public static ReadOnlyInventoryItem GetItemHasMaxQuantity<T>(this Inventory inventory) where T : ItemBase
        {
            return GetReadOnlyItemMinMax<T>(inventory, true);
        }

        /// <summary>
        /// Returns the item that has maximum quantity
        /// </summary>
        public static ReadOnlyInventoryItem GetItemHasMaxQuantity(this Inventory inventory, ItemBase item)
        {
            return GetReadOnlyItemMinMax(inventory, item, true);
        }
        
        /// <summary>
        /// Fills the <paramref name="indicesBuffer"/> with the corresponding indices of <typeparamref name="T"/>
        /// </summary>
        public static int GetIndicesOf<T>(this Inventory inventory, int[] indicesBuffer) where T : ItemBase
        {
            int length = indicesBuffer.Length;
            int count = 0;
            int slotCount = inventory.slotCount;
            for (int i = 0; i < slotCount && count < length; i++)
            {
                var inventoryItem = inventory[i];
                if (inventoryItem.IsEmpty == false && inventoryItem.Item is T)
                {
                    indicesBuffer[count++] = i;
                }
            }

            return count;
        }
        
        public static int GetIndicesOf(this Inventory inventory, Type itemType, int[] indicesBuffer)
        {
            int length = indicesBuffer.Length;
            int count = 0;
            int slotCount = inventory.slotCount;
            for (int i = 0; i < slotCount && count < length; i++)
            {
                var inventoryItem = inventory[i];
                if (inventoryItem.IsEmpty == false && inventoryItem.Item.GetType() == itemType)
                {
                    indicesBuffer[count++] = i;
                }
            }

            return count;
        }
        
        public static int GetIndicesOf(this Inventory inventory, ItemBase item, int[] indicesBuffer)
        {
            int length = indicesBuffer.Length;
            int count = 0;
            int slotCount = inventory.slotCount;
            for (int i = 0; i < slotCount && count < length; i++)
            {
                var inventoryItem = inventory[i];
                if (inventoryItem.IsEmpty == false && inventoryItem.Item.Equals(item))
                {
                    indicesBuffer[count++] = i;
                }
            }

            return count;
        }
        
        /// <summary>
        /// Fills the <paramref name="indicesBuffer"/> with the corresponding indices of <typeparamref name="T"/>
        /// and matches with <paramref name="match"/>
        /// </summary>
        public static int GetIndicesOf<T>(this Inventory inventory, int[] indicesBuffer, Predicate<T> match) where T : ItemBase
        {
            var tempIndices = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            int tempIndicesLength = GetIndicesOf<T>(inventory, tempIndices);
            int count = FilterMatch<T>(inventory, indicesBuffer, tempIndices, tempIndicesLength, match);
            ArrayPool<int>.Shared.Return(tempIndices);
            return count;
        }

        public static int GetIndicesOf(this Inventory inventory, ItemBase item, int[] indicesBuffer, Predicate<ItemBase> match)
        {
            var tempIndices = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            int length = GetIndicesOf(inventory, item, tempIndices);
            int count = FilterMatch(inventory, indicesBuffer, tempIndices, length, match);
            ArrayPool<int>.Shared.Return(tempIndices);
            return count;
        }

        static int CalculateRemainingAfterRemove(int quantity, ReadOnlyInventoryItem[] items, int itemsLength)
        {
            for (var i = 0; i < itemsLength && quantity > 0; i++)
            {
                quantity -= Math.Min(items[i].Quantity, quantity);
            }
            return quantity;
        }

        static int GetIndices(this Inventory inventory, int[] indicesBuffer, bool searchEmpty)
        {
            int length = indicesBuffer.Length;
            int count = 0;
            int slotCount = inventory.slotCount;
            for (int i = 0; i < slotCount && count < length; i++)
            {
                var inventoryItem = inventory[i];
                if (inventoryItem.IsEmpty == searchEmpty)
                {
                    indicesBuffer[count++] = i;
                }
            }

            return count;
        }

        static ReadOnlyInventoryItem GetReadOnlyItemMinMax<T>(Inventory inventory, bool searchMax) where T : ItemBase
        {
            var indicesBuffer = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            int length = GetIndicesOf<T>(inventory, indicesBuffer);
            ReadOnlyInventoryItem current = GetReadOnlyItemMinMax(inventory, indicesBuffer, length, searchMax);
            ArrayPool<int>.Shared.Return(indicesBuffer);
            return current;
        }

        static ReadOnlyInventoryItem GetReadOnlyItemMinMax(Inventory inventory, ItemBase item, bool searchMax)
        {
            var indicesBuffer = ArrayPool<int>.Shared.Rent(inventory.slotCount);
            int length = GetIndicesOf(inventory, item, indicesBuffer);
            ReadOnlyInventoryItem current = GetReadOnlyItemMinMax(inventory, indicesBuffer, length, searchMax);
            ArrayPool<int>.Shared.Return(indicesBuffer);
            return current;
        }

        static ReadOnlyInventoryItem GetReadOnlyItemMinMax(Inventory inventory, int[] indicesBuffer, int indicesBufferLength, bool searchMax)
        {
            int max = searchMax ? int.MinValue : int.MaxValue;
            ReadOnlyInventoryItem current = default;
            for (var i = 0; i < indicesBufferLength; i++)
            {
                var inventoryItem = inventory[indicesBuffer[i]];
                if ((searchMax && inventoryItem.Quantity >= max) || (!searchMax && inventoryItem.Quantity <= max))
                {
                    max = inventoryItem.Quantity;
                    current = inventoryItem;
                }
            }
            return current;
        }

        static int CalculateRemainingAfterAdd(Inventory inventory, int stackableAmount, int quantity, ReadOnlyInventoryItem[] existingItems, int existingItemsLength)
        {
            // Fill existing item slots
            for (var i = 0; i < existingItemsLength && quantity > 0; i++)
            {
                var inventoryItem = existingItems[i];
                int stackLeft = inventoryItem.Item.stackableQuantity - inventoryItem.Quantity;
                if (stackLeft == 0) continue;
                quantity -= Math.Min(stackLeft, quantity);
            }

            if (quantity <= 0) return quantity;
            
            // Fill new slots while quantity is greater than 0
            var emptySlotCount = GetEmptySlotCount(inventory);
            for (int i = 0; i < emptySlotCount && quantity > 0; i++)
            {
                quantity -= Math.Min(stackableAmount, quantity);
            }
            return quantity;
        }

        static int GetQuantity(Inventory inventory, int[] indicesBuffer, int length)
        {
            int quantity = 0;
            for (int i = 0; i < length; i++)
            {
                quantity += inventory[indicesBuffer[i]].Quantity;
            }

            return quantity;
        }

        static int FilterMatch<T>(Inventory inventory, int[] indicesBuffer, int[] indices, int indicesLength, Predicate<T> match) where T : ItemBase
        {
            int count = 0;
            int indicesBufferLength = indicesBuffer.Length;
            for (int i = 0; i < indicesLength && count < indicesBufferLength; i++)
            {
                var inventoryItem = inventory[indices[i]];
                if (match.Invoke((T)inventoryItem.Item))
                {
                    indicesBuffer[count++] = indices[i];
                }
            }
            return count;
        }

        static int FilterMatch(Inventory inventory, int[] indicesBuffer, int[] indices, int indicesLength, Predicate<ItemBase> match)
        {
            int count = 0;
            int indicesBufferLength = indicesBuffer.Length;
            for (int i = 0; i < indicesLength && count < indicesBufferLength; i++)
            {
                var inventoryItem = inventory[indices[i]];
                if (match.Invoke(inventoryItem.Item))
                {
                    indicesBuffer[count++] = indices[i];
                }
            }
            return count;
        }

        /// <summary>
        /// Use to fill <paramref name="inventoryItemBuffer"/> if both buffers requires different limits
        /// </summary>
        static int Fill(Inventory inventory, 
            ReadOnlyInventoryItem[] inventoryItemBuffer, int inventoryItemBufferLength, 
            int[] indicesBuffer, int indicesBufferLength)
        {
            int count = 0;
            for (; count < indicesBufferLength && count < inventoryItemBufferLength; count++)
            {
                inventoryItemBuffer[count] = inventory[indicesBuffer[count]];
            }
            return count;
        }
    }
}