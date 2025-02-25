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
        // クリアまたはゲームオーバーの結果を表示
        if (GameManager.Instance.GetGameResult() == GameManager.GameResult.Cleared)
        {
            resultText.text = "ステージクリア！";
        }
        else if (GameManager.Instance.GetGameResult() == GameManager.GameResult.GameOver)
        {
            resultText.text = "ゲームオーバー...";
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
