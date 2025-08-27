using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helps us to visualize the inventory in the inspector by listening the changes.
/// </summary>
namespace XIV.Packages.InventorySystem.ScriptableObjects
{
    [System.Serializable]
    public class RuntimeInventoryDisplayer
    {
        [System.Serializable]
        struct RuntimeItemData
        {
            public string name;
            public ItemBase item;
            public int quantity;
        }
        [SerializeField] List<RuntimeItemData> runtimeItems;

        public RuntimeInventoryDisplayer(Inventory inventory, int slotCount)
        {
            runtimeItems = new List<RuntimeItemData>(slotCount);
            for (var i = 0; i < inventory.slotCount; i++)
            {
                ReadOnlyInventoryItem item = inventory[i];
                var runtimeItem = new RuntimeItemData
                {
                    name = item.IsEmpty ? "EMPTY" : item.Item.GetType().Name.Split('.')[^1],
                    quantity = item.Quantity,
                    item = item.Item,
                };
                runtimeItems.Add(runtimeItem);
            }
            inventory.Register(new InventoryRuntimeListener(inventory, runtimeItems));
        }
            
        class InventoryRuntimeListener : IInventoryListener
        {
            Inventory inventory;
            List<RuntimeItemData> runtimeItems;
            
            public InventoryRuntimeListener(Inventory inventory, List<RuntimeItemData> runtimeItems)
            {
                this.inventory = inventory;
                this.runtimeItems = runtimeItems;
                Refresh();
            }
            
            void IInventoryListener.OnInventoryChanged(InventoryChange inventoryChange)
            {
                Refresh();
            }

            void Refresh()
            {
                runtimeItems.Clear();
                for (int i = 0; i < inventory.slotCount; i++)
                {
                    ReadOnlyInventoryItem item = inventory[i];
                    var name = item.IsEmpty == false ? item.Item.GetType().Name.Split('.')[^1] : "Empty";
                    var quantity = item.Quantity;
                    var itemBase = item.Item;
                    var runtimeItem = new RuntimeItemData { name = name, quantity = quantity, item = itemBase, };
                    runtimeItems.Add(runtimeItem);
                }
            }
        }
    }
}