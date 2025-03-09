using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ステージ全体の管理を行うクラス。
/// 敵のスポーン、Wave進行、プレイヤーのスポーン位置管理などを担当。
/// </summary>
public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [NamedArray("Stage1 Data", "Stage2 Data", "Sample Data")]
    public StageData[] stageDatas;

    [Header("Spawn Settings")]
    [SerializeField] GameObject enemyPrefab;  // 敵のプレハブ
    [SerializeField] Vector3 _spawnAreaCenter; // スポーン範囲の中心（デバッグ用）
    [SerializeField] Vector3 _spawnAreaSize;  // スポーン範囲のサイズ（デバッグ用）
    [SerializeField] GameObject spawnVFX; // スポーン時のVFX
    [SerializeField] float spawnInterval = 1.0f; // VFXと敵のスポーン間隔

    List<GameObject> activeEnemies = new List<GameObject>(); // 現在フィールド上にいる敵リスト
    int currentWave = 0; // 現在のWave
    int currentEnemyCount = 0; // 現在フィールドにいる敵の数
    int spawnEnemyCount = 0; // すでにスポーンした敵の数
    int enemiesDefeated = 0; // 倒された敵の数
    float waveCompleteDelay = 3.0f; // Waveクリア後の待機時間

    /// <summary>
    /// シングルトンの初期化を行う。
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

        // シーンロード時のイベント登録
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// シーンロード時に呼び出される。
    /// </summary>
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ゲームシーン以外では非アクティブ化
        gameObject.SetActive(GameManager.Instance.currentSceneType == SceneType.Game);
    }

    void OnDestroy()
    {
        // シーンアンロード時にイベントを解除
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 総ステージ数を取得。
    /// </summary>
    public int TotalStages => stageDatas.Length;

    /// <summary>
    /// 指定されたステージをセットアップする。
    /// </summary>
    /// <param name="stageIndex">ステージのインデックス</param>
    public void SetupStage(int stageIndex)
    {
        ClearStageData();
        StartCoroutine(WaveRoutine(stageDatas[stageIndex]));
    }

    /// <summary>
    /// Wave進行処理を管理するコルーチン。
    /// </summary>
    IEnumerator WaveRoutine(StageData stageData)
    {
        for (currentWave = 0; currentWave < stageData.waves.Length; currentWave++)
        {
            WaveData wave = stageData.waves[currentWave];

            spawnEnemyCount = 0;
            enemiesDefeated = 0;

            // Wave開始の演出
            yield return new WaitForSeconds(1.0f);
            UIManager.Instance.SetActiveGameUI(false);
            UIManager.Instance.ShowWaveStart(currentWave + 1);
            UIManager.Instance.SetCurrentWave(currentWave + 1);
            UIManager.Instance.UpdateEnemyGauge(enemiesDefeated, wave.enemyCount);
            yield return new WaitForSeconds(wave.firstSpawnDelay);

            yield return StartCoroutine(SpawnWave(wave));

            // Waveが完了するまで待機
            while (currentEnemyCount > 0)
            {
                yield return null;
            }

            UIManager.Instance.SetActiveGameUI(false);
            UIManager.Instance.ShowWaveComplete(currentWave + 1);
            yield return new WaitForSeconds(waveCompleteDelay);
        }

        // ステージクリア処理
        GameManager.Instance.OnStageCleared();
    }

    /// <summary>
    /// Waveに従って敵をスポーンする。
    /// </summary>
    IEnumerator SpawnWave(WaveData wave)
    {
        while (spawnEnemyCount < wave.enemyCount)
        {
            if (currentEnemyCount < wave.maxEnemiesOnField)
            {
                StartCoroutine(SpawnEnemy());
            }
            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }

    /// <summary>
    /// 敵をスポーンする処理。
    /// </summary>
    IEnumerator SpawnEnemy()
    {
        Vector3 spawnPosition;
        bool validPositionFound = false;
        int attempts = 0;
        int maxAttempts = 10;

        while (!validPositionFound && attempts < maxAttempts)
        {
            spawnPosition = GetRandomSpawnPosition();

            if (Physics.Raycast(spawnPosition + Vector3.up * 50, Vector3.down, out RaycastHit hit, Mathf.Infinity))
            {
                if (hit.collider.CompareTag("Ground"))
                {
                    spawnPosition.y = hit.point.y;

                    // スポーンVFXを生成
                    GameObject vfx = Instantiate(spawnVFX, spawnPosition, Quaternion.identity);
                    validPositionFound = true;

                    yield return new WaitForSeconds(spawnInterval);

                    // スポーンVFXを破棄
                    Destroy(vfx, spawnInterval);

                    // 敵を生成
                    GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                    activeEnemies.Add(enemy);
                    currentEnemyCount++;
                    spawnEnemyCount++;
                }
            }
            attempts++;
        }

        if (!validPositionFound)
        {
            Debug.LogWarning("敵のスポーン位置が見つかりませんでした。");
        }
    }

    /// <summary>
    /// ステージデータをクリアする。
    /// </summary>
    public void ClearStageData()
    {
        Debug.Log("ステージデータをクリアします...");

        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
        currentWave = 0;
        currentEnemyCount = 0;
        spawnEnemyCount = 0;
        enemiesDefeated = 0;
    }

    /// <summary>
    /// ランダムなスポーン位置を取得する。
    /// </summary>
    /// <returns>ランダムなスポーン位置</returns>
    Vector3 GetRandomSpawnPosition()
    {
        Vector3 spawnAreaSize = stageDatas[GameManager.Instance.GetCurrentStage()].spawnAreaSize;

        Vector3 randomPosition = new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            50,
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );

        if (Physics.Raycast(randomPosition, Vector3.down, out RaycastHit hit, Mathf.Infinity))
        {
            randomPosition.y = hit.point.y;
        }
        else
        {
            randomPosition.y = 0;
        }

        return stageDatas[GameManager.Instance.GetCurrentStage()].spawnAreaCenter + randomPosition;
    }

    /// <summary>
    /// 敵が倒されたときの処理。
    /// </summary>
    public void OnEnemyDefeated(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            Destroy(enemy);
            currentEnemyCount--;
            enemiesDefeated++;

            UIManager.Instance.UpdateEnemyGauge(GetWaveDefeatedCount(), GetWaveEnemyCount());
        }
    }

    public Vector3 GetPlayerSpawnPoint() => stageDatas[GameManager.Instance.GetCurrentStage()].playerSpawnPoint;
    public int GetWaveEnemyCount() => stageDatas[GameManager.Instance.GetCurrentStage()].waves[currentWave].enemyCount;
    public int GetWaveDefeatedCount() => enemiesDefeated;

    /// <summary>
    /// Spawn Areaの可視化（Sceneビュー） 
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);  // 緑色、透明度30%
        Gizmos.DrawCube(_spawnAreaCenter, _spawnAreaSize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_spawnAreaCenter, _spawnAreaSize);
    }
}
