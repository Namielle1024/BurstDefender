using UnityEngine;
using UnityEngine.UI;

public class RainbowText : MonoBehaviour
{
    private Text textComponent;

    void Start()
    {
        // Textコンポーネントを取得
        textComponent = GetComponent<Text>();
        if (textComponent == null)
        {
            Debug.LogError("テキストがコンポーネントされているオブジェクトが見つかりませんでした");
        }
    }

    void Update()
    {
        if (textComponent != null)
        {
            // 色を虹色に変化させる
            textComponent.color = GetRainbowColor(Time.time);
        }
    }

    Color GetRainbowColor(float time)
    {
        // 時間に基づいて色を計算
        float r = Mathf.Sin(time * 2f) * 0.5f + 0.5f;
        float g = Mathf.Sin(time * 2f + 2f * Mathf.PI / 3f) * 0.5f + 0.5f;
        float b = Mathf.Sin(time * 2f + 4f * Mathf.PI / 3f) * 0.5f + 0.5f;
        return new Color(r, g, b);
    }
}
