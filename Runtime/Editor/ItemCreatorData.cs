using System;
using System.Collections.Generic;
using UnityEngine;
using XIV.Packages.InventorySystem.ScriptableObjects;

namespace XIV.Packages.InventorySystem.Editor
{
    [CreateAssetMenu(menuName = MenuPaths.EDITOR_DATA_MENU + nameof(ItemCreatorData), fileName = nameof(ItemCreatorData), order = 0)]
    public class ItemCreatorData : ScriptableObject
    {
        [Serializable]
        public class CustomFieldInfo
        {
            public string fieldName;
            public string fieldType; // Full type name (e.g., "int", "float", "MyGame.CustomStruct")
            public string valueString; // String representation of the initial/default value

            public CustomFieldInfo(string fieldName = "", string fieldType = "int", string valueString = "0")
            {
                this.fieldName = fieldName;
                this.fieldType = fieldType;
                this.valueString = valueString;
            }
        }

        public List<CustomFieldInfo> fields = new List<CustomFieldInfo>();
    }
}