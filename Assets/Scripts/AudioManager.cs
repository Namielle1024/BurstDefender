using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mixer")]
    [SerializeField] AudioMixer audioMixer;

    [Header("Sources")]
    [SerializeField] AudioSource bgmSource;     // BGM用
    [SerializeField] AudioSource seSource2D;    // UI・2D SE用

    [Header("BGM Clips")]
    [SerializeField] AudioClip titleBGM;
    [SerializeField] AudioClip gameBGM;
    [SerializeField] AudioClip resultBGM;

    [Header("UI SE Clips")]
    [SerializeField] AudioClip clickSE; // ゲームシーンのクリックSE
    [SerializeField] AudioClip decideButtonSE; // タイトル、リザルトの決定ボタンSE
    [SerializeField] AudioClip cancelButtonSE; // タイトル、リザルトのキャンセル(戻る等)ボタンSE

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    /// <summary>
    /// BGM再生
    /// </summary>
    /// <param name="clip">BGMのクリップ</param>
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource.clip == clip) return;
        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    /// <summary>
    /// 2D UI SE再生
    /// </summary>
    /// <param name="clip">SEのクリップ</param>
    public void PlaySE2D(AudioClip clip)
    {
        if (clip == null) return;
        seSource2D.PlayOneShot(clip);
    }

    /// <summary>
    /// BGM音量設定
    /// </summary>
    /// <param name="volume">音量</param>
    public void SetBGMVolume(float volume)
    {
        GameManager.Instance.bgmVolume = volume;
        audioMixer.SetFloat("BGMVolume", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1)) * 20);
    }

    /// <summary>
    /// SE音量設定
    /// </summary>
    /// <param name="volume">音量</param>
    public void SetSEVolume(float volume)
    {
        GameManager.Instance.seVolume = volume;
        audioMixer.SetFloat("SEVolume", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1)) * 20);
    }

    /// <summary>
    /// シーンに応じたBGM再生
    /// </summary>
    /// <param name="type">シーンタイプ</param>
    public void PlaySceneBGM(SceneType type)
    {
        switch (type)
        {
            case SceneType.Title:
                PlayBGM(titleBGM);
                break;
            case SceneType.Game:
                PlayBGM(gameBGM);
                break;
            case SceneType.Result:
                PlayBGM(resultBGM);
                break;
        }
    }

    /// <summary>
    /// クリックSE再生
    /// </summary>
    public void PlayClickSE()
    {
        PlaySE2D(clickSE);
    }

    /// <summary>
    /// 決定ボタンクリック時のSE再生
    /// </summary>
    public void PlayDecideSE()
    {
        PlaySE2D(decideButtonSE);
    }

    /// <summary>
    /// キャンセルボタンクリック時のSE再生
    /// </summary>
    public void PlayCancelSE()
    {
        PlaySE2D(cancelButtonSE);
    }
}
