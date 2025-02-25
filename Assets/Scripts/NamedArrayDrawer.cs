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
            // プロパティパスを解析してインデックスを取得
            string[] splitPath = property.propertyPath.Split(new char[] { '[', ']' });
            if (splitPath.Length > 1 && int.TryParse(splitPath[1], out int pos))
            {
                var namedArray = (NamedArrayAttribute)attribute;

                // 配列の範囲を確認してラベルを設定
                string elementName = (pos < namedArray.names.Length) ? namedArray.names[pos] : $"Element {pos}";

                // ラベルを適用してプロパティを描画
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

    // 高さを調整して折りたたみを考慮
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
#endif
