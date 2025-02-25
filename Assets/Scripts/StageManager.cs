using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [NamedArray("Stage1 Data", "Stage2 Data", "Sample Data")]
    public StageData[] stageDatas;

    [Header("Spawn Settings")]
    [SerializeField] // 敵のプレハブ
    GameObject enemyPrefab;
    [SerializeField] // スポーン範囲の中心(デバッグ用)
    Vector3 _spawnAreaCenter;
    [SerializeField] // スポーン範囲のサイズ(デバッグ用)
    Vector3 _spawnAreaSize;
    [SerializeField] // スポーン時のVFX
    GameObject spawnVFX;
    [SerializeField] // VFXと敵のスポーン間隔
    float spawnInterval = 1.0f;

    List<GameObject> activeEnemies = new List<GameObject>();
    int currentWave = 0;
    int currentEnemyCount = 0;
    int spawnEnemyCount = 0;
    int enemiesDefeated = 0;
    float waveCompleteDelay = 3.0f;　// Waveクリア後の待機時間
    bool waveInProgress = false;

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

    public int TotalStages => stageDatas.Length;

    // ステージ設定
    public void SetupStage(int stageIndex)
    {
        ClearStageData();
        StartCoroutine(WaveRoutine(stageDatas[stageIndex]));
    }

    IEnumerator WaveRoutine(StageData stageData)
    {
        for (currentWave = 0; currentWave < stageData.waves.Length; currentWave++)
        {
            WaveData wave = stageData.waves[currentWave];

            spawnEnemyCount = 0;
            enemiesDefeated = 0;
            waveInProgress = true;

            // Wave開始の演出
            yield return new WaitForSeconds(1.0f);
            UIManager.Instance.SetActiveGameUI(false);
            UIManager.Instance.ShowWaveStart(currentWave + 1);
            UIManager.Instance.SetCurrentWave(currentWave + 1);
            UIManager.Instance.UpdateEnemyGauge(enemiesDefeated, wave.enemyCount);
            yield return new WaitForSeconds(wave.firstSpawnDelay);

            yield return StartCoroutine(SpawnWave(wave));

            // Waveが完了するまで待つ
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

    // 敵をスポーンする処理
    IEnumerator SpawnEnemy()
    {
        Vector3 spawnPosition;
        bool validPositionFound = false;
        int attempts = 0;
        int maxAttempts = 10;

        while (!validPositionFound && attempts < maxAttempts)
        {
            spawnPosition = GetRandomSpawnPosition();

            // スポーン位置の真下に地面があるか確認
            RaycastHit hit;
            if (Physics.Raycast(spawnPosition + Vector3.up * 50, Vector3.down, out hit, Mathf.Infinity))
            {
                if (hit.collider.CompareTag("Ground"))  // 地面タグがあるオブジェクトのみ
                {
                    spawnPosition.y = hit.point.y; // 地形の高さを取得

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

    // ステージデータのクリア
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
        waveInProgress = false;
    }

    // ランダムなスポーン位置を取得
    Vector3 GetRandomSpawnPosition()
    {
        Vector3 spawnAreaSize = stageDatas[GameManager.Instance.GetCurrentStage()].spawnAreaSize;

        Vector3 randomPosition = new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            50, // レイキャストを上方向から開始
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );

        RaycastHit hit;
        if (Physics.Raycast(randomPosition, Vector3.down, out hit, Mathf.Infinity))
        {
            randomPosition.y = hit.point.y; // 地形の高さを設定
        }
        else
        {
            randomPosition.y = 0; // デフォルトの高さ
        }

        return stageDatas[GameManager.Instance.GetCurrentStage()].spawnAreaCenter + randomPosition;
    }

    // 敵が倒されたときにカウント更新
    public void OnEnemyDefeated(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            Destroy(enemy);
            currentEnemyCount--;
            enemiesDefeated++;

            // 残敵数のUIを更新
            UIManager.Instance.UpdateEnemyGauge(GetWaveDefeatedCount(), GetWaveEnemyCount());
        }
    }

    // プレイヤーのスポーンポイントを取得
    public Vector3 GetPlayerSpawnPoint()
    {
        return stageDatas[GameManager.Instance.GetCurrentStage()].playerSpawnPoint;
    }

    public int GetWaveEnemyCount()
    {
        return stageDatas[GameManager.Instance.GetCurrentStage()].waves[currentWave].enemyCount;
    }

    public int GetWaveDefeatedCount()
    {
        return enemiesDefeated;
    }

    // Spawn Areaの可視化（Sceneビュー）
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);  // 緑色、透明度30%
        Gizmos.DrawCube(_spawnAreaCenter, _spawnAreaSize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_spawnAreaCenter, _spawnAreaSize);
    }
}
