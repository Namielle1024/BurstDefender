using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// ゲーム全体の管理を行うシングルトンクラス
/// シーン管理、プレイヤーのリスポーン、ゲームの進行制御を担当
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // シングルトンインスタンス

    [Header("Manager")]
    [SerializeField] GameObject sceneManager;
    [SerializeField] GameObject stageManager;
    [SerializeField] GameObject uiManager;

    public enum GameResult { None, Cleared, GameOver }

    [Header("Global Settings")]
    public float bgmVolume = 0.5f; // BGMの音量
    public float seVolume = 0.5f; // SEの音量
    public SceneType currentSceneType; // 現在のシーンタイプ

    [Header("Game Settings")]
    [SerializeField] int maxStages = 2;    // 最大ステージ数
    [SerializeField] int unlockedStages = 1; // 解放済みのステージ数
    GameResult lastGameResult = GameResult.None;
    int currentStage = 0; // 現在のステージ番号

    [Header("Player Settings")]
    [SerializeField] GameObject playerPrefab;
    GameObject currentPlayer;
    [SerializeField] int playerLives = 3;       // プレイヤーの残機
    [SerializeField] float respawnDelay = 3.0f; // リスポーン待機時間

    [Header("Title Settings")]
    bool isSelectMode = false;

    /// <summary>
    /// シングルトンの初期化とマネージャーオブジェクトの生成
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 各マネージャーのインスタンスを生成
        sceneManager = Instantiate(sceneManager);
        stageManager = Instantiate(stageManager);
        uiManager = Instantiate(uiManager);
    }

    /// <summary>
    /// 毎フレームの処理（ポーズ処理、デバッグ用入力）
    /// </summary>
    void Update()
    {
        if (SceneType.Game == currentSceneType)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                bool isCursorActive = Cursor.visible;
                SetCursorState(!isCursorActive);
                UIManager.Instance.OnPause(!isCursorActive);
                Time.timeScale = isCursorActive ? 1 : 0;
            }
        }

#if DEBUG // デバッグ用
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.Instance.LoadResultScene(); // リザルト画面へ
        }

        if (Input.GetKeyDown(KeyCode.R) && currentSceneType == SceneType.Game)
        {
            StartCoroutine(RespawnPlayer()); // プレイヤーリスポーン
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            OnGameOver(); // ゲームオーバー
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            OnStageCleared(); // ステージクリア
        }
#endif
    }

    /// <summary>
    /// アプリがポーズされた際の処理（ポーズメニューの表示）
    /// </summary>
    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SetCursorState(true);
            UIManager.Instance.OnPause(true);
            Time.timeScale = 0;
        }
    }

    /// <summary>
    /// シーンの種類に応じた初期化処理
    /// </summary>
    public void InitializeScene()
    {
        switch (currentSceneType)
        {
            case SceneType.Title:
                InitializeTitleScene();
                break;
            case SceneType.Game:
                InitializeGameScene();
                break;
            case SceneType.Result:
                InitializeResultScene();
                break;
        }
    }

    /// <summary>
    /// タイトルシーンの初期化
    /// </summary>
    void InitializeTitleScene()
    {
        Debug.Log("Title Scene Initialized");
        SetCursorState(true);
    }

    /// <summary>
    /// ゲームシーンの初期化
    /// </summary>
    void InitializeGameScene()
    {
        Debug.Log("Game Scene Initialized");

        SetCursorState(false); // カーソルをロック

        // ステージとプレイヤーのセットアップ
        StageManager.Instance.gameObject.SetActive(true);
        StageManager.Instance.SetupStage(currentStage);
        PlayerManager.Instance.Revive(StageManager.Instance.GetPlayerSpawnPoint());
    }

    /// <summary>
    /// リザルトシーンの初期化
    /// </summary>
    void InitializeResultScene()
    {
        Debug.Log("Result Scene Initialized");
        SetCursorState(true);
    }

    /// <summary>
    /// プレイヤーをスポーン
    /// </summary>
    public void SpawnPlayer()
    {
        if (currentPlayer == null)
        {
            Debug.Log("プレイヤーをスポーンします...");
            currentPlayer = Instantiate(playerPrefab, StageManager.Instance.GetPlayerSpawnPoint(), Quaternion.identity);
            DontDestroyOnLoad(currentPlayer);
        }
        else
        {
            Debug.Log("既存のプレイヤーをアクティブ化");
            currentPlayer.transform.position = StageManager.Instance.GetPlayerSpawnPoint();
            currentPlayer.SetActive(true);
        }

        PlayerManager.Instance.ResetPlayer();
        PlayerManager.Instance.inputEnable();
    }

    /// <summary>
    /// プレイヤーを非アクティブ化
    /// </summary>
    public void RemovePlayer()
    {
        if (currentPlayer != null)
        {
            PlayerManager.Instance.inputDisable();
            Debug.Log("プレイヤーを非アクティブ化");
            currentPlayer.SetActive(false);
        }
    }

    /// <summary>
    /// プレイヤーのリスポーン処理
    /// </summary>
    /// <returns>リスポーン待機時間後にプレイヤーを復活させる</returns>
    IEnumerator RespawnPlayer()
    {
        Debug.Log("Respawning Player...");
        yield return new WaitForSeconds(respawnDelay);
        PlayerManager.Instance.Revive(StageManager.Instance.GetPlayerSpawnPoint());
    }

    /// <summary>
    /// プレイヤーが死亡したときの処理
    /// </summary>
    public void OnPlayerDeath()
    {
        playerLives--;

        if (playerLives <= 0)
        {
            OnGameOver();
        }
        else
        {
            StartCoroutine(RespawnPlayer());
        }
    }

    /// <summary>
    /// ステージクリア時の処理
    /// </summary>
    public void OnStageCleared()
    {
        lastGameResult = GameResult.Cleared;
        if (unlockedStages < maxStages) unlockedStages++;
        currentSceneType = SceneType.Result;
        SceneManager.Instance.LoadResultScene();
    }

    /// <summary>
    /// ゲームオーバー時の処理
    /// </summary>
    public void OnGameOver()
    {
        lastGameResult = GameResult.GameOver;
        currentSceneType = SceneType.Result;
        SceneManager.Instance.LoadResultScene();
    }

    /// <summary>
    /// タイトルシーンへ戻る（セレクト画面を表示する）
    /// </summary>
    public void ReturnToTitle()
    {
        isSelectMode = true;
        currentSceneType = SceneType.Title;
        SceneManager.Instance.LoadTitleScene();
    }

    /// <summary>
    /// VFXをすべて停止
    /// </summary>
    public void StopVFXEffects()
    {
        foreach (var effect in FindObjectsByType<VisualEffect>(FindObjectsSortMode.None))
        {
            effect.Stop();
        }
    }

    /// <summary>
    /// カーソルの表示状態を取得
    /// </summary>
    /// <returns>カーソルが表示されているかどうか</returns>
    public bool GetCursorActive()
    {
        return Cursor.visible;
    }

    /// <summary>
    /// カーソルの表示・非表示を設定
    /// </summary>
    /// <param name="active">true:表示, false:非表示</param>
    public void SetCursorState(bool active)
    {
        Cursor.visible = active;
        Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;
    }

    /// <summary>
    /// 現在のシーンタイプを設定
    /// </summary>
    /// <param name="sceneType">設定するシーンタイプ</param>
    public void SetSceneType(SceneType sceneType)
    {
        currentSceneType = sceneType;
    }

    /// <summary>
    /// 現在のステージを取得
    /// </summary>
    /// <returns>現在のステージ番号</returns>
    public int GetCurrentStage()
    {
        return currentStage;
    }

    /// <summary>
    /// ステージ番号を設定
    /// </summary>
    /// <param name="stage">設定するステージ番号</param>
    public void SetCurrentStage(int stage)
    {
        currentStage = stage;
    }

    /// <summary>
    /// ステージ数を取得
    /// </summary>
    /// <returns>最大ステージ数</returns>
    public int GetMaxStages()
    {
        return maxStages;
    }

    /// <summary>
    /// 解放済みのステージ数を取得
    /// </summary>
    /// <returns>解放済みステージ数</returns>
    public int GetUnlockedStages()
    {
        return unlockedStages;
    }

    /// <summary>
    /// 選択モードかどうかを取得
    /// </summary>
    /// <returns>選択モードの状態</returns>
    public bool GetSelectMode()
    {
        return isSelectMode;
    }

    /// <summary>
    /// 現在のゲーム結果を取得
    /// </summary>
    /// <returns>ゲーム結果</returns>
    public GameResult GetGameResult()
    {
        return lastGameResult;
    }
}

/// <summary>
/// シーンの種類
/// </summary>
public enum SceneType
{
    Title,
    Game,
    Result
}
