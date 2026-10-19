using UnityEngine;
using XIV.Packages.InventorySystem;
using XIV.Packages.InventorySystem.ScriptableObjects;

namespace XIV.Packages.InventorySystem.Samples
{
    public class InventoryManager : MonoBehaviour, IInventoryListener
    {
        [SerializeField] InventorySO inventorySO;

        public Inventory inventory { get; private set; }

        void Awake() => inventory = inventorySO.GetInventory();
        void Start() => CommunicationAnchor.InvokeInventoryLoad(inventory);
        
        void OnEnable()
        {
            CommunicationAnchor.RegisterSceneReady(OnSceneReady);
            inventory.Register(this);
        }

        void OnDisable()
        {
            CommunicationAnchor.UnregisterSceneReady(OnSceneReady);
            inventory.Unregister(this);
        }

        void OnSceneReady()
        {
            CommunicationAnchor.InvokeInventoryLoad(inventory);
        }

        void IInventoryListener.OnInventoryChanged(InventoryChange inventoryChange)
        {
            CommunicationAnchor.InvokeInventoryChange(inventoryChange);
        }
    }
}