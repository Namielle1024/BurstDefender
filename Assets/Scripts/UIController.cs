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
    [SerializeField] // Wave�J�n�E�I������UI
    Image waveImage;
    [SerializeField] // Wave�J�n�E�I�����̃e�L�X�g
    Text waveText;
    [SerializeField] // UI�\������
    float displayTime = 3.0f;

    [Header("Current Wave UI")]
    [SerializeField] // �w�i�摜
    Image BackImage;
    [SerializeField] // ���݂�Wave��\������e�L�X�g
    Text currentWaveText;

    [Header("Player UI")]
    [SerializeField] // �v���C���[�̔w�i�摜
    Image playerBackground;
    [SerializeField] // �v���C���[�̃|�[�g���[�g�摜
    Image playerPortrait;
    [SerializeField] // �v���C���[��HP�o�[
    Image playerHPBar;
    [SerializeField] // �v���C���[��HP�o�[�̔w�i
    Image playerHPBarBack;
    [SerializeField] // �_���[�W���̓_�Ŏ���
    float damageFlashDuration = 0.1f;

    [Header("Enemy Count UI")]
    [SerializeField] // �c�G����\������e�L�X�g
    Text enemyCountText;
    [SerializeField] // �c�G���̃Q�[�W
    Image enemyCountGauge;
    [SerializeField] // �c�G���̃Q�[�W�̔w�i
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

        // �{�^���C�x���g�̐ݒ�
        resumeButton.onClick.AddListener(ResumeGame);
        titleButton.onClick.AddListener(ReturnToTitle);
        tutorialButton.onClick.AddListener(ToggleExplanationUI);
        confimButton.onClick.AddListener(ConfimTutorial);

        // �X���C�_�[�̏�����
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

    // �v���C���[��HP���X�V
    public void UpdateHPBar(float currentHP, float maxHP)
    {
        if (playerHPBar == null || playerHPBarBack == null) return;

        float fillAmount = currentHP / maxHP;
        playerHPBar.fillAmount = fillAmount;

        // HP50%�ȉ��Ȃ�w�i��Ԃ�
        playerBackground.color = fillAmount <= 0.5f ? new Color(1, 0, 0, 1) : new Color(1, 1, 1, 1);
    }

    // �_���[�W���̃G�t�F�N�g
    public void ShowDamageEffect()
    {
        StartCoroutine(DamageFlash());
    }

    IEnumerator DamageFlash()
    {
        for (int i = 0; i < 3; i++)
        {
            playerPortrait.material.color = new Color(1, 0, 0, 0.9f); // �ԐF
            yield return new WaitForSeconds(damageFlashDuration);
            playerPortrait.material.color = Color.white; // ���ɖ߂�
            yield return new WaitForSeconds(damageFlashDuration);
        }
    }

    // �G�̎c���Q�[�W�X�V
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
