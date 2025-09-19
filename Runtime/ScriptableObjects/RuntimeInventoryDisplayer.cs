using System.Collections.Generic;
using UnityEngine;

namespace XIV.Packages.InventorySystem.ScriptableObjects
{
    /// <summary>
    /// Helps us to visualize the inventory in the inspector by listening the changes.
    /// </summary>
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

        public RuntimeInventoryDisplayer(Inventory inventory)
        {
            runtimeItems = new List<RuntimeItemData>(inventory.slotCount);
            inventory.Register(new InventoryRuntimeListener(inventory, runtimeItems));
        }
            
        class InventoryRuntimeListener : IInventoryListener
        {
            readonly Inventory inventory;
            readonly List<RuntimeItemData> runtimeItems;
            
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
                    var name = item.IsEmpty == false ? item.Item.name : "Empty";
                    var quantity = item.Quantity;
                    var itemBase = item.Item;
                    var runtimeItem = new RuntimeItemData { name = name, quantity = quantity, item = itemBase, };
                    runtimeItems.Add(runtimeItem);
                }
            }
        }
    }
}