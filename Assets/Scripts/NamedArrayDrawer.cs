#if UNITY_EDITOR
using UnityEditor;
#endif 
using UnityEngine;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(NamedArrayAttribute))]
public class NamedArrayDrawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        try
        {
            // �v���p�e�B�p�X����͂��ăC���f�b�N�X���擾
            string[] splitPath = property.propertyPath.Split(new char[] { '[', ']' });
            if (splitPath.Length > 1 && int.TryParse(splitPath[1], out int pos))
            {
                var namedArray = (NamedArrayAttribute)attribute;

                // �z��͈̔͂��m�F���ă��x����ݒ�
                string elementName = (pos < namedArray.names.Length) ? namedArray.names[pos] : $"Element {pos}";

                // ���x����K�p���ăv���p�e�B��`��
                EditorGUI.PropertyField(rect, property, new GUIContent(elementName), true);
            }
            else
            {
                EditorGUI.PropertyField(rect, property, label, true);
            }
        }
        catch
        {
            EditorGUI.PropertyField(rect, property, label, true);
        }
    }

    // �����𒲐����Đ܂肽���݂��l��
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
#endif
