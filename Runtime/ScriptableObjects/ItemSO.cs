using UnityEngine;

namespace XIV_Packages.InventorySystem.ScriptableObjects
{
    /// <summary>
    /// An abstract class to connect primitive items to Unity.
    /// Any non-serializable object can be stored in this class.
    /// </summary>
    /// <seealso cref="ItemBase"/>
    public abstract class ItemSO : ScriptableObject
    {
        public Sprite icon;
        
        /// <summary>
        /// Returns the referenced item.
        /// </summary>
        public abstract ItemBase GetItem();
    }

    /// <summary>
    /// <inheritdoc cref="ItemSO"/>
    /// <example>
    /// <code>
    /// Define a class that contains required values for your item.
    /// public class SwordItem : ItemBase
    /// {
    ///     public int damage;
    ///     public float swingSpeed;
    /// }
    /// Create another class to allow creating SwordItem in the Editor.
    /// [CreateAssetMenu(menuName = "Items/" + nameof(SwordItemSO))]
    /// public class SwordItemSO : ItemSO&lt;SwordItem&gt;
    /// {
    /// }
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    public abstract class ItemSO<T> : ItemSO where T : ItemBase
    {
        [SerializeField] T item;

        public override ItemBase GetItem() => item;
    }
}