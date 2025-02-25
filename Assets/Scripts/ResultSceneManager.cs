using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ResultSceneManager : MonoBehaviour
{
    [SerializeField] 
    Text resultText;
    [SerializeField] 
    Button returnToTitleButton;
    [SerializeField]
    VideoPlayer videoPlayer;
    [SerializeField]
    VideoClip gameClearVideo;
    [SerializeField]
    VideoClip gameOverVideo;

    void Start()
    {
        // �N���A�܂��̓Q�[���I�[�o�[�̌��ʂ�\��
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

    private void ReturnToTitle()
    {
        GameManager.Instance.ReturnToTitle();
    }

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
            Debug.LogError("���悪�ݒ肳��Ă��܂���I");
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
