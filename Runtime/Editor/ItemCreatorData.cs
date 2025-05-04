using System;
using System.Collections;
using UnityEngine;

namespace XIV.Packages.InventorySystem.Editor
{
    [CreateAssetMenu(menuName = "Create ItemCreatorData", fileName = "ItemCreatorData", order = 0)]
    public class ItemCreatorData : ScriptableObject
    {
        [Serializable]
        public class FInfo<T>
        {
            public Type fieldType => fieldValue.GetType();
            public string fieldName;
            public T fieldValue;

            public FInfo()
            {
                fieldName = string.Empty;
                fieldValue = default(T);
            }
        }
        
        [Serializable]
        public sealed class FInfoInt : FInfo<int>{}
        [Serializable]
        public sealed class FInfoFloat : FInfo<float>{}
        [Serializable]
        public sealed class FInfoLong : FInfo<long>{}
        [Serializable]
        public sealed class FInfoDouble : FInfo<double>{}
        [Serializable]
        public sealed class FInfoByte : FInfo<byte>{}
        [Serializable]
        public sealed class FInfoBool : FInfo<bool>{}
        [Serializable]
        public sealed class FInfoChar : FInfo<char>{}
        [Serializable]
        public sealed class FInfoString : FInfo<string>{}
        
        [Serializable]
        public sealed class SerializableIntList : XIVSerializedList<FInfoInt>{}
        [Serializable]
        public sealed class SerializableFloatList : XIVSerializedList<FInfoFloat>{}
        [Serializable]
        public sealed class SerializableLongList : XIVSerializedList<FInfoLong>{}
        [Serializable]
        public sealed class SerializableDoubleList : XIVSerializedList<FInfoDouble>{}
        [Serializable]
        public sealed class SerializableByteList : XIVSerializedList<FInfoByte>{}
        [Serializable]
        public sealed class SerializableBoolList : XIVSerializedList<FInfoBool>{}
        [Serializable]
        public sealed class SerializableCharList : XIVSerializedList<FInfoChar>{}
        [Serializable]
        public sealed class SerializableStringList : XIVSerializedList<FInfoString>{}
        
        [Serializable]
        public sealed class XIVSerializedStringToXIVSerializedListDict : XIVSerializedDictionary<string, XIVSerializedList>{}
        
        public XIVSerializedStringToXIVSerializedListDict fields = new()
        {
            { typeof(int).FullName, new SerializableIntList() },
            { typeof(float).FullName, new SerializableFloatList() },
            { typeof(long).FullName, new SerializableLongList() },
            { typeof(double).FullName, new SerializableDoubleList() },
            { typeof(byte).FullName, new SerializableByteList() },
            { typeof(bool).FullName, new SerializableBoolList() },
            { typeof(char).FullName, new SerializableCharList() },
            { typeof(string).FullName, new SerializableStringList() },
        };
    }
}