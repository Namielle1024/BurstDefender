using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Manager")]
    [SerializeField]
    GameObject sceneManager;
    [SerializeField]
    GameObject stageManager;
    [SerializeField]
    GameObject uiManager;

    public enum GameResult { None, Cleared, GameOver }

    [Header("Global Settings")]
    public float bgmVolume = 0.5f; // BGMの音量
    public float seVolume = 0.5f; // SEの音量
    public SceneType currentSceneType; // 現在のシーンタイプ

    [Header("Game Settings")]
    [SerializeField] // 最大ステージ数
    int maxStages = 2;
    [SerializeField] // 解放されたステージ数
    int unlockedStages = 1;
    GameResult lastGameResult = GameResult.None;
    int currentStage = 0; // 現在のステージ番号

    [Header("Player Settings")]
    [SerializeField]
    GameObject playerPrefab;
    GameObject currentPlayer;
    [SerializeField] 
    int playerLives = 3;       // プレイヤーの残機
    [SerializeField] 
    float respawnDelay = 3.0f; // リスポーンの待機時間

    [Header("Title Settings")]
    bool isSelectMode = false;

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

        sceneManager = Instantiate(sceneManager);
        stageManager = Instantiate(stageManager);
        uiManager = Instantiate(uiManager);
    }

    void Update()
    {
        if (SceneType.Game == currentSceneType)
        {
            if (Input.GetKeyDown(KeyCode.Escape) && Cursor.visible == false)
            {
                SetCursorState(true); // カーソル表示
                UIManager.Instance.OnPause(true);
                Time.timeScale = 0;
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && Cursor.visible == true)
            {
                SetCursorState(false); // カーソル非表示
                UIManager.Instance.OnPause(false);
                Time.timeScale = 1;
            }
        }

#if DEBUG /// デバッグ
        // Enterキーで次のシーンへ（テスト用）
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.Instance.LoadResultScene();
        }

        // Rキーでリスポーンテスト
        if (Input.GetKeyDown(KeyCode.R) && currentSceneType == SceneType.Game)
        {
            StartCoroutine(RespawnPlayer());
        }

        // Gキーでゲームオーバーテスト
        if (Input.GetKeyDown(KeyCode.G))
        {
            OnGameOver();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            OnStageCleared();
        }
#endif
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SetCursorState(true); // カーソル表示
            UIManager.Instance.OnPause(true);
            Time.timeScale = 0;
        }
    }

    // 現在のシーンタイプに応じた初期化
    public void InitializeScene()
    {
       // currentSceneType = (SceneType)UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;

        // シーンタイプごとの初期化
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

    // タイトルシーンの初期化
    void InitializeTitleScene()
    {
        Debug.Log("Title Scene Initialized");
        UnlockCursor();
    }

    // ゲームシーンの初期化
    void InitializeGameScene()
    {
        Debug.Log("Game Scene Initialized");

        // カーソルをロック＆非表示
        LockCursor();

        // ステージの初期化
        StageManager.Instance.SetupStage(currentStage);

        // プレイヤースポーン
        PlayerManager.Instance.Revive(StageManager.Instance.GetPlayerSpawnPoint());
    }

    // リザルトシーンの初期化
    void InitializeResultScene()
    {
        Debug.Log("Result Scene Initialized");
        UnlockCursor();
    }

    public void SpawnPlayer()
    {
        if (currentPlayer == null)
        {
            Debug.Log("プレイヤーをスポーンします...");
            currentPlayer = Instantiate(playerPrefab, StageManager.Instance.GetPlayerSpawnPoint(), Quaternion.identity);
            DontDestroyOnLoad(currentPlayer);
            PlayerManager.Instance.ResetPlayer();
            PlayerManager.Instance.inputEnable();
        }
        else
        {
            Debug.Log("既存のプレイヤーをアクティブ化します。");
            currentPlayer.transform.position = StageManager.Instance.GetPlayerSpawnPoint();
            currentPlayer.SetActive(true);
            PlayerManager.Instance.ResetPlayer();
            PlayerManager.Instance.inputEnable();
        }
    }

    public void RemovePlayer()
    {
        if (currentPlayer != null)
        {
            PlayerManager.Instance.inputDisable();
            Debug.Log("プレイヤーを非アクティブ化します...");
            currentPlayer.SetActive(false);
        }
    }

    public void ResetPlayerLives()
    {
        Debug.Log("プレイヤーの残機をリセットします...");
        playerLives = 3;
    }

    // プレイヤーが死亡したときの処理
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

    // プレイヤーのリスポーン処理
    IEnumerator RespawnPlayer()
    {
        Debug.Log("Respawning Player...");
        yield return new WaitForSeconds(respawnDelay);

        PlayerManager.Instance.Revive(StageManager.Instance.GetPlayerSpawnPoint());
    }

    // ステージクリア処理
    public void OnStageCleared()
    {
        lastGameResult = GameResult.Cleared;
        if (unlockedStages < maxStages)
        {
            unlockedStages++;
        }
        currentSceneType = SceneType.Result;
        SceneManager.Instance.LoadResultScene();
    }

    // ゲームオーバー処理
    public void OnGameOver()
    {
        lastGameResult = GameResult.GameOver;
        currentSceneType = SceneType.Result;
        SceneManager.Instance.LoadResultScene();
    }

    // タイトルシーンへ戻る際にセレクト画面を表示する
    public void ReturnToTitle()
    {
        isSelectMode = true;
        currentSceneType = SceneType.Title;
        SceneManager.Instance.LoadTitleScene();
    }

    // カーソルをロック
    void LockCursor()
    {
        Debug.Log("カーソル非表示");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // カーソルを解除
    void UnlockCursor()
    {
        Debug.Log("カーソル表示");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StopVFXEffects()
    {
        VisualEffect[] effects = FindObjectsByType<VisualEffect>(FindObjectsSortMode.None);
        foreach (var effect in effects)
        {
            effect.Stop();  // 全VFXを停止
        }
    }

    // カーソルの状態を取得するメソッド
    public bool GetCursorActive()
    {
        return Cursor.visible;
    }

    // 現在のステージを取得するメソッド
    public int GetCurrentStage()
    {
        return currentStage;
    }

    public void SetCurrentStage(int stage)
    {
        currentStage = stage;
    }

    // タイトルシーンの状態を取得するメソッド
    public bool GetSelectMode()
    {
        return isSelectMode;
    }

    public int GetUnlockedStages()
    {
        return unlockedStages;
    }

    public GameResult GetGameResult()
    {
        return lastGameResult;
    }

    public void SetCursorState(bool active)
    {
        Cursor.visible = active;
        Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void SetSceneType(SceneType sceneType)
    {
        currentSceneType = sceneType;
    }
}

public enum SceneType
{
    Title,
    Game,
    Result
}
