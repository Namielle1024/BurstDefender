using UnityEngine;
using UnityEngine.UI;

public class RainbowImage : MonoBehaviour
{
    private Image imageComponent;

    void Start()
    {
        // Text�R���|�[�l���g���擾
        imageComponent = GetComponent<Image>();
        if (imageComponent == null)
        {
            Debug.LogError("�C���[�W���R���|�[�l���g����Ă���I�u�W�F�N�g��������܂���ł���");
        }
    }

    void Update()
    {
        if (imageComponent != null)
        {
            // �F����F�ɕω�������
            imageComponent.color = GetRainbowColor(Time.time);
        }
    }

    Color GetRainbowColor(float time)
    {
        // ���ԂɊ�Â��ĐF���v�Z
        float r = Mathf.Sin(time * 2f) * 0.5f + 0.5f;
        float g = Mathf.Sin(time * 2f + 2f * Mathf.PI / 3f) * 0.5f + 0.5f;
        float b = Mathf.Sin(time * 2f + 4f * Mathf.PI / 3f) * 0.5f + 0.5f;
        return new Color(r, g, b);
    }
}
