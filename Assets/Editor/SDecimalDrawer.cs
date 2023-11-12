using UnityEngine;
using UnityEditor;
using LongDistancePhysics;

[CustomPropertyDrawer(typeof(sdecimal))]
public class SerializableDecimalDrawer : PropertyDrawer
{
    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {
        var obj = property.serializedObject.targetObject;
        var inst = (sdecimal)this.fieldInfo.GetValue(obj);
        var fieldRect = EditorGUI.PrefixLabel(position, label);
        string text = GUI.TextField(fieldRect, inst.value.ToString());
        if (GUI.changed)
        {
            decimal val;
            if(decimal.TryParse(text, out val))
            {
                inst.value = val;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}