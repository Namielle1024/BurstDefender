using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameResult { None, Cleared, GameOver }

    [Header("Global Settings")]
    public float bgmVolume = 0.5f; // BGMの音量
    public float seVolume = 0.5f; // SEの音量
    public SceneType currentSceneType; // 現在のシーンタイプ

    [Header("Game Settings")]
    [SerializeField] 
    int maxStages = 3;         // 最大ステージ数
    [SerializeField] 
    int unlockedStages = 1;    // 最初は１ステージだけ解放
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
    }

    void Update()
    {
        if (SceneType.Game == currentSceneType)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetCursorState(true); // カーソル表示
            }
            else if (Input.GetMouseButtonDown(0) && Cursor.visible == true)
            {
                SetCursorState(false); // カーソル非表示
            }
        }

        /// デバッグ
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
            PlayerManager.Instance.inputEnable(); // InputSystem有効化
        }
        else
        {
            PlayerManager.Instance.inputEnable(); // InputSystem無効化
            Debug.Log("既存のプレイヤーをアクティブ化します。");
            currentPlayer.transform.position = StageManager.Instance.GetPlayerSpawnPoint();
            currentPlayer.SetActive(true);
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

    void SetCursorState(bool active)
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
    Title = 0,
    Game = 1,
    Result = 2
}
