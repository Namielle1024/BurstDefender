using UnityEngine;
using UnityEngine.UI;

public class RainbowButton : MonoBehaviour
{
    private Button buttonComponent;

    void Start()
    {
        // Textコンポーネントを取得
        buttonComponent = GetComponent<Button>();
        if (buttonComponent == null)
        {
            Debug.LogError("イメージがコンポーネントされているオブジェクトが見つかりませんでした");
        }
    }

    void Update()
    {
        if (buttonComponent != null)
        {
            // 色を虹色に変化させる
            buttonComponent.image.color = GetRainbowColor(Time.time);
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