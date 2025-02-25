using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance;

    const int TITLE_SCENE_INDEX = 0;
    const int RESULT_SCENE_INDEX = 1;
    const int GAME_SCENE_INDEX_START = 2;

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

    // シーンがロードされたときに実行されるメソッド
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"シーンがロードされました: {scene.name}");

        if (scene.buildIndex >= GAME_SCENE_INDEX_START)
        {
            GameManager.Instance.SpawnPlayer();
            GameManager.Instance.StopVFXEffects();
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
            GameManager.Instance.RemovePlayer();
        }

        // GameManagerのシーンタイプを設定
        if (scene.buildIndex == TITLE_SCENE_INDEX)
        {
            GameManager.Instance.SetSceneType(SceneType.Title);
            GameManager.Instance.InitializeScene();
        }
        else if (scene.buildIndex == RESULT_SCENE_INDEX)
        {
            GameManager.Instance.SetSceneType(SceneType.Result);
            GameManager.Instance.InitializeScene();
        }
        else
        {
            GameManager.Instance.SetSceneType(SceneType.Game);
            GameManager.Instance.InitializeScene();
        }
    }

    void OnDestroy()
    {
        // シーンアンロード時にイベントを解除
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // シーンをロード
    public void LoadScene(int sceneIndex)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }

    // タイトルシーンへ遷移（常にセレクトモードへ）
    public void LoadTitleScene()
    {
        LoadScene(TITLE_SCENE_INDEX);
    }

    // 指定したステージに移動
    public void LoadStage(int stageIndex)
    {
        GameManager.Instance.SetSceneType(SceneType.Game);
        GameManager.Instance.SetCurrentStage(stageIndex - 2);
        LoadScene(stageIndex);
    }

    // リザルトシーンへ移動
    public void LoadResultScene()
    {
        LoadScene(RESULT_SCENE_INDEX);
    }
}
