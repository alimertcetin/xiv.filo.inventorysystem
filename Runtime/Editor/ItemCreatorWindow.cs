using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XIV.Packages.InventorySystem.Editor
{
    public class ItemCreatorWindow : EditorWindow
    {
        public string itemName = "Misc Item";
        public ItemCreatorData itemCreatorData;
        public string path = "Scripts/Items/";

        string customTypeName = "MyNamespace.MyCustomStruct";
        string customFieldName = "myField";

        static readonly string[] BuiltInTypeOptions = new string[]
        {
            "Add Built-In Field...", "int", "float", "bool", "char", "string", "double", "long", "byte", "Vector2", "Vector3", "Color"
        };

        [MenuItem("XIV Tools/Inventory System/ItemCreator")]
        public static void ShowWindow()
        {
            var w = GetWindow<ItemCreatorWindow>("Item Creator");
            string savedPath = EditorPrefs.GetString(nameof(ItemCreatorWindow) + "-" + nameof(path), "Scripts/Items/");
            if (!string.IsNullOrEmpty(savedPath))
            {
                w.path = savedPath;
            }
            w.Show();
        }

        void OnDestroy()
        {
            EditorPrefs.SetString(nameof(ItemCreatorWindow) + "-" + nameof(path), path);
        }

        void OnGUI()
        {
            EnsureDataAsset();

            var obj = EditorGUILayout.ObjectField("Creator Data", itemCreatorData, typeof(ItemCreatorData), false);
            if (obj is ItemCreatorData newObj && obj != itemCreatorData)
            {
                itemCreatorData = newObj;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Item Creator", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Item Name", GUILayout.Width(90));
            itemName = EditorGUILayout.TextField(itemName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            DrawAddFieldsSection();

            EditorGUILayout.Space();
            DrawFieldsListSection();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Create At:", GUILayout.Width(90));
            EditorGUILayout.LabelField("Assets/", GUILayout.Width(55));
            path = EditorGUILayout.TextField(path, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            if (GUILayout.Button("Create Item Classes", GUILayout.Height(30)))
            {
                GenerateItemClasses();
            }

            if (GUILayout.Button("Clear All Fields"))
            {
                itemCreatorData.fields.Clear();
                EditorUtility.SetDirty(itemCreatorData);
            }

            AssetDatabase.SaveAssetIfDirty(itemCreatorData);
        }

        void EnsureDataAsset()
        {
            if (!itemCreatorData)
            {
                string assetPath = "Assets/Editor";
                string fullAssetPath = assetPath + "/ItemCreatorData.asset";
                itemCreatorData = AssetDatabase.LoadAssetAtPath<ItemCreatorData>(fullAssetPath);

                if (!itemCreatorData)
                {
                    itemCreatorData = CreateInstance<ItemCreatorData>();
                    Directory.CreateDirectory(Path.GetDirectoryName(Application.dataPath + "/../" + assetPath));
                    AssetDatabase.CreateAsset(itemCreatorData, fullAssetPath);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        void DrawAddFieldsSection()
        {
            EditorGUILayout.LabelField("Add Fields", EditorStyles.boldLabel);

            // 1. Built-in Types Dropdown
            int selectedTypeIndex = EditorGUILayout.Popup("Built-in Type", 0, BuiltInTypeOptions);
            if (selectedTypeIndex > 0)
            {
                string typeName = BuiltInTypeOptions[selectedTypeIndex];
                itemCreatorData.fields.Add(new ItemCreatorData.CustomFieldInfo("newField", typeName, GetDefaultValueString(typeName)));
                EditorUtility.SetDirty(itemCreatorData);
            }

            EditorGUILayout.Space(5);

            // 2. Custom Type / Struct Input
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Add Custom Struct / Type", EditorStyles.miniBoldLabel);
            customFieldName = EditorGUILayout.TextField("Field Name", customFieldName);
            customTypeName = EditorGUILayout.TextField("Type (FullName)", customTypeName);

            if (GUILayout.Button("Add Custom Field"))
            {
                if (string.IsNullOrWhiteSpace(customFieldName) || string.IsNullOrWhiteSpace(customTypeName))
                {
                    EditorUtility.DisplayDialog("Error", "Field Name and Type Name cannot be empty.", "OK");
                }
                else
                {
                    itemCreatorData.fields.Add(new ItemCreatorData.CustomFieldInfo(customFieldName.Trim(), customTypeName.Trim(), "new()"));
                    EditorUtility.SetDirty(itemCreatorData);
                }
            }
            EditorGUILayout.EndVertical();
        }

        void DrawFieldsListSection()
        {
            if (itemCreatorData.fields.Count == 0) return;

            EditorGUILayout.LabelField("Defined Fields", EditorStyles.boldLabel);

            int indexToRemove = -1;

            for (int i = 0; i < itemCreatorData.fields.Count; i++)
            {
                var field = itemCreatorData.fields[i];

                EditorGUILayout.BeginHorizontal();
                field.fieldName = EditorGUILayout.TextField(field.fieldName, GUILayout.Width(130));
                
                EditorGUILayout.LabelField($"({field.fieldType})", EditorStyles.miniLabel, GUILayout.Width(110));

                field.valueString = DrawFieldValueInput(field.fieldType, field.valueString);

                if (GUILayout.Button("X", GUILayout.Width(22)))
                {
                    indexToRemove = i;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (indexToRemove != -1)
            {
                itemCreatorData.fields.RemoveAt(indexToRemove);
                EditorUtility.SetDirty(itemCreatorData);
            }
        }

        string DrawFieldValueInput(string typeName, string currentValue)
        {
            switch (typeName)
            {
                case "int":
                    int.TryParse(currentValue, out int iVal);
                    return EditorGUILayout.IntField(iVal).ToString();

                case "float":
                    float.TryParse(currentValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float fVal);
                    return EditorGUILayout.FloatField(fVal).ToString(CultureInfo.InvariantCulture);

                case "bool":
                    bool.TryParse(currentValue, out bool bVal);
                    return EditorGUILayout.Toggle(bVal).ToString().ToLowerInvariant();

                case "string":
                    return EditorGUILayout.TextField(currentValue ?? "");

                case "char":
                    char cVal = string.IsNullOrEmpty(currentValue) ? ' ' : currentValue[0];
                    string res = EditorGUILayout.TextField(cVal.ToString());
                    return string.IsNullOrEmpty(res) ? " " : res[0].ToString();

                case "double":
                    double.TryParse(currentValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double dVal);
                    return EditorGUILayout.DoubleField(dVal).ToString(CultureInfo.InvariantCulture);

                case "long":
                    long.TryParse(currentValue, out long lVal);
                    return EditorGUILayout.LongField(lVal).ToString();

                case "byte":
                    byte.TryParse(currentValue, out byte byteVal);
                    return ((byte)Mathf.Clamp(EditorGUILayout.IntField(byteVal), 0, 255)).ToString();

                default:
                    // Fallback for custom structs/classes: default initializer string or field expression
                    return EditorGUILayout.TextField(currentValue ?? "new()");
            }
        }

        string GetDefaultValueString(string typeName)
        {
            return typeName switch
            {
                "int" => "0",
                "float" => "0f",
                "bool" => "false",
                "string" => "\"\"",
                "char" => "' '",
                "double" => "0d",
                "long" => "0L",
                "byte" => "0",
                "Vector2" => "Vector2.zero",
                "Vector3" => "Vector3.zero",
                "Color" => "Color.white",
                _ => "default",
            };
        }

        void GenerateItemClasses()
        {
            string tmpItemName = itemName.Replace(" ", "");
            if (string.IsNullOrEmpty(tmpItemName))
            {
                EditorUtility.DisplayDialog("Invalid Name", "Item name cannot be empty.", "Ok");
                return;
            }

            var itemGenerator = new ClassGenerator(className: tmpItemName, inheritance: "ItemBase");
            itemGenerator.AddUsing("XIV.Packages.InventorySystem");
            itemGenerator.AddUsing("UnityEngine");
            itemGenerator.AddAttribute("System.Serializable");
            itemGenerator.PutInsideNamespace("TheGame.Items");

            var fieldNames = new HashSet<string>();

            foreach (var field in itemCreatorData.fields)
            {
                string fName = field.fieldName.Replace(" ", "");

                if (string.IsNullOrEmpty(fName))
                {
                    EditorUtility.DisplayDialog("Empty Field Name", "Found a field with an empty name.", "Ok");
                    return;
                }

                if (!fieldNames.Add(fName))
                {
                    EditorUtility.DisplayDialog("Duplicate Field Name", $"Duplicate field name: {fName}", "Ok");
                    return;
                }

                // Automatically inject using namespaces for fully qualified custom structs if needed
                if (field.fieldType.Contains("."))
                {
                    int lastDot = field.fieldType.LastIndexOf('.');
                    string ns = field.fieldType.Substring(0, lastDot);
                    itemGenerator.AddUsing(ns);
                }

                string formattedVal = FormatDefaultValue(field.fieldType, field.valueString);
                itemGenerator.AddField(fName, formattedVal, field.fieldType, accessModifier: "public");
            }

            string itemNameSO = tmpItemName + "SO";
            var soGenerator = new ClassGenerator(className: itemNameSO, inheritance: $"ItemSO<{tmpItemName}>");
            soGenerator.AddAttribute($"[CreateAssetMenu(menuName = MenuPaths.ITEMS_MENU + nameof({itemNameSO}))]");
            soGenerator.AddUsing("UnityEngine");
            soGenerator.AddUsing("XIV.Packages.InventorySystem.ScriptableObjects");
            soGenerator.AddUsing("TheGame.Items");
            soGenerator.PutInsideNamespace("TheGame.ScriptableObjects");

            string fullFolderPath = Path.Combine(Application.dataPath, path);
            if (!Directory.Exists(fullFolderPath))
            {
                Directory.CreateDirectory(fullFolderPath);
            }

            File.WriteAllText(Path.Combine(fullFolderPath, tmpItemName + ".cs"), itemGenerator.ToString());
            File.WriteAllText(Path.Combine(fullFolderPath, itemNameSO + ".cs"), soGenerator.ToString());

            EditorUtility.SetDirty(itemCreatorData);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", $"Item classes successfully created at Assets/{path}", "Ok");
        }

        string FormatDefaultValue(string fieldType, string val)
        {
            if (string.IsNullOrEmpty(val)) return "default";

            return fieldType switch
            {
                "string" => val.StartsWith("\"") && val.EndsWith("\"") ? val : $"\"{val}\"",
                "float" => val.ToLowerInvariant().EndsWith("f") ? val : $"{val}f",
                "char" => val.StartsWith("'") && val.EndsWith("'") ? val : $"'{val[0]}'",
                "bool" => val.ToLowerInvariant(),
                _ => val
            };
        }
    }
}