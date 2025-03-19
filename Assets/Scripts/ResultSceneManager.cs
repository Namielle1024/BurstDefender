using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ResultSceneManager : MonoBehaviour
{
    // リザルトのテキスト表示
    [SerializeField]
    Text resultText;

    // タイトルに戻るボタン
    [SerializeField]
    Button returnToTitleButton;

    // リザルト画面の動画プレイヤー
    [SerializeField]
    VideoPlayer videoPlayer;

    // ゲームクリア時の動画
    [SerializeField]
    VideoClip gameClearVideo;

    // ゲームオーバー時の動画
    [SerializeField]
    VideoClip gameOverVideo;

    void Start()
    {
        // クリアまたはゲームオーバーの結果を表示
        if (GameManager.Instance.GetGameResult() == GameManager.GameResult.Cleared)
        {
            resultText.text = "STAGE CLEAR!";
            PlayResultVideo(gameClearVideo);
        }
        else if (GameManager.Instance.GetGameResult() == GameManager.GameResult.GameOver)
        {
            resultText.text = "GAME OVER...";
            resultText.color = Color.red;
            PlayResultVideo(gameOverVideo);
        }
        else
        {
            resultText.text = "No Context";
        }

        returnToTitleButton.onClick.AddListener(ReturnToTitle);
    }

    void Update()
    {
        if (GameManager.Instance.GetGameResult() == GameManager.GameResult.Cleared)
        {
            resultText.color = GetRainbowColor(Time.time);
        }
    }

    /// <summary>
    /// タイトルシーンへ戻る
    /// </summary>
    private void ReturnToTitle()
    {
        GameManager.Instance.ReturnToTitle();
    }

    /// <summary>
    /// 結果に応じた動画を再生する
    /// </summary>
    /// <param name="clip">再生する動画クリップ</param>
    void PlayResultVideo(VideoClip clip)
    {
        if (clip != null)
        {
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = clip;
            videoPlayer.Play();
        }
        else
        {
            Debug.LogError("動画が設定されていません！");
        }
    }

    /// <summary>
    /// 時間に基づいてレインボーの色を計算する
    /// </summary>
    /// <param name="time">現在の時間</param>
    /// <returns>時間に応じた色</returns>
    Color GetRainbowColor(float time)
    {
        // 時間に基づいて色を計算
        float r = Mathf.Sin(time * 2f) * 0.5f + 0.5f;
        float g = Mathf.Sin(time * 2f + 2f * Mathf.PI / 3f) * 0.5f + 0.5f;
        float b = Mathf.Sin(time * 2f + 4f * Mathf.PI / 3f) * 0.5f + 0.5f;
        return new Color(r, g, b);
    }
}
