using System;
using System.Collections.Generic;
using UnityEngine;

namespace XIV.Packages.InventorySystem.Editor
{
    /// <summary>
    /// Designed to use with <see cref="XIVSerializedDictionary{TKey,TValue}"/>
    /// </summary>
    /// <example>
    /// <code>
    /// public sealed class SerializableStringIntListDictionary : XIVSerializedDictionary{string, SerializedList{int}} {}
    /// </code>
    /// </example>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public abstract class XIVSerializedList<T> : XIVSerializedList, ISerializationCallbackReceiver
    {
        [SerializeField] List<T> values = new List<T>();

        T[] arr = new T[8];
        int count = 0;

        public T this[int index]
        {
            get => arr[index];
            set => arr[index] = value;
        }

        public override void Add(object value)
        {
            var v = (T)value;
            this.Add(v);
        }

        public override void Clear()
        {
            count = 0;
        }

        public override object GetItem(int index)
        {
            return this[index];
        }

        public override void RemoveAt(int index)
        {
            if (index < 0) return;
            
            for (int i = index; i < Count - 1; i++)
            {
                values[i] = values[i + 1];
            }
            count--;
        }

        protected override int GetCount()
        {
            return count;
        }

        public void OnBeforeSerialize()
        {
            values.Clear();

            for (var i = 0; i < this.count; i++)
            {
                values.Add(this[i]);
            }
        }

        public void OnAfterDeserialize()
        {
            for(int i=0; i<values.Count; i++) 
                Add(values[i]);
            values.Clear();
        }

        public void Add(T value)
        {
            if (count >= arr.Length) Array.Resize(ref arr, arr.Length * 2);
            arr[count++] = value;
        }
    }

    [Serializable]
    public abstract class XIVSerializedList
    {
        public object this[int index] => GetItem(index);
        public int Count => GetCount();
        public abstract void Add(object value);
        public abstract void Clear();
        public abstract object GetItem(int index);
        public abstract void RemoveAt(int index);
        protected abstract int GetCount();
        
    }
}