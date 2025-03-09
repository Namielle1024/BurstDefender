using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーンの管理を行うシングルトンクラス。
/// シーンのロードやプレイヤーの管理を行う。
/// </summary>
public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance;

    // シーンインデックスの定数
    const int TITLE_SCENE_INDEX = 0;
    const int RESULT_SCENE_INDEX = 1;
    const int GAME_SCENE_INDEX_START = 2;

    /// <summary>
    /// シングルトンの初期化とシーンロードイベントの登録。
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // シーンロード時のイベント登録
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// シーンがロードされた際に実行される処理。
    /// </summary>
    /// <param name="scene">ロードされたシーン</param>
    /// <param name="mode">ロードモード</param>
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"シーンがロードされました: {scene.name}");

        if (scene.buildIndex >= GAME_SCENE_INDEX_START)
        {
            // ゲームシーンの場合、プレイヤーをスポーン
            GameManager.Instance.SpawnPlayer();
            GameManager.Instance.StopVFXEffects();

            // カメラ設定
            Camera gameCamera = FindAnyObjectByType<Camera>();
            if (gameCamera != null)
            {
                PlayerManager.Instance.SetCamera(gameCamera);
                Debug.Log("カメラを設定しました。");
            }
            else
            {
                Debug.LogWarning("カメラが見つかりません！");
            }
        }
        else
        {
            // ゲームシーン以外ではプレイヤーを非アクティブ化
            GameManager.Instance.RemovePlayer();
        }

        // シーンの種類を GameManager に通知し、初期化を実行
        if (scene.buildIndex == TITLE_SCENE_INDEX)
        {
            GameManager.Instance.SetSceneType(SceneType.Title);
        }
        else if (scene.buildIndex == RESULT_SCENE_INDEX)
        {
            GameManager.Instance.SetSceneType(SceneType.Result);
        }
        else
        {
            GameManager.Instance.SetSceneType(SceneType.Game);
        }

        GameManager.Instance.InitializeScene();
    }

    /// <summary>
    /// シーンが破棄された際にイベントを解除。
    /// </summary>
    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 指定したシーンをロード。
    /// </summary>
    /// <param name="sceneIndex">ロードするシーンのインデックス</param>
    public void LoadScene(int sceneIndex)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }

    /// <summary>
    /// タイトルシーンへ遷移（常にセレクトモードへ）。
    /// </summary>
    public void LoadTitleScene()
    {
        LoadScene(TITLE_SCENE_INDEX);
    }

    /// <summary>
    /// 指定したステージに移動。
    /// </summary>
    /// <param name="stageIndex">ステージのインデックス（ゲームシーンのインデックス）</param>
    public void LoadStage(int stageIndex)
    {
        GameManager.Instance.SetSceneType(SceneType.Game);
        GameManager.Instance.SetCurrentStage(stageIndex - GAME_SCENE_INDEX_START);
        LoadScene(stageIndex);
    }

    /// <summary>
    /// リザルトシーンへ移動。
    /// </summary>
    public void LoadResultScene()
    {
        LoadScene(RESULT_SCENE_INDEX);
    }
}
