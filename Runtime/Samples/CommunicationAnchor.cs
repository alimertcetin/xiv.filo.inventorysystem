using System;
using UnityEngine;
using XIV.Packages.InventorySystem;

namespace XIV.Packages.InventorySystem.Samples
{
    /// <summary>
    /// Simple communication anchor between <see cref="InventoryManager"/> and other systems.
    /// </summary>
    public static class CommunicationAnchor
    {
        static Action onSceneReady;
        static Action<Inventory> onInventoryLoaded;
        static Action<InventoryChange> onInventoryChange;

        // To avoid unexpected things when "enter play mode options" enabled
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            // initialize empty to avoid null checks
            onSceneReady = delegate {  };
            onInventoryLoaded = delegate {  };
            onInventoryChange = delegate {  };
        }

        /// <summary>
        /// Register with <paramref name="action"/> to get informed when <see cref="InvokeSceneReady"/> called.
        /// </summary>
        /// <param name="action">Action to invoke on <see cref="InvokeSceneReady"/> call.</param>
        public static void RegisterSceneReady(Action action) => onSceneReady += action;
        /// <summary>
        /// Unregister the previously registered <paramref name="action"/>.
        /// </summary>
        /// <param name="action">Action that used before on <see cref="RegisterSceneReady"/>.</param>
        public static void UnregisterSceneReady(Action action) => onSceneReady -= action;
        /// <summary>
        /// Inform registered actions.
        /// </summary>
        public static void InvokeSceneReady() => onSceneReady.Invoke();

        /// <summary>
        /// Register with <paramref name="action"/> to get informed when <see cref="InvokeInventoryLoad"/> called.
        /// </summary>
        /// <param name="action">Action to invoke on <see cref="InvokeInventoryLoad"/> call.</param>
        public static void RegisterInventoryLoad(Action<Inventory> action) => onInventoryLoaded += action;
        /// <summary>
        /// Unregister the previously registered <paramref name="action"/>.
        /// </summary>
        /// <param name="action">Action that used before on <see cref="RegisterInventoryLoad"/>.</param>
        public static void UnregisterInventoryLoad(Action<Inventory> action) => onInventoryLoaded -= action;
        /// <summary>
        /// Inform registered actions.
        /// </summary>
        public static void InvokeInventoryLoad(Inventory inventory) => onInventoryLoaded.Invoke(inventory);

        /// <summary>
        /// Register with <paramref name="action"/> to get informed when <see cref="InvokeInventoryChange"/> called.
        /// </summary>
        /// <param name="action">Action to invoke on <see cref="InvokeInventoryLoad"/> call.</param>
        public static void RegisterInventoryChange(Action<InventoryChange> action) => onInventoryChange += action;
        /// <summary>
        /// Unregister the previously registered <paramref name="action"/>.
        /// </summary>
        /// <param name="action">Action that used before on <see cref="RegisterInventoryChange"/>.</param>
        public static void UnregisterInventoryChange(Action<InventoryChange> action) => onInventoryChange -= action;
        /// <summary>
        /// Inform registered actions.
        /// </summary>
        public static void InvokeInventoryChange(InventoryChange inventoryChange) => onInventoryChange.Invoke(inventoryChange);
    }
}