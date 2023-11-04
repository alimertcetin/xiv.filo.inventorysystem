using System;

namespace XIV.Packages.InventorySystem
{
    /// <summary>
    /// An abstract class to represent items in the game.
    /// </summary>
    /// <remarks>
    /// It is recommended to implement every derived class to implement <see cref="IEquatable{T}"/>.
    /// </remarks>
    [Serializable]
    public abstract class ItemBase : IEquatable<ItemBase>
    {
        /// <summary>
        /// The name of the item.
        /// </summary>
        public string name;
        /// <summary>
        /// The description of item.
        /// </summary>
        public string description;
        /// <summary>
        /// The stackable quantity of item.
        /// </summary>
        /// <remarks>
        /// Make sure stackable quantity of item is greater than 0.
        /// Otherwise, you won't be able to see any error.
        /// </remarks>
        public int stackableQuantity = 1;

        public bool Equals(ItemBase other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return name == other.name && description == other.description && stackableQuantity == other.stackableQuantity;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is ItemBase other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(name, description, stackableQuantity);
        }
    }
}