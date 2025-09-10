using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using XIV.Core.Utils;

namespace XIV.Packages.InventorySystem.Editor
{
    public class ItemCreatorWindow : EditorWindow
    {
        public string itemName = "Misc Item";
        
        public ItemCreatorData itemCreatorData;

        private string[] typeOptions = new string[]
        {
            "Add Field", "int", "float", "bool", "char", "string", "double", "long", "byte"
        };

        Dictionary<string, Type> FInfoMap = new()
        {
            { "int", typeof(ItemCreatorData.FInfoInt) },
            { "float", typeof(ItemCreatorData.FInfoFloat) },
            { "double", typeof(ItemCreatorData.FInfoDouble) },
            { "long", typeof(ItemCreatorData.FInfoLong) },
            { "byte", typeof(ItemCreatorData.FInfoByte) },
            { "bool", typeof(ItemCreatorData.FInfoBool) },
            { "char", typeof(ItemCreatorData.FInfoChar) },
            { "string", typeof(ItemCreatorData.FInfoString) },
        };

        public string path;

        [MenuItem("XIV Tools/Inventory System/ItemCreator")]
        public static void ShowWindow()
        {
            var w = GetWindow<ItemCreatorWindow>("Item Creator");
            w.path = EditorPrefs.GetString(nameof(ItemCreatorWindow) + "-" + nameof(path));
            w.Show();
        }

        void OnDestroy()
        {
            EditorPrefs.SetString(nameof(ItemCreatorWindow) + "-" + nameof(path), path);
        }

        void OnGUI()
        {
            if (!itemCreatorData)
            {
                string path = "Assets/Editor/ItemCreatorData.asset";
                itemCreatorData = AssetDatabase.LoadAssetAtPath<ItemCreatorData>(path);

                if (!itemCreatorData)
                {
                    itemCreatorData = CreateInstance<ItemCreatorData>();
                    Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(itemCreatorData, path);
                    AssetDatabase.SaveAssets();
                }
            }
            
            var obj = EditorGUILayout.ObjectField(itemCreatorData, typeof(ItemCreatorData), false);
            if (obj is ItemCreatorData newObj && obj != itemCreatorData)
            {
                itemCreatorData = newObj;
            }
            
            EditorGUILayout.LabelField("Item Creator", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Item Name");
            itemName = EditorGUILayout.TextField(itemName);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Select a Type", EditorStyles.boldLabel);
            var selectedTypeIndex = EditorGUILayout.Popup("Type", 0, typeOptions);
            if (selectedTypeIndex != 0)
            {
                var typeName = typeOptions[selectedTypeIndex];
                var selectedType = GetSelectionType(typeName);
                var fieldInfoType = FInfoMap[typeName];
                object instance = Activator.CreateInstance(fieldInfoType);
                itemCreatorData.fields[selectedType.FullName].Add(instance);
                EditorUtility.SetDirty(itemCreatorData);
            }
            
            EditorGUILayout.Space();
            foreach (var kvp in itemCreatorData.fields)
            {
                Type type = Type.GetType(kvp.Key);
                var list = kvp.Value;

                if (list.Count == 0) continue;

                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"Fields of type {type.Name}", EditorStyles.boldLabel);

                for (int i = 0; i < list.Count; i++)
                {
                    object fieldInfoObj = list[i];
                    Type fieldInfoType = fieldInfoObj.GetType();
                    
                    FieldInfo nameField = fieldInfoType.GetField("fieldName");
                    FieldInfo valueField = fieldInfoType.GetField("fieldValue");

                    if (nameField != null && valueField != null)
                    {
                        string name = (string)nameField.GetValue(fieldInfoObj);
                        object value = valueField.GetValue(fieldInfoObj);
                        name = name?.Replace(" ", "");

                        EditorGUILayout.BeginHorizontal();

                        name = EditorGUILayout.TextField(name);
                        object newValue = DrawFieldValue(type, value);

                        nameField.SetValue(fieldInfoObj, name);
                        valueField.SetValue(fieldInfoObj, newValue);
                        
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                        {
                            list.RemoveAt(i);
                            EditorUtility.SetDirty(itemCreatorData);
                            EditorGUILayout.EndHorizontal();
                            break;
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            if (HasFields())
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Create At:", GUILayout.Width(90));
                EditorGUILayout.LabelField("Assets/", GUILayout.Width(55));
                path = EditorGUILayout.TextArea(path, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();
            }
            
            if (HasFields() && GUILayout.Button("Create Item"))
            {
                var tmpItemName = itemName.Replace(" ", "");
                ClassGenerator itemGenerator = new ClassGenerator(className: tmpItemName, inheritance: "ItemBase");
                itemGenerator.AddUsing("XIV.Packages.InventorySystem");
                itemGenerator.AddAttribute("System.Serializable");
                itemGenerator.PutInsideNamespace("TheGame.Items");

                Dictionary<string, int> fieldNames = new();

                foreach (var kvp in itemCreatorData.fields)
                {
                    for (int i = 0; i < kvp.Value.Count; i++)
                    {
                        object fieldInfoObj = kvp.Value[i];
                        Type fieldInfoType = fieldInfoObj.GetType();
                        FieldInfo nameField = fieldInfoType.GetField("fieldName");
                        FieldInfo valueField = fieldInfoType.GetField("fieldValue");
                        
                        string name = (string)nameField.GetValue(fieldInfoObj);
                        object value = valueField.GetValue(fieldInfoObj);
                        // numeric valuse and boolean
                        var val = value.ToString().Replace(",", ".").ToLowerInvariant();
                        var fieldTypeStr = valueField.FieldType.ToString();

                        if (string.IsNullOrEmpty(name))
                        {
                            EditorUtility.DisplayDialog("Empty Field Name",
                                $"There is a field with undefined name in {kvp.Key} at position {i}. Please Fix the naming and try again",
                                "Ok");
                            return;
                        }

                        if (fieldNames.TryGetValue(name, out var v))
                        {
                            EditorUtility.DisplayDialog("Duplicate Field Name",
                                $"Duplicate Detected.\n" +
                                $"Name : {name}\n" +
                                $"Field defined with the same or similar name in {kvp.Key} at position {i}.\n",
                                "Ok");
                            return;
                        }
                        fieldNames.Add(name, i);

                        name = name.Replace(" ", "");
                        if (fieldTypeStr == "System.String")
                        {
                            val = "\"" + val + "\"";
                        }

                        if (fieldTypeStr == "System.Single")
                        {
                            val = val + "f";
                        }

                        if (fieldTypeStr == "System.Char")
                        {
                            val = "'" + val + "'";
                        }
                        
                        itemGenerator.AddUsing(valueField.FieldType.Namespace);
                        itemGenerator.AddField(name, val, fieldTypeStr, accessModifier:"public");
                    }
                }
                string itemNameSO = tmpItemName + "SO";
                ClassGenerator soGenerator = new ClassGenerator(className: itemNameSO, inheritance: "ItemSO<" + tmpItemName + ">");
                soGenerator.AddAttribute($"[CreateAssetMenu(menuName = MenuPaths.ITEMS_MENU + nameof({itemNameSO}))]");
                soGenerator.AddUsing("UnityEngine");
                soGenerator.AddUsing("XIV.Packages.InventorySystem.ScriptableObjects");
                soGenerator.AddUsing("TheGame.Items");
                soGenerator.PutInsideNamespace("TheGame.ScriptableObjects");

                var tmpPath = Path.Combine("Assets/", path);
                File.WriteAllText(tmpPath + tmpItemName + ".cs", itemGenerator.ToString());
                File.WriteAllText(tmpPath + itemNameSO + ".cs", soGenerator.ToString());
                EditorUtility.SetDirty(itemCreatorData);
                AssetDatabase.Refresh();
            }

            if (HasFields() && GUILayout.Button("Clear"))
            {
                foreach (var kvp in itemCreatorData.fields)
                {
                    kvp.Value.Clear();
                    EditorUtility.SetDirty(itemCreatorData);
                }
            }
            
            AssetDatabase.SaveAssetIfDirty(itemCreatorData);
        }

        bool HasFields()
        {
            foreach (var kvp in itemCreatorData.fields)
            {
                if (kvp.Value.Count > 0) return true;
            }
            return false;
        }

        object DrawFieldValue(Type type, object value)
        {
            if (type == typeof(int))
                return EditorGUILayout.IntField((int)(value ?? 0));
            if (type == typeof(float))
                return EditorGUILayout.FloatField((float)(value ?? 0f));
            if (type == typeof(bool))
                return EditorGUILayout.Toggle((bool)(value ?? false));
            if (type == typeof(string))
                return EditorGUILayout.TextField((string)(value ?? ""));
            if (type == typeof(double))
                return EditorGUILayout.DoubleField((double)(value ?? 0d));
            if (type == typeof(long))
                return EditorGUILayout.LongField((long)(value ?? 0L));
            if (type == typeof(byte))
                return (byte)Mathf.Clamp(EditorGUILayout.IntField((byte)(value ?? (byte)0)), 0, 255);
            if (type == typeof(char))
            {
                string str = EditorGUILayout.TextField((value ?? ' ').ToString());
                return string.IsNullOrEmpty(str) ? ' ' : str[0];
            }

            EditorGUILayout.LabelField("Unsupported type");
            return value;
        }

        Type GetSelectionType(string selectedType)
        {
            switch (selectedType)
            {
                case "int":
                    return typeof(int);
                case "float":
                    return typeof(float);
                case "bool":
                    return typeof(bool);
                case "char":
                    return typeof(char);
                case "string":
                    return typeof(string);
                case "double": 
                    return typeof(double);
                case "long":
                    return typeof(long);
                case "byte":
                    return typeof(byte);
                default:
                    return null;
            }
        }
    }
}
