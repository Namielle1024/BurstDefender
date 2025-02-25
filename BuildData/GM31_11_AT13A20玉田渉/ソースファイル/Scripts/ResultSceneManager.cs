using UnityEngine;
using UnityEngine.UI;

public class ResultSceneManager : MonoBehaviour
{
    [SerializeField] 
    Text resultText;
    [SerializeField] 
    Button returnToTitleButton;

    void Start()
    {
        // �N���A�܂��̓Q�[���I�[�o�[�̌��ʂ�\��
        if (GameManager.Instance.GetGameResult() == GameManager.GameResult.Cleared)
        {
            resultText.text = "�X�e�[�W�N���A�I";
        }
        else if (GameManager.Instance.GetGameResult() == GameManager.GameResult.GameOver)
        {
            resultText.text = "�Q�[���I�[�o�[...";
        }
        else
        {
            resultText.text = "No Context";
        }

        returnToTitleButton.onClick.AddListener(ReturnToTitle);
    }

    private void ReturnToTitle()
    {
        GameManager.Instance.ReturnToTitle();
    }
}
