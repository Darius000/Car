using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Reflection;

[CustomPropertyDrawer(typeof(SerializableDictionaryBase), true)]
internal class SerializableDictionaryPropertyDrawer : PropertyDrawer
{
    const string KeyFieldName = "m_Keys";
    const string ValueFieldName = "m_Values";
    protected const float IndentWidth = 15f;

    static GUIContent s_IconPlus = IconContent("Toolbar Plus", "Add Entry");
    static GUIContent s_IconMinus = IconContent("Toolbar Minus", "Remove Entry");
    static GUIContent s_WarningIconConflict = IconContent("console.warnicon.sml", "Conflicting Key, this entry will be lost");
    static GUIContent s_WarningIconOther = IconContent("console.infoicon.sml", "Conflicting Key");
    static GUIContent s_WarningIconNull = IconContent("console.warnicon.sml", "Null Key, this entry will be lost");
    static GUIStyle s_ButtonStyle = GUIStyle.none;
    static GUIContent s_TempContent = new GUIContent();

    class ConflictState
    {
        public object conflictKey = null;
        public object conflictValue = null;
        public int conflictIndex = -1;
        public int conflictOtherIndex = -1;
        public bool conflictKeyPropertyExpanded = false;
        public bool conflictValuePropertyExpanded = false;
        public float conflictLineHeight = 0f;
    }

    struct PropertyIdentity
    {
        public PropertyIdentity(SerializedProperty property)
        {
            instance = property.serializedObject.targetObject;
            propertyPath = property.propertyPath;
        }

        public UnityEngine.Object instance;
        public string propertyPath;
    }

    static Dictionary<PropertyIdentity, ConflictState> s_conflictStateDict = new Dictionary<PropertyIdentity, ConflictState>();

    enum Action
    {
        None,
        Add,
        Remove
    }



    static Dictionary<SerializedPropertyType, PropertyInfo> s_serializedPropertyValueAccessorDict;

    static SerializableDictionaryPropertyDrawer()
    {
        Dictionary<SerializedPropertyType, string> serializedPropertyValueAccessorNameDict = new Dictionary<SerializedPropertyType, string>()
        {
            {SerializedPropertyType.Integer, "intValue" },
            {SerializedPropertyType.Boolean, "boolValue" },
            {SerializedPropertyType.Float, "floatValue" },
            {SerializedPropertyType.String, "stringValue" },
            {SerializedPropertyType.Color, "colorValue" },
            { SerializedPropertyType.ObjectReference, "objectReferenceValue" },
            { SerializedPropertyType.LayerMask, "intValue" },
            { SerializedPropertyType.Enum, "intValue" },
            { SerializedPropertyType.Vector2, "vector2Value" },
            { SerializedPropertyType.Vector3, "vector3Value" },
            { SerializedPropertyType.Vector4, "vector4Value" },
            { SerializedPropertyType.Rect, "rectValue" },
            { SerializedPropertyType.ArraySize, "intValue" },
            { SerializedPropertyType.Character, "intValue" },
            { SerializedPropertyType.AnimationCurve, "animationCurveValue" },
            { SerializedPropertyType.Bounds, "boundsValue" },
            { SerializedPropertyType.Quaternion, "quaternionValue" },
        };

        Type serializedPropertyType = typeof(SerializedProperty);

        s_serializedPropertyValueAccessorDict = new Dictionary<SerializedPropertyType, PropertyInfo>();
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

        foreach(var kvp in serializedPropertyValueAccessorNameDict)
        {
            PropertyInfo propertyInfo = serializedPropertyType.GetProperty(kvp.Value, flags);
            s_serializedPropertyValueAccessorDict.Add(kvp.Key, propertyInfo);
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);

        Action buttonAction = Action.None;
        int buttonActionIndex = 0;

        var keyArrayProperty = property.FindPropertyRelative(KeyFieldName);
        var valueArrayProperty = property.FindPropertyRelative(ValueFieldName);

        ConflictState conflictState = GetConflictState(property);

        if (conflictState.conflictIndex != -1)
        {
            keyArrayProperty.InsertArrayElementAtIndex(conflictState.conflictIndex);
            var keyProperty = keyArrayProperty.GetArrayElementAtIndex(conflictState.conflictIndex);
            SetPropertyValue(keyProperty, conflictState.conflictKey);
            keyProperty.isExpanded = conflictState.conflictKeyPropertyExpanded;

            if (valueArrayProperty != null)
            {
                valueArrayProperty.InsertArrayElementAtIndex(conflictState.conflictIndex);
                var valueProperty = valueArrayProperty.GetArrayElementAtIndex(conflictState.conflictIndex);
                SetPropertyValue(valueProperty, conflictState.conflictValue);
                valueProperty.isExpanded = conflictState.conflictValuePropertyExpanded;
            }
        }

        var buttonWidth = s_ButtonStyle.CalcSize(s_IconPlus).x;

        var labelPosition = position;
        labelPosition.height = EditorGUIUtility.singleLineHeight;
        if (property.isExpanded)
            labelPosition.xMax -= s_ButtonStyle.CalcSize(s_IconPlus).x;

        EditorGUI.PropertyField(labelPosition, property, label, false);
        // property.isExpanded = EditorGUI.Foldout(labelPosition, property.isExpanded, label);
        if (property.isExpanded)
        {
            var buttonPosition = position;
            buttonPosition.xMin = buttonPosition.xMax - buttonWidth;
            buttonPosition.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginDisabledGroup(conflictState.conflictIndex != -1);
            if (GUI.Button(buttonPosition, s_IconPlus, s_ButtonStyle))
            {
                buttonAction = Action.Add;
                buttonActionIndex = keyArrayProperty.arraySize;
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.indentLevel++;
            var linePosition = position;
            linePosition.y += EditorGUIUtility.singleLineHeight;
            linePosition.xMax -= buttonWidth;

            foreach (var entry in EnumerateEntries(keyArrayProperty, valueArrayProperty))
            {
                var keyProperty = entry.keyProperty;
                var valueProperty = entry.valueProperty;
                int i = entry.index;

                float lineHeight = DrawKeyValueLine(keyProperty, valueProperty, linePosition, i);

                buttonPosition = linePosition;
                buttonPosition.x = linePosition.xMax;
                buttonPosition.height = EditorGUIUtility.singleLineHeight;
                if (GUI.Button(buttonPosition, s_IconMinus, s_ButtonStyle))
                {
                    buttonAction = Action.Remove;
                    buttonActionIndex = i;
                }

                if (i == conflictState.conflictIndex && conflictState.conflictOtherIndex == -1)
                {
                    var iconPosition = linePosition;
                    iconPosition.size = s_ButtonStyle.CalcSize(s_WarningIconNull);
                    GUI.Label(iconPosition, s_WarningIconNull);
                }
                else if (i == conflictState.conflictIndex)
                {
                    var iconPosition = linePosition;
                    iconPosition.size = s_ButtonStyle.CalcSize(s_WarningIconConflict);
                    GUI.Label(iconPosition, s_WarningIconConflict);
                }
                else if (i == conflictState.conflictOtherIndex)
                {
                    var iconPosition = linePosition;
                    iconPosition.size = s_ButtonStyle.CalcSize(s_WarningIconOther);
                    GUI.Label(iconPosition, s_WarningIconOther);
                }


                linePosition.y += lineHeight;
            }

            EditorGUI.indentLevel--;
        }

        if (buttonAction == Action.Add)
        {
            keyArrayProperty.InsertArrayElementAtIndex(buttonActionIndex);
            if (valueArrayProperty != null)
                valueArrayProperty.InsertArrayElementAtIndex(buttonActionIndex);
        }
        else if (buttonAction == Action.Remove)
        {
            DeleteArrayElementAtIndex(keyArrayProperty, buttonActionIndex);
            if (valueArrayProperty != null)
                DeleteArrayElementAtIndex(valueArrayProperty, buttonActionIndex);
        }

        conflictState.conflictKey = null;
        conflictState.conflictValue = null;
        conflictState.conflictIndex = -1;
        conflictState.conflictOtherIndex = -1;
        conflictState.conflictLineHeight = 0f;
        conflictState.conflictKeyPropertyExpanded = false;
        conflictState.conflictValuePropertyExpanded = false;

        foreach (var entry1 in EnumerateEntries(keyArrayProperty, valueArrayProperty))
        {
            var keyProperty1 = entry1.keyProperty;
            int i = entry1.index;
            object keyProperty1Value = GetPropertyValue(keyProperty1);

            if (keyProperty1Value == null)
            {
                var valueProperty1 = entry1.valueProperty;
                SaveProperty(keyProperty1, valueProperty1, i, -1, conflictState);
                DeleteArrayElementAtIndex(keyArrayProperty, i);
                if (valueArrayProperty != null)
                    DeleteArrayElementAtIndex(valueArrayProperty, i);

                break;
            }


            foreach (var entry2 in EnumerateEntries(keyArrayProperty, valueArrayProperty, i + 1))
            {
                var keyProperty2 = entry2.keyProperty;
                int j = entry2.index;
                object keyProperty2Value = GetPropertyValue(keyProperty2);

                if (ComparePropertyValues(keyProperty1Value, keyProperty2Value))
                {
                    var valueProperty2 = entry2.valueProperty;
                    SaveProperty(keyProperty2, valueProperty2, j, i, conflictState);
                    DeleteArrayElementAtIndex(keyArrayProperty, j);
                    if (valueArrayProperty != null)
                        DeleteArrayElementAtIndex(valueArrayProperty, j);

                    goto breakLoops;
                }
            }
        }
    breakLoops:

        EditorGUI.EndProperty();
    }

    static float DrawKeyValueLine(SerializedProperty keyProperty, SerializedProperty valueProperty, Rect linePosition, int index)
    {
        bool keyCanBeExpanded = CanPropertyBeExpanded(keyProperty);

        if (valueProperty != null)
        {
            bool valueCanBeExpanded = CanPropertyBeExpanded(valueProperty);

            if (!keyCanBeExpanded && valueCanBeExpanded)
            {
                return DrawKeyValueLineExpand(keyProperty, valueProperty, linePosition);
            }
            else
            {
                var keyLabel = keyCanBeExpanded ? ("Key " + index.ToString()) : "";
                var valueLabel = valueCanBeExpanded ? ("Value " + index.ToString()) : "";
                return DrawKeyValueLineSimple(keyProperty, valueProperty, keyLabel, valueLabel, linePosition);
            }
        }
        else
        {
            if (!keyCanBeExpanded)
            {
                return DrawKeyLine(keyProperty, linePosition, null);
            }
            else
            {
                var keyLabel = string.Format("{0} {1}", ObjectNames.NicifyVariableName(keyProperty.type), index);
                return DrawKeyLine(keyProperty, linePosition, keyLabel);
            }
        }
    }


    static float DrawKeyValueLineSimple(SerializedProperty keyProperty, SerializedProperty valueProperty, string keyLabel, string valueLabel, Rect linePosition)
    {
        float labelWidth = EditorGUIUtility.labelWidth;
        float labelWidthRelative = labelWidth / linePosition.width;

        float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
        var keyPosition = linePosition;
        keyPosition.height = keyPropertyHeight;
        keyPosition.width = labelWidth - IndentWidth;
        EditorGUIUtility.labelWidth = keyPosition.width * labelWidthRelative;
        EditorGUI.PropertyField(keyPosition, keyProperty, TempContent(keyLabel), true);

        float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
        var valuePosition = linePosition;
        valuePosition.height = valuePropertyHeight;
        valuePosition.xMin += labelWidth;
        EditorGUIUtility.labelWidth = valuePosition.width * labelWidthRelative;
        EditorGUI.indentLevel--;
        EditorGUI.PropertyField(valuePosition, valueProperty, TempContent(valueLabel), true);
        EditorGUI.indentLevel++;

        EditorGUIUtility.labelWidth = labelWidth;

        return Mathf.Max(keyPropertyHeight, valuePropertyHeight);
    }

    static float DrawKeyValueLineExpand(SerializedProperty keyProperty, SerializedProperty valueProperty, Rect linePosition)
    {
        float labelWidth = EditorGUIUtility.labelWidth;

        float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
        var keyPosition = linePosition;
        keyPosition.height = keyPropertyHeight;
        keyPosition.width = labelWidth - IndentWidth;
        EditorGUI.PropertyField(keyPosition, keyProperty, GUIContent.none, true);

        float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
        var valuePosition = linePosition;
        valuePosition.height = valuePropertyHeight;
        EditorGUI.PropertyField(valuePosition, valueProperty, GUIContent.none, true);

        EditorGUIUtility.labelWidth = labelWidth;

        return Mathf.Max(keyPropertyHeight, valuePropertyHeight);
    }

    static float DrawKeyLine(SerializedProperty keyProperty, Rect linePosition, string keyLabel)
    {
        float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
        var keyPosition = linePosition;
        keyPosition.height = keyPropertyHeight;
        keyPosition.width = linePosition.width;

        var keyLabelContent = keyLabel != null ? TempContent(keyLabel) : GUIContent.none;
        EditorGUI.PropertyField(keyPosition, keyProperty, keyLabelContent, true);

        return keyPropertyHeight;
    }

    static bool CanPropertyBeExpanded(SerializedProperty property)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.Generic:
            case SerializedPropertyType.Vector4:
            case SerializedPropertyType.Quaternion:
                return true;
            default:
                return false;
        }
    }

    static void SaveProperty(SerializedProperty keyProperty, SerializedProperty valueProperty, int index, int otherIndex, ConflictState conflictState)
    {
        conflictState.conflictKey = GetPropertyValue(keyProperty);
        conflictState.conflictValue = valueProperty != null ? GetPropertyValue(valueProperty) : null;
        float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
        float valuePropertyHeight = valueProperty != null ? EditorGUI.GetPropertyHeight(valueProperty) : 0f;
        float lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
        conflictState.conflictLineHeight = lineHeight;
        conflictState.conflictIndex = index;
        conflictState.conflictOtherIndex = otherIndex;
        conflictState.conflictKeyPropertyExpanded = keyProperty.isExpanded;
        conflictState.conflictValuePropertyExpanded = valueProperty != null ? valueProperty.isExpanded : false;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float propertyHeight = EditorGUIUtility.singleLineHeight;

        if (property.isExpanded)
        {
            var keysProperty = property.FindPropertyRelative(KeyFieldName);
            var valuesProperty = property.FindPropertyRelative(ValueFieldName);

            foreach (var entry in EnumerateEntries(keysProperty, valuesProperty))
            {
                var keyProperty = entry.keyProperty;
                var valueProperty = entry.valueProperty;
                float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
                float valuePropertyHeight = valueProperty != null ? EditorGUI.GetPropertyHeight(valueProperty) : 0f;
                float lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
                propertyHeight += lineHeight;
            }

            ConflictState conflictState = GetConflictState(property);

            if (conflictState.conflictIndex != -1)
            {
                propertyHeight += conflictState.conflictLineHeight;
            }
        }

        return propertyHeight;
    }

    static ConflictState GetConflictState(SerializedProperty property)
    {
        ConflictState conflictState;
        PropertyIdentity propId = new PropertyIdentity(property);
        if (!s_conflictStateDict.TryGetValue(propId, out conflictState))
        {
            conflictState = new ConflictState();
            s_conflictStateDict.Add(propId, conflictState);
        }
        return conflictState;
    }

    static GUIContent IconContent(string name, string tooltip)
    {
        var icon = EditorGUIUtility.IconContent(name);
        return new GUIContent(icon.image, tooltip);
    }

    static GUIContent TempContent(string text)
    {
        s_TempContent.text = text;
        return s_TempContent;
    }

    static void DeleteArrayElementAtIndex(SerializedProperty arrayproperty, int index)
    {
        var property = arrayproperty.GetArrayElementAtIndex(index);

        if(property.propertyType == SerializedPropertyType.ObjectReference)
        {
            property.objectReferenceValue = null;
        }

        arrayproperty.DeleteArrayElementAtIndex(index);
    }

    public static object GetPropertyValue(SerializedProperty property)
    {
        PropertyInfo propertyInfo;
        if(s_serializedPropertyValueAccessorDict.TryGetValue(property.propertyType, out propertyInfo))
        {
            return propertyInfo.GetValue(property, null);
        }
        else
        {
            if(property.isArray)
            {
                return GetPropertyValueArray(property);
            }
            else
            {
                return GetPropertyValueGeneric(property);
            }
        }
    }

    static object GetPropertyValueArray(SerializedProperty property)
    {
        object[] array = new object[property.arraySize];
        for (int i = 0; i < property.arraySize; i++)
        {
            SerializedProperty item = property.GetArrayElementAtIndex(i);
            array[i] = GetPropertyValue(item);
        }
        return array;
    }

    static object GetPropertyValueGeneric(SerializedProperty property)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        var iterator = property.Copy();
        if (iterator.Next(true))
        {
            var end = property.GetEndProperty();
            do
            {
                string name = iterator.name;
                object value = GetPropertyValue(iterator);
                dict.Add(name, value);
            } while (iterator.Next(false) && iterator.propertyPath != end.propertyPath);
        }
        return dict;
    }

    static void SetPropertyValue(SerializedProperty property, object v)
    {
        PropertyInfo propertyInfo;
        if(s_serializedPropertyValueAccessorDict.TryGetValue(property.propertyType, out propertyInfo))
        {
            propertyInfo.SetValue(property, v, null);
        }
        else
        {
            if(property.isArray)
            {
                SetPropertyValueArray(property, v);
            }
            else
            {
                SetPropertyValueGeneric(property, v);
            }
        }
    }

    static void SetPropertyValueArray(SerializedProperty property, object v)
    {
        object[] array = (object[])v;
        property.arraySize = array.Length;
        for(int i = 0; i < property.arraySize; i++)
        {
            SerializedProperty item = property.GetArrayElementAtIndex(i);
            SetPropertyValue(item, array[i]);
        }
    }

    static void SetPropertyValueGeneric(SerializedProperty property, object v)
    {
        Dictionary<string, object> dict = (Dictionary<string, object>)v;
        var iterator = property.Copy();
        if(iterator.Next(true))
        {
            var end = property.GetEndProperty();
            do
            {
                string name = iterator.name;
                SetPropertyValue(iterator, dict[name]);
            }
            while(iterator.Next(false) && iterator.propertyPath != end.propertyPath);
        }
    }

    static bool ComparePropertyValues(object a, object b)
    {
        if(a is Dictionary<string, object> && b is Dictionary<string, object>)
        {
            var dict1 = (Dictionary<string, object>)a;
            var dict2 = (Dictionary<string, object>)b;
            return CompareDictionaries(dict1, dict2);
        }
        else
        {
            return object.Equals(a, b);
        }
    }

    static bool CompareDictionaries(Dictionary<string, object> a, Dictionary<string , object> b)
    {
        if (a.Count != b.Count) return false;

        foreach (KeyValuePair<string , object> pair in a)
        {
            var key1 = pair.Key;
            object value1 = pair.Value;

            object value2;
            if(!b.TryGetValue(key1, out value2)) return false;

            if (!ComparePropertyValues(value1, value2)) return false;
        }

        return true;
    }

    struct EnumerationEntry
    {
        public SerializedProperty keyProperty;
        public SerializedProperty valueProperty;
        public int index;

        public EnumerationEntry(SerializedProperty keyproperty, SerializedProperty valueProperty, int index)
        {
            this.keyProperty = keyproperty;
            this.valueProperty = valueProperty;
            this.index = index;
        }
    }

    static IEnumerable<EnumerationEntry> EnumerateEntries(SerializedProperty keyArrayProperty, SerializedProperty valueArrayProperty, int startIndex = 0)
    {
        if(keyArrayProperty.arraySize > startIndex)
        {
            int index = startIndex;
            var keyProperty = keyArrayProperty.GetArrayElementAtIndex(startIndex);
            var valueProperty = valueArrayProperty != null ? valueArrayProperty.GetArrayElementAtIndex(index) : null;
            var endProperty = keyArrayProperty.GetEndProperty();

            do
            {
                yield return new EnumerationEntry(keyProperty, valueProperty, index);
                index++;
            }
            while (keyProperty.Next(false)
                && (valueProperty != null ? valueProperty.Next(false) : true)
                && !SerializedProperty.EqualContents(keyProperty, endProperty));

        }
    }
}

[CustomPropertyDrawer(typeof(SerializableDictionaryBase.Storage), true)]
public class SerializableDictionaryStoragePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property.Next(true);
        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        property.Next(true);
        return EditorGUI.GetPropertyHeight(property);
    }
}