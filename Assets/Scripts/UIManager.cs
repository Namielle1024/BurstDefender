using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    bool isGameScene = false; // ゲームシーンかを判別するフラグ

    [Header("UI Prefabs")]
    [SerializeField] // すべてのUIをまとめたPrefab
    GameObject uiPrefab;
    GameObject instantiatedUI;
    UIController uiController;

    /// <summary>
    /// シングルトン化
    /// </summary>
    void Awake()
    {
        // シングルトン化
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// シーンがロードされたときに呼ばれるメソッド
    /// false : ゲームUIを破棄する
    /// true  : ゲームUIを生成する
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ゲームシーンかどうかを判定
        isGameScene = GameManager.Instance.currentSceneType == SceneType.Game;

        if (isGameScene)
        {
            InitializeUI();
        }
        else
        {
            DestroyUI();
        }

        if (instantiatedUI != null)
        {
            instantiatedUI.SetActive(isGameScene);
        }
    }

    /// <summary>
    /// UIの生成
    /// </summary>
    void InitializeUI()
    {
        if (instantiatedUI == null)
        {
            instantiatedUI = Instantiate(uiPrefab, transform);

            // UIController を取得
            uiController = instantiatedUI.GetComponent<UIController>();

            if (uiController == null)
            {
                Debug.LogError("UIControllerがuiPrefabにアタッチされていません");
            }
        }
    }

    /// <summary>
    /// UIの破棄
    /// </summary>
    void DestroyUI()
    {
        if (instantiatedUI != null)
        {
            Destroy(instantiatedUI);
            instantiatedUI = null;
            uiController = null;
            Debug.Log("UIManager: ゲームシーン以外のためUIを削除しました");
        }
    }

    /// <summary>
    /// ポーズ時のUIを表示、非表示設定
    /// </summary>
    /// <param name="pause"></param>
    public void OnPause(bool pause)
    {
        if (instantiatedUI != null)
        {
            uiController.OnPause(pause);
            PlayerManager.Instance.ResetPlayerInput();
        }
    }

    /// <summary>
    /// ゲームシーンのUIをアクティブにする
    /// false: 全UIを非表示
    /// true : StartUI以外を表示
    /// </summary>
    /// <param name="active"></param>
    public void SetActiveGameUI(bool active)
    {
        if (instantiatedUI != null)
        {
            uiController.SetActiveGameUI(active);
        }
    }

    /// <summary>
    /// ウェーブ開始時のUIを表示
    /// </summary>
    /// <param name="waveNumber"></param>
    public void ShowWaveStart(int waveNumber)
    {
        if (instantiatedUI != null)
        {
            uiController.ShowWaveStart(waveNumber);
        }
    }

    /// <summary>
    /// ウェーブクリア時のUIを表示
    /// </summary>
    /// <param name="waveNumber"></param>
    public void ShowWaveComplete(int waveNumber)
    {
        if (instantiatedUI != null)
        {
            uiController.ShowWaveComplete(waveNumber);
        }
    }

    /// <summary>
    /// 現在ウェーブ数を設定
    /// </summary>
    /// <param name="waveNumber"></param>
    public void SetCurrentWave(int waveNumber)
    {
        if (instantiatedUI != null)
        {
            uiController.SetCurrentWave(waveNumber);
        }
    }

    /// <summary>
    /// プレイヤーのHPバーを更新
    /// </summary>
    /// <param name="currentHP"></param>
    /// <param name="maxHP"></param>
    public void UpdateHPBar(float currentHP, float maxHP)
    {
        if (instantiatedUI != null)
        {
            uiController.UpdateHPBar(currentHP, maxHP);
        }
    }

    /// <summary>
    /// ダメージエフェクトを表示
    /// </summary>
    public void ShowDamageEffect()
    {
        if (instantiatedUI != null)
        {
            uiController.ShowDamageEffect();
        }
    }

    /// <summary>
    /// 敵の残り数UIを更新
    /// </summary>
    /// <param name="remaining"></param>
    /// <param name="total"></param>
    public void UpdateEnemyGauge(int remaining, int total)
    {
        if (instantiatedUI != null)
        {
            uiController.UpdateEnemyGauge(remaining, total);
        }
    }

    /// <summary>
    /// ポーズ画面がアクティブかどうかを返す
    /// true  : アクティブ
    /// false : 非アクティブ
    /// </summary>
    /// <returns></returns>
    public bool IsPauseActive()
    {
        return uiController.IsPauseActive();
    }
}
