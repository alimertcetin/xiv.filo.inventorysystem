using System;
using System.Collections.Generic;
using UnityEngine;

namespace XIV.Packages.InventorySystem.Editor
{
    /// <summary>
    /// A Serializable Dictionary
    /// </summary>
    /// <example>
    /// <code>
    /// public sealed class SerializableStringIntListDictionary : XIVSerializedDictionary{string, SerializedList{int}} {}
    /// </code>
    /// </example>
    /// <typeparam name="TKey">TKey</typeparam>
    /// <typeparam name="TValue">TValue</typeparam>
    [Serializable]
    public class XIVSerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] public List<TKey> keys = new List<TKey>();
        [SerializeField] public List<TValue> values = new List<TValue>();

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach (var kvp in this)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            if (keys.Count != values.Count) return;
            
            for (int i = 0; i < keys.Count; i++)
            {
                this.Add(keys[i], values[i]);
            }

            keys.Clear();
            values.Clear();
        }
    }
}