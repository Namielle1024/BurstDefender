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

    //[Header("Audio Settings")]
    //[SerializeField] AudioSource bgmAudioSource;
    //[SerializeField] AudioSource seAudioSource;

    void Awake()
    {
        pauseUI.gameObject.SetActive(false);
    }

    void Start()
    {
        SetActiveGameUI(false);

        // ボタンイベントの設定
        resumeButton.onClick.AddListener(ResumeGame);
        titleButton.onClick.AddListener(ReturnToTitle);
        tutorialButton.onClick.AddListener(ToggleExplanationUI);
        confimButton.onClick.AddListener(ConfimTutorial);

        // スライダーの初期化
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        seSlider.onValueChanged.AddListener(SetSEVolume);
    }

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


    public void SetActiveGameUI(bool active)
    {
        if (active)
        {
            BackImage.gameObject.SetActive(true);
            currentWaveText.gameObject.SetActive(true);
            playerBackground.gameObject.SetActive(true);
            playerPortrait.gameObject.SetActive(true);
            playerHPBar.gameObject.SetActive(true);
            playerHPBarBack.gameObject.SetActive(true);
            enemyCountText.gameObject.SetActive(true);
            enemyCountGauge.gameObject.SetActive(true);
            enemyCountGaugeBack.gameObject.SetActive(true);
        }
        else
        {
            BackImage.gameObject.SetActive(false);
            waveImage.gameObject.SetActive(false);
            waveText.gameObject.SetActive(false);
            currentWaveText.gameObject.SetActive(false);
            playerBackground.gameObject.SetActive(false);
            playerPortrait.gameObject.SetActive(false);
            playerHPBar.gameObject.SetActive(false);
            playerHPBarBack.gameObject.SetActive(false);
            enemyCountText.gameObject.SetActive(false);
            enemyCountGauge.gameObject.SetActive(false);
            enemyCountGaugeBack.gameObject.SetActive(false);
        }
    }

    public void ShowWaveStart(int waveNumber)
    {
        if (waveText == null || waveImage == null) return;

        string waveLabel = waveNumber == 1 ? "1" : waveNumber == 2 ? "2" : "Last";
        waveText.text = $"Wave {waveLabel} Start!";
        waveImage.gameObject.SetActive(true);
        waveText.gameObject.SetActive(true);
        Invoke(nameof(HideWaveText), displayTime);
    }

    public void ShowWaveComplete(int waveNumber)
    {
        if (waveText == null || waveImage == null) return;

        string waveLabel = waveNumber == 1 ? "1" : waveNumber == 2 ? "2" : "Last";
        waveText.text = $"Wave {waveNumber} Complete!";
        waveImage.gameObject.SetActive(true);
        waveText.gameObject.SetActive(true);
        Invoke(nameof(HideWaveText), displayTime);
    }

    void HideWaveText()
    {
        if (waveText == null || waveImage == null) return;

        waveImage.gameObject.SetActive(false);
        waveText.gameObject.SetActive(false);
        SetActiveGameUI(true);
    }

    public void SetCurrentWave(int waveNumber)
    {
        if (currentWaveText == null) return;

        string waveLabel = waveNumber == 1 ? "1st" : waveNumber == 2 ? "2nd" : "Last";
        currentWaveText.text = $"{waveLabel} Wave";
    }

    // プレイヤーのHPを更新
    public void UpdateHPBar(float currentHP, float maxHP)
    {
        if (playerHPBar == null || playerHPBarBack == null) return;

        float fillAmount = currentHP / maxHP;
        playerHPBar.fillAmount = fillAmount;

        // HP50%以下なら背景を赤く
        playerBackground.color = fillAmount <= 0.5f ? new Color(1, 0, 0, 1) : new Color(1, 1, 1, 1);
    }

    // ダメージ時のエフェクト
    public void ShowDamageEffect()
    {
        StartCoroutine(DamageFlash());
    }

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

    // 敵の残数ゲージ更新
    public void UpdateEnemyGauge(int remaining, int total)
    {
        if (enemyCountText == null || enemyCountGauge == null) return;

        enemyCountText.text = $"Enemies";
        enemyCountGauge.fillAmount = 1 - ((float)remaining / total);
    }

    void ResumeGame()
    {
        GameManager.Instance.SetCursorState(false);
        UIManager.Instance.OnPause(false);
        Time.timeScale = 1;
    }

    void ReturnToTitle()
    {
        Time.timeScale = 1;
        GameManager.Instance.ReturnToTitle();
    }

    void SetBGMVolume(float volume)
    {
        //if (bgmAudioSource != null)
        //{
        //    bgmAudioSource.volume = volume;
        //}
    }

    void SetSEVolume(float volume)
    {
        //if (seAudioSource != null)
        //{
        //    seAudioSource.volume = volume;
        //}
    }

    void ToggleExplanationUI()
    {
        if (tutorialButton != null)
        {
            tutorialImage.SetActive(true);
        }
    }

    void ConfimTutorial()
    {
        tutorialImage.SetActive(false);
    }


    public bool IsPauseActive()
    {
        return pauseUI != null && pauseUI.activeSelf;
    }
}
