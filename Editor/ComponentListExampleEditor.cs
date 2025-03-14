using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using System.Reflection;

[CustomEditor(typeof(Jobs))]
public class ComponentListExampleEditor : Editor
{
    private bool[] foldouts;
    private bool showScriptSelector;
    private Vector2 scriptSelectorScroll;
    private string searchTerm = "";
    private string selectedCategory = "";
    private static CustomJobs clipboardComponent;
    private GUIStyle smallButtonStyle;

    private void OnEnable()
    {
        Jobs example = (Jobs)target;
        foldouts = new bool[example.customComponents.Count];

        smallButtonStyle = new GUIStyle(EditorStyles.miniButton)
        {
            padding = new RectOffset(4, 4, 2, 2)
        };
    }

    public override void OnInspectorGUI()
    {
        Jobs example = (Jobs)target;
        serializedObject.Update();

        if (example.customComponents == null)
            example.customComponents = new List<CustomJobs>();

        if (foldouts == null || foldouts.Length != example.customComponents.Count)
        {
            foldouts = new bool[example.customComponents.Count];
        }

        EditorGUILayout.Space();

        // Header with title and paste button
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Jobs", EditorStyles.boldLabel);
        GUI.enabled = clipboardComponent != null;
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Display existing components
        for (int i = 0; i < example.customComponents.Count; i++)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Header row with buttons
            EditorGUILayout.BeginHorizontal();

            // Move Up button
            GUI.enabled = i > 0;
            if (GUILayout.Button("↑", smallButtonStyle, GUILayout.Width(25)))
            {
                MoveComponent(example, i, i - 1);
                break;
            }

            // Move Down button
            GUI.enabled = i < example.customComponents.Count - 1;
            if (GUILayout.Button("↓", smallButtonStyle, GUILayout.Width(25)))
            {
                MoveComponent(example, i, i + 1);
                break;
            }
            GUI.enabled = true;

            GUILayout.Space(10);

            string headerText = example.customComponents[i].script != null
                ? $"{i + 1}: {example.customComponents[i].script.GetType().Name}"
                : $"{i + 1}: No Job";

            foldouts[i] = EditorGUILayout.Foldout(foldouts[i], headerText, true);

            GUILayout.Space(10);

            // Copy button
            if (GUILayout.Button("Copy", smallButtonStyle, GUILayout.Width(40)))
            {
                CopyComponent(example.customComponents[i]);
            }

            // Remove button
            if (GUILayout.Button("×", smallButtonStyle, GUILayout.Width(20)))
            {
                example.customComponents.RemoveAt(i);
                GUI.changed = true;
                break;
            }

            EditorGUILayout.EndHorizontal();

            if (foldouts[i] && example.customComponents[i].script != null)
            {
                EditorGUILayout.Space(2);
                EditorGUI.indentLevel++;
                DrawScriptProperties(example.customComponents[i].script);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        EditorGUILayout.Space();

        // Footer buttons
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Add"))
        {
            showScriptSelector = !showScriptSelector;
        }

        GUI.enabled = clipboardComponent != null;
        if (GUILayout.Button("Paste", GUILayout.Width(100)))
        {
            PasteComponent(example);
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        DrawScriptSelector(example);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawScriptSelector(Jobs example)
    {
        if (!showScriptSelector) return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // Search bar
        EditorGUILayout.BeginHorizontal();
        searchTerm = EditorGUILayout.TextField("Search", searchTerm);
        if (GUILayout.Button("Clear", GUILayout.Width(50)))
        {
            searchTerm = "";
        }
        EditorGUILayout.EndHorizontal();

        // Category dropdown
        var categories = JobTypeRegistry.GetCategories();
        categories.Insert(0, "All Categories");
        int categoryIndex = string.IsNullOrEmpty(selectedCategory) ? 0 :
            categories.FindIndex(c => c == selectedCategory);

        categoryIndex = EditorGUILayout.Popup("Category", categoryIndex, categories.ToArray());
        selectedCategory = categoryIndex == 0 ? "" : categories[categoryIndex];

        EditorGUILayout.Space();

        // Display available jobs
        scriptSelectorScroll = EditorGUILayout.BeginScrollView(scriptSelectorScroll,
            GUILayout.MaxHeight(300));

        var filteredJobs = JobTypeRegistry.SearchJobs(searchTerm, selectedCategory);
        string currentCategory = null;

        foreach (var jobInfo in filteredJobs)
        {
            if (currentCategory != jobInfo.Category)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(jobInfo.Category, EditorStyles.boldLabel);
                currentCategory = jobInfo.Category;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (GUILayout.Button(jobInfo.Name))
            {
                IJob newScript = (IJob)Activator.CreateInstance(jobInfo.Type);
                example.AddComponent(newScript);
                showScriptSelector = false;
                foldouts = new bool[example.customComponents.Count];
                foldouts[example.customComponents.Count - 1] = true;
                GUI.changed = true;
            }

            EditorGUILayout.LabelField(jobInfo.Description, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void CopyComponent(CustomJobs component)
    {
        clipboardComponent = new CustomJobs
        {
            script = DeepCopyScript(component.script)
        };
    }

    private void PasteComponent(Jobs example)
    {
        if (clipboardComponent != null)
        {
            var newComponent = new CustomJobs
            {
                script = DeepCopyScript(clipboardComponent.script)
            };

            example.customComponents.Add(newComponent);
            foldouts = new bool[example.customComponents.Count];
            foldouts[example.customComponents.Count - 1] = true;
            GUI.changed = true;
        }
    }

    private IJob DeepCopyScript(IJob original)
    {
        if (original == null) return null;

        Type scriptType = original.GetType();
        IJob copy = (IJob)Activator.CreateInstance(scriptType);

        foreach (var property in scriptType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {
            if (property.CanRead && property.CanWrite)
            {
                var value = property.GetValue(original);
                property.SetValue(copy, value);
            }
        }

        foreach (var field in scriptType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {
            var value = field.GetValue(original);
            field.SetValue(copy, value);
        }

        return copy;
    }

    private void MoveComponent(Jobs example, int fromIndex, int toIndex)
    {
        if (toIndex < 0 || toIndex >= example.customComponents.Count)
            return;

        var item = example.customComponents[fromIndex];
        example.customComponents.RemoveAt(fromIndex);
        example.customComponents.Insert(toIndex, item);

        var foldoutState = foldouts[fromIndex];
        var newFoldouts = new bool[foldouts.Length];
        Array.Copy(foldouts, newFoldouts, foldouts.Length);

        if (fromIndex < toIndex)
        {
            for (int i = fromIndex; i < toIndex; i++)
                newFoldouts[i] = foldouts[i + 1];
        }
        else
        {
            for (int i = fromIndex; i > toIndex; i--)
                newFoldouts[i] = foldouts[i - 1];
        }
        newFoldouts[toIndex] = foldoutState;
        foldouts = newFoldouts;

        GUI.changed = true;
    }

    private void DrawScriptProperties(IJob script)
    {
        var scriptType = script.GetType();
        var fields = scriptType.GetFields();

        foreach (var field in fields)
        {
            if (field.IsPublic)
            {
                var value = field.GetValue(script);
                var newValue = DrawField(field.Name, value, field.FieldType);
                if (!object.Equals(value, newValue))
                {
                    field.SetValue(script, newValue);
                    GUI.changed = true;
                }
            }
        }
    }

    private object DrawField(string name, object value, Type fieldType)
    {
        // Handle null values with improved null handling
        if (value == null)
        {
            if (fieldType.IsValueType)
            {
                value = Activator.CreateInstance(fieldType);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(name, "Null");
                if (GUILayout.Button("Create Instance", GUILayout.Width(100)))
                {
                    return Activator.CreateInstance(fieldType);
                }
                EditorGUILayout.EndHorizontal();
                return null;
            }
        }

        // Handle enums
        if (fieldType.IsEnum)
        {
            return EditorGUILayout.EnumPopup(name, (Enum)value);
        }

        // Handle basic types
        if (fieldType == typeof(float))
        {
            var rangeAttribute = GetRangeAttribute(name, fieldType);
            if (rangeAttribute != null)
                return EditorGUILayout.Slider(name, (float)value, rangeAttribute.min, rangeAttribute.max);
            return EditorGUILayout.FloatField(name, (float)value);
        }
        if (fieldType == typeof(int))
        {
            var rangeAttribute = GetRangeAttribute(name, fieldType);
            if (rangeAttribute != null)
                return EditorGUILayout.IntSlider(name, (int)value, (int)rangeAttribute.min, (int)rangeAttribute.max);
            return EditorGUILayout.IntField(name, (int)value);
        }
        if (fieldType == typeof(double))
            return EditorGUILayout.DoubleField(name, (double)value);
        if (fieldType == typeof(long))
            return EditorGUILayout.LongField(name, (long)value);
        if (fieldType == typeof(string))
            return EditorGUILayout.TextField(name, (string)value);
        if (fieldType == typeof(bool))
            return EditorGUILayout.Toggle(name, (bool)value);

        // Handle Unity types
        if (fieldType == typeof(Vector2))
            return EditorGUILayout.Vector2Field(name, (Vector2)value);
        if (fieldType == typeof(Vector2Int))
            return EditorGUILayout.Vector2IntField(name, (Vector2Int)value);
        if (fieldType == typeof(Vector3))
            return EditorGUILayout.Vector3Field(name, (Vector3)value);
        if (fieldType == typeof(Vector3Int))
            return EditorGUILayout.Vector3IntField(name, (Vector3Int)value);
        if (fieldType == typeof(Vector4))
            return EditorGUILayout.Vector4Field(name, (Vector4)value);
        if (fieldType == typeof(Rect))
            return EditorGUILayout.RectField(name, (Rect)value);
        if (fieldType == typeof(RectInt))
            return EditorGUILayout.RectIntField(name, (RectInt)value);
        if (fieldType == typeof(Bounds))
            return EditorGUILayout.BoundsField(name, (Bounds)value);
        if (fieldType == typeof(BoundsInt))
            return EditorGUILayout.BoundsIntField(name, (BoundsInt)value);
        if (fieldType == typeof(Color))
            return EditorGUILayout.ColorField(name, (Color)value);
        if (fieldType == typeof(Color32))
            return (Color32)EditorGUILayout.ColorField(name, (Color32)value);
        if (fieldType == typeof(AnimationCurve))
            return EditorGUILayout.CurveField(name, (AnimationCurve)value);
        if (fieldType == typeof(Gradient))
            return EditorGUILayout.GradientField(name, (Gradient)value);
        if (fieldType == typeof(LayerMask))
        {
            int layer = ((LayerMask)value).value;
            layer = EditorGUILayout.LayerField(name, layer);
            return (LayerMask)layer;
        }

        // Handle Dictionary types
        if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            return DrawDictionaryField(name, value, fieldType);
        }

        // Handle HashSet types
        if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(HashSet<>))
        {
            return DrawHashSetField(name, value, fieldType);
        }

        // Handle Queue and Stack
        if (fieldType.IsGenericType &&
            (fieldType.GetGenericTypeDefinition() == typeof(Queue<>) ||
             fieldType.GetGenericTypeDefinition() == typeof(Stack<>)))
        {
            return DrawCollectionField(name, value, fieldType);
        }

        // Handle arrays and lists
        if (fieldType.IsArray || (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>)))
        {
            return DrawCollectionField(name, value, fieldType);
        }

        // Handle Tuple types
        if (fieldType.IsGenericType && fieldType.Name.StartsWith("Tuple`"))
        {
            return DrawTupleField(name, value, fieldType);
        }

        // Handle DateTime
        if (fieldType == typeof(DateTime))
        {
            DateTime dateValue = (DateTime)value;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name);
            string newDate = EditorGUILayout.TextField(dateValue.ToString("yyyy-MM-dd HH:mm:ss"));
            EditorGUILayout.EndHorizontal();
            if (DateTime.TryParse(newDate, out DateTime result))
                return result;
            return dateValue;
        }

        // Handle TimeSpan
        if (fieldType == typeof(TimeSpan))
        {
            TimeSpan timeSpan = (TimeSpan)value;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name);
            int hours = EditorGUILayout.IntField(timeSpan.Hours, GUILayout.Width(30));
            EditorGUILayout.LabelField("h", GUILayout.Width(15));
            int minutes = EditorGUILayout.IntField(timeSpan.Minutes, GUILayout.Width(30));
            EditorGUILayout.LabelField("m", GUILayout.Width(15));
            int seconds = EditorGUILayout.IntField(timeSpan.Seconds, GUILayout.Width(30));
            EditorGUILayout.LabelField("s", GUILayout.Width(15));
            EditorGUILayout.EndHorizontal();
            return new TimeSpan(hours, minutes, seconds);
        }

        // Handle custom serializable classes
        if (fieldType.IsClass && !typeof(UnityEngine.Object).IsAssignableFrom(fieldType) &&
            !fieldType.IsPrimitive && fieldType != typeof(string))
        {
            return DrawCustomClassField(name, value, fieldType);
        }

        // If we don't have a specific handler, just return the original value
        EditorGUILayout.LabelField(name, value?.ToString() ?? "null");
        return value;
    }
    private object DrawDictionaryField(string name, object value, Type fieldType)
    {
        var dictionary = value as IDictionary;
        Type keyType = fieldType.GetGenericArguments()[0];
        Type valueType = fieldType.GetGenericArguments()[1];

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(name, EditorStyles.boldLabel);

        if (dictionary != null)
        {
            EditorGUI.indentLevel++;
            var keysToRemove = new List<object>();
            object newKey = null;
            object newValue = null;

            foreach (DictionaryEntry entry in dictionary)
            {
                EditorGUILayout.BeginHorizontal();
                var newKeyValue = DrawField("Key", entry.Key, keyType);
                var newEntryValue = DrawField("Value", entry.Value, valueType);

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    keysToRemove.Add(entry.Key);
                }
                EditorGUILayout.EndHorizontal();

                if (!Equals(newKeyValue, entry.Key) || !Equals(newEntryValue, entry.Value))
                {
                    keysToRemove.Add(entry.Key);
                    newKey = newKeyValue;
                    newValue = newEntryValue;
                }
            }

            foreach (var key in keysToRemove)
            {
                dictionary.Remove(key);
                if (newKey != null)
                {
                    dictionary[newKey] = newValue;
                }
            }

            if (GUILayout.Button("Add Entry"))
            {
                dictionary.Add(
                    keyType.IsValueType ? Activator.CreateInstance(keyType) : null,
                    valueType.IsValueType ? Activator.CreateInstance(valueType) : null
                );
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
        return dictionary;
    }

    private object DrawHashSetField(string name, object value, Type fieldType)
    {
        var hashSet = value as IEnumerable;
        Type elementType = fieldType.GetGenericArguments()[0];
        var list = new List<object>(hashSet.Cast<object>());

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(name, EditorStyles.boldLabel);

        EditorGUI.indentLevel++;
        for (int i = 0; i < list.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            list[i] = DrawField($"Element {i}", list[i], elementType);

            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                list.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Element"))
        {
            list.Add(elementType.IsValueType ? Activator.CreateInstance(elementType) : null);
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.EndVertical();
        return Activator.CreateInstance(fieldType, new object[] { list });
    }

    private Matrix4x4 DrawMatrix4x4Field(string name, Matrix4x4 matrix)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(name, EditorStyles.boldLabel);

        for (int row = 0; row < 4; row++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int col = 0; col < 4; col++)
            {
                matrix[row, col] = EditorGUILayout.FloatField(matrix[row, col], GUILayout.Width(60));
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
        return matrix;
    }

    private object DrawCustomClassField(string name, object value, Type fieldType)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(name, EditorStyles.boldLabel);

        EditorGUI.indentLevel++;
        var fields = fieldType.GetFields(BindingFlags.Public | BindingFlags.Instance);
        var properties = fieldType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in fields)
        {
            var fieldValue = field.GetValue(value);
            var newValue = DrawField(field.Name, fieldValue, field.FieldType);
            if (!Equals(fieldValue, newValue))
            {
                field.SetValue(value, newValue);
                GUI.changed = true;
            }
        }

        foreach (var property in properties)
        {
            if (property.CanRead && property.CanWrite)
            {
                var propertyValue = property.GetValue(value);
                var newValue = DrawField(property.Name, propertyValue, property.PropertyType);
                if (!Equals(propertyValue, newValue))
                {
                    property.SetValue(value, newValue);
                    GUI.changed = true;
                }
            }
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
        return value;
    }
    private object DrawCollectionField(string name, object value, Type fieldType)
    {
        bool isArray = fieldType.IsArray;
        Type elementType = isArray ? fieldType.GetElementType() : fieldType.GetGenericArguments()[0];
        IList list = (IList)value;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(name, EditorStyles.boldLabel);

        // Add button for lists
        if (!isArray && GUILayout.Button("+", GUILayout.Width(20)))
        {
            var newItem = elementType.IsValueType ? Activator.CreateInstance(elementType) : null;
            list.Add(newItem);
        }
        EditorGUILayout.EndHorizontal();

        if (list != null)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                list[i] = DrawField($"Element {i}", list[i], elementType);

                // Remove button for lists
                if (!isArray && GUILayout.Button("-", GUILayout.Width(20)))
                {
                    list.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        return list;
    }
    private object DrawTupleField(string name, object value, Type fieldType)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(name, EditorStyles.boldLabel);

        Type[] genericArgs = fieldType.GetGenericArguments();
        var tupleFields = fieldType.GetFields();

        object[] newValues = new object[genericArgs.Length];
        bool valueChanged = false;

        EditorGUI.indentLevel++;
        for (int i = 0; i < genericArgs.Length; i++)
        {
            string itemName = $"Item{i + 1}";
            var fieldValue = tupleFields[i].GetValue(value);
            var newValue = DrawField(itemName, fieldValue, genericArgs[i]);

            if (!Equals(fieldValue, newValue))
            {
                valueChanged = true;
            }
            newValues[i] = newValue;
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.EndVertical();

        if (valueChanged)
        {
            return Activator.CreateInstance(fieldType, newValues);
        }
        return value;
    }

    // Add these basic type handlers to the DrawField method before the "Fall back to existing handling" section:
    private object DrawBasicTypes(string name, object value, Type fieldType)
    {
        // Handle enums
        if (fieldType.IsEnum)
        {
            return EditorGUILayout.EnumPopup(name, (Enum)value);
        }

        // Basic types with min-max sliders for numeric types
        if (fieldType == typeof(float))
        {
            var rangeAttribute = GetRangeAttribute(name, fieldType);
            if (rangeAttribute != null)
                return EditorGUILayout.Slider(name, (float)value, rangeAttribute.min, rangeAttribute.max);
            return EditorGUILayout.FloatField(name, (float)value);
        }
        if (fieldType == typeof(int))
        {
            var rangeAttribute = GetRangeAttribute(name, fieldType);
            if (rangeAttribute != null)
                return EditorGUILayout.IntSlider(name, (int)value, (int)rangeAttribute.min, (int)rangeAttribute.max);
            return EditorGUILayout.IntField(name, (int)value);
        }
        if (fieldType == typeof(double))
            return EditorGUILayout.DoubleField(name, (double)value);
        if (fieldType == typeof(long))
            return EditorGUILayout.LongField(name, (long)value);
        if (fieldType == typeof(string))
            return EditorGUILayout.TextField(name, (string)value);
        if (fieldType == typeof(bool))
            return EditorGUILayout.Toggle(name, (bool)value);

        // Unity specific types
        if (fieldType == typeof(Vector2))
            return EditorGUILayout.Vector2Field(name, (Vector2)value);
        if (fieldType == typeof(Vector2Int))
            return EditorGUILayout.Vector2IntField(name, (Vector2Int)value);
        if (fieldType == typeof(Vector3))
            return EditorGUILayout.Vector3Field(name, (Vector3)value);
        if (fieldType == typeof(Vector3Int))
            return EditorGUILayout.Vector3IntField(name, (Vector3Int)value);
        if (fieldType == typeof(Vector4))
            return EditorGUILayout.Vector4Field(name, (Vector4)value);
        if (fieldType == typeof(Rect))
            return EditorGUILayout.RectField(name, (Rect)value);
        if (fieldType == typeof(RectInt))
            return EditorGUILayout.RectIntField(name, (RectInt)value);
        if (fieldType == typeof(Bounds))
            return EditorGUILayout.BoundsField(name, (Bounds)value);
        if (fieldType == typeof(BoundsInt))
            return EditorGUILayout.BoundsIntField(name, (BoundsInt)value);
        if (fieldType == typeof(Color))
            return EditorGUILayout.ColorField(name, (Color)value);
        if (fieldType == typeof(Color32))
            return (Color32)EditorGUILayout.ColorField(name, (Color32)value);
        if (fieldType == typeof(AnimationCurve))
            return EditorGUILayout.CurveField(name, (AnimationCurve)value);
        if (fieldType == typeof(Gradient))
            return EditorGUILayout.GradientField(name, (Gradient)value);
        if (fieldType == typeof(LayerMask))
        {
            int layer = ((LayerMask)value).value;
            layer = EditorGUILayout.LayerField(name, layer);
            return (LayerMask)layer;
        }

        return value;
    }
    private RangeAttribute GetRangeAttribute(string fieldName, Type type)
    {
        var field = type.GetField(fieldName);
        if (field != null)
        {
            return field.GetCustomAttributes(typeof(RangeAttribute), true)
                .FirstOrDefault() as RangeAttribute;
        }
        return null;
    }
}