using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    public StageData[] stageDatas;

    [Header("Spawn Settings")]
    [SerializeField]
    private GameObject enemyPrefab; // 敵のプレハブ
    [SerializeField]
    private Vector3 spawnAreaCenter; // スポーン範囲の中心
    [SerializeField]
    private Vector3 spawnAreaSize; // スポーン範囲のサイズ（矩形）

    private List<GameObject> activeEnemies = new List<GameObject>();
    private int currentEnemyCount = 0;
    private int enemiesDefeated = 0;

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
        StageData stageData = stageDatas[stageIndex];
        StartCoroutine(SpawnEnemiesRoutine(stageData));
    }

    private IEnumerator SpawnEnemiesRoutine(StageData stageData)
    {
        while (enemiesDefeated < stageData.enemyCount)
        {
            if (currentEnemyCount < stageData.maxEnemiesOnField)
            {
                SpawnEnemy();
            }
            yield return new WaitForSeconds(stageData.spawnInterval);
        }

        // 敵全滅待ち
        while (currentEnemyCount > 0)
        {
            yield return null;
        }

        // ステージクリア処理
        GameManager.Instance.OnStageCleared();
    }

    // 敵をスポーンする処理
    private void SpawnEnemy()
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
                    GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                    activeEnemies.Add(enemy);
                    currentEnemyCount++;
                    validPositionFound = true;
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
        currentEnemyCount = 0;
        enemiesDefeated = 0;
    }

    // ランダムなスポーン位置を取得
    private Vector3 GetRandomSpawnPosition()
    {
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

        return spawnAreaCenter + randomPosition;
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
        }
    }

    // ステージクリアの判定
    public bool IsStageClear()
    {
        return enemiesDefeated >= stageDatas[GameManager.Instance.GetCurrentStage()].enemyCount;
    }

    // プレイヤーのスポーンポイントを取得
    public Vector3 GetPlayerSpawnPoint()
    {
        return stageDatas[GameManager.Instance.GetCurrentStage()].playerSpawnPoint;
    }

    // Spawn Areaの可視化（Sceneビュー）
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);  // 緑色、透明度30%
        Gizmos.DrawCube(spawnAreaCenter, spawnAreaSize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
    }
}
