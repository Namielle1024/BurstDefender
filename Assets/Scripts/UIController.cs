using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIController : MonoBehaviour
{
    [SerializeField]
    GameObject inGameUI;
    [SerializeField]
    GameObject pauseUI;

    [Header("Wave UI")]
    [SerializeField] // Wave開始・終了時のUI
    Image waveImage;
    [SerializeField] // Wave開始・終了時のテキスト
    Text waveText;
    [SerializeField] // UI表示時間
    float displayTime = 3.0f;

    [Header("Current Wave UI")]
    [SerializeField] // 背景画像
    Image BackImage;
    [SerializeField] // 現在のWaveを表示するテキスト
    Text currentWaveText;

    [Header("Player UI")]
    [SerializeField] // プレイヤーの背景画像
    Image playerBackground;
    [SerializeField] // プレイヤーのポートレート画像
    Image playerPortrait;
    [SerializeField] // プレイヤーのHPバー
    Image playerHPBar;
    [SerializeField] // プレイヤーのHPバーの背景
    Image playerHPBarBack;
    [SerializeField] // ダメージ時の点滅時間
    float damageFlashDuration = 0.1f;

    [Header("Enemy Count UI")]
    [SerializeField] // 残敵数を表示するテキスト
    Text enemyCountText;
    [SerializeField] // 残敵数のゲージ
    Image enemyCountGauge;
    [SerializeField] // 残敵数のゲージの背景
    Image enemyCountGaugeBack;

    [Header("Pause UI")]
    [SerializeField]
    Button resumeButton;
    [SerializeField]
    Button titleButton;
    [SerializeField]
    Slider bgmSlider;
    [SerializeField]
    Slider seSlider;
    [SerializeField]
    Button tutorialButton;
    [SerializeField]
    GameObject tutorialImage;
    [SerializeField]
    Button confimButton;

    void Awake()
    {
        // 初期化時にポーズUIは非表示にしておく
        pauseUI.gameObject.SetActive(false);
    }

    void Start()
    {
        SetActiveGameUI(false);

        // 各ボタンのイベント登録
        resumeButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayClickSE();
            ResumeGame();
        });

        titleButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayClickSE();
            ReturnToTitle();
        });

        tutorialButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayClickSE();
            ToggleExplanationUI();
        });

        confimButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayClickSE();
            ConfimTutorial();
        });

        // スライダーに音量設定のイベントを登録
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        seSlider.onValueChanged.AddListener(SetSEVolume);
    }

    /// <summary>
    /// ポーズ状態を切り替える
    /// </summary>
    /// <param name="pause">true: ポーズ, false: ポーズ解除</param>
    public void OnPause(bool pause)
    {
        if (pause)
        {
            pauseUI.gameObject.SetActive(true);
            inGameUI.gameObject.SetActive(false);
            tutorialImage.gameObject.SetActive(false);
        }
        else
        {
            pauseUI.gameObject.SetActive(false);
            inGameUI.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// ゲーム中のUIを一括で表示/非表示する
    /// </summary>
    /// <param name="active">表示状態</param>
    public void SetActiveGameUI(bool active)
    {
        // 各UIコンポーネントの表示状態を切り替える
        BackImage.gameObject.SetActive(active);
        currentWaveText.gameObject.SetActive(active);
        playerBackground.gameObject.SetActive(active);
        playerPortrait.gameObject.SetActive(active);
        playerHPBar.gameObject.SetActive(active);
        playerHPBarBack.gameObject.SetActive(active);
        enemyCountText.gameObject.SetActive(active);
        enemyCountGauge.gameObject.SetActive(active);
        enemyCountGaugeBack.gameObject.SetActive(active);

        // Wave UIだけは明示的に非表示化
        if (!active)
        {
            waveImage.gameObject.SetActive(false);
            waveText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Wave開始演出を表示
    /// </summary>
    /// <param name="waveNumber">現在のWave番号</param>
    public void ShowWaveStart(int waveNumber)
    {
        if (waveText == null || waveImage == null) return;

        string waveLabel = waveNumber == 1 ? "1" : waveNumber == 2 ? "2" : "Last";
        waveText.text = $"Wave {waveLabel} Start!";
        waveImage.gameObject.SetActive(true);
        waveText.gameObject.SetActive(true);
        Invoke(nameof(HideWaveText), displayTime);
    }

    /// <summary>
    /// Wave終了演出を表示
    /// </summary>
    /// <param name="waveNumber">現在のWave番号</param>
    public void ShowWaveComplete(int waveNumber)
    {
        if (waveText == null || waveImage == null) return;

        string waveLabel = waveNumber == 1 ? "1" : waveNumber == 2 ? "2" : "Last";
        waveText.text = $"Wave {waveNumber} Complete!";
        waveImage.gameObject.SetActive(true);
        waveText.gameObject.SetActive(true);
        Invoke(nameof(HideWaveText), displayTime);
    }

    /// <summary>
    /// Wave表示UIを非表示にする
    /// </summary>
    void HideWaveText()
    {
        if (waveText == null || waveImage == null) return;

        waveImage.gameObject.SetActive(false);
        waveText.gameObject.SetActive(false);
        SetActiveGameUI(true);
    }

    /// <summary>
    /// 現在のWave数をテキストに表示
    /// </summary>
    /// <param name="waveNumber">現在のWave番号</param>
    public void SetCurrentWave(int waveNumber)
    {
        if (currentWaveText == null) return;

        string waveLabel = waveNumber == 1 ? "1st" : waveNumber == 2 ? "2nd" : "Last";
        currentWaveText.text = $"{waveLabel} Wave";
    }

    /// <summary>
    /// プレイヤーのHPバーを更新
    /// </summary>
    /// <param name="currentHP">現在のHP</param>
    /// <param name="maxHP">最大HP</param>
    public void UpdateHPBar(float currentHP, float maxHP)
    {
        if (playerHPBar == null || playerHPBarBack == null) return;

        float fillAmount = currentHP / maxHP;
        playerHPBar.fillAmount = fillAmount;

        // HP50%以下なら背景を赤く
        playerBackground.color = fillAmount <= 0.5f ? new Color(1, 0, 0, 1) : new Color(1, 1, 1, 1);
    }

    /// <summary>
    /// ダメージ時の点滅エフェクトを再生
    /// </summary>
    public void ShowDamageEffect()
    {
        StartCoroutine(DamageFlash());
    }

    /// <summary>
    /// プレイヤーアイコンを赤く点滅させるコルーチン
    /// </summary>
    IEnumerator DamageFlash()
    {
        for (int i = 0; i < 3; i++)
        {
            playerPortrait.material.color = new Color(1, 0, 0, 0.9f); // 赤色
            yield return new WaitForSeconds(damageFlashDuration);
            playerPortrait.material.color = Color.white; // 元に戻す
            yield return new WaitForSeconds(damageFlashDuration);
        }
    }

    /// <summary>
    /// 敵の残数ゲージを更新
    /// </summary>
    /// <param name="remaining">倒した数</param>
    /// <param name="total">合計数</param>
    public void UpdateEnemyGauge(int remaining, int total)
    {
        if (enemyCountText == null || enemyCountGauge == null) return;

        enemyCountText.text = $"Enemies";
        enemyCountGauge.fillAmount = 1 - ((float)remaining / total);
    }

    /// <summary>
    /// ポーズUI内「ゲームに戻る」ボタン処理
    /// </summary>
    void ResumeGame()
    {
        GameManager.Instance.SetCursorState(false);
        UIManager.Instance.OnPause(false);
        Time.timeScale = 1;
    }

    /// <summary>
    /// ポーズUI内「タイトルへ戻る」ボタン処理
    /// </summary>
    void ReturnToTitle()
    {
        Time.timeScale = 1;
        GameManager.Instance.ReturnToTitle();
    }

    /// <summary>
    /// BGM音量スライダーの設定
    /// </summary>
    /// <param name="volume">音量</param>
    void SetBGMVolume(float volume)
    {
        AudioManager.Instance.SetBGMVolume(volume);
    }


    /// <summary>
    /// SE音量スライダーの設定
    /// </summary>
    /// <param name="volume">音量</param>
    void SetSEVolume(float volume)
    {
        AudioManager.Instance.SetSEVolume(volume);
    }

    /// <summary>
    /// 操作説明UIの表示切り替え
    /// </summary>
    void ToggleExplanationUI()
    {
        if (tutorialButton != null)
        {
            tutorialImage.SetActive(true);
        }
    }

    /// <summary>
    /// 操作説明画面を閉じる
    /// </summary>
    void ConfimTutorial()
    {
        tutorialImage.SetActive(false);
    }

    /// <summary>
    /// ポーズUIが表示中かどうかを判定
    /// </summary>
    /// <returns>表示中ならtrue</returns>
    public bool IsPauseActive()
    {
        return pauseUI != null && pauseUI.activeSelf;
    }
}
