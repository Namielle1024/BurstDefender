using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    public StageData[] stageDatas;

    [Header("Spawn Settings")]
    [SerializeField]
    private GameObject enemyPrefab; // �G�̃v���n�u
    [SerializeField]
    private Vector3 spawnAreaCenter; // �X�|�[���͈͂̒��S
    [SerializeField]
    private Vector3 spawnAreaSize; // �X�|�[���͈͂̃T�C�Y�i��`�j

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

    // �X�e�[�W�ݒ�
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

        // �G�S�ő҂�
        while (currentEnemyCount > 0)
        {
            yield return null;
        }

        // �X�e�[�W�N���A����
        GameManager.Instance.OnStageCleared();
    }

    // �G���X�|�[�����鏈��
    private void SpawnEnemy()
    {
        Vector3 spawnPosition;
        bool validPositionFound = false;
        int attempts = 0;
        int maxAttempts = 10;

        while (!validPositionFound && attempts < maxAttempts)
        {
            spawnPosition = GetRandomSpawnPosition();

            // �X�|�[���ʒu�̐^���ɒn�ʂ����邩�m�F
            RaycastHit hit;
            if (Physics.Raycast(spawnPosition + Vector3.up * 50, Vector3.down, out hit, Mathf.Infinity))
            {
                if (hit.collider.CompareTag("Ground"))  // �n�ʃ^�O������I�u�W�F�N�g�̂�
                {
                    spawnPosition.y = hit.point.y; // �n�`�̍������擾
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
            Debug.LogWarning("�G�̃X�|�[���ʒu��������܂���ł����B");
        }
    }

    // �X�e�[�W�f�[�^�̃N���A
    public void ClearStageData()
    {
        Debug.Log("�X�e�[�W�f�[�^���N���A���܂�...");

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

    // �����_���ȃX�|�[���ʒu���擾
    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 randomPosition = new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            50, // ���C�L���X�g�����������J�n
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );

        RaycastHit hit;
        if (Physics.Raycast(randomPosition, Vector3.down, out hit, Mathf.Infinity))
        {
            randomPosition.y = hit.point.y; // �n�`�̍�����ݒ�
        }
        else
        {
            randomPosition.y = 0; // �f�t�H���g�̍���
        }

        return spawnAreaCenter + randomPosition;
    }

    // �G���|���ꂽ�Ƃ��ɃJ�E���g�X�V
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

    // �X�e�[�W�N���A�̔���
    public bool IsStageClear()
    {
        return enemiesDefeated >= stageDatas[GameManager.Instance.GetCurrentStage()].enemyCount;
    }

    // �v���C���[�̃X�|�[���|�C���g���擾
    public Vector3 GetPlayerSpawnPoint()
    {
        return stageDatas[GameManager.Instance.GetCurrentStage()].playerSpawnPoint;
    }

    // Spawn Area�̉����iScene�r���[�j
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);  // �ΐF�A�����x30%
        Gizmos.DrawCube(spawnAreaCenter, spawnAreaSize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
    }
}
