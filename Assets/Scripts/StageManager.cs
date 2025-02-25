using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [NamedArray("Stage1 Data", "Stage2 Data", "Sample Data")]
    public StageData[] stageDatas;

    [Header("Spawn Settings")]
    [SerializeField] // �G�̃v���n�u
    GameObject enemyPrefab;
    [SerializeField] // �X�|�[���͈͂̒��S(�f�o�b�O�p)
    Vector3 _spawnAreaCenter;
    [SerializeField] // �X�|�[���͈͂̃T�C�Y(�f�o�b�O�p)
    Vector3 _spawnAreaSize;
    [SerializeField] // �X�|�[������VFX
    GameObject spawnVFX;
    [SerializeField] // VFX�ƓG�̃X�|�[���Ԋu
    float spawnInterval = 1.0f;

    List<GameObject> activeEnemies = new List<GameObject>();
    int currentWave = 0;
    int currentEnemyCount = 0;
    int spawnEnemyCount = 0;
    int enemiesDefeated = 0;
    float waveCompleteDelay = 3.0f;�@// Wave�N���A��̑ҋ@����
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

    // �X�e�[�W�ݒ�
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

            // Wave�J�n�̉��o
            yield return new WaitForSeconds(1.0f);
            UIManager.Instance.SetActiveGameUI(false);
            UIManager.Instance.ShowWaveStart(currentWave + 1);
            UIManager.Instance.SetCurrentWave(currentWave + 1);
            UIManager.Instance.UpdateEnemyGauge(enemiesDefeated, wave.enemyCount);
            yield return new WaitForSeconds(wave.firstSpawnDelay);

            yield return StartCoroutine(SpawnWave(wave));

            // Wave����������܂ő҂�
            while (currentEnemyCount > 0)
            {
                yield return null;
            }

            UIManager.Instance.SetActiveGameUI(false);
            UIManager.Instance.ShowWaveComplete(currentWave + 1);
            yield return new WaitForSeconds(waveCompleteDelay);
        }

        // �X�e�[�W�N���A����
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

    // �G���X�|�[�����鏈��
    IEnumerator SpawnEnemy()
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

                    // �X�|�[��VFX�𐶐�
                    GameObject vfx = Instantiate(spawnVFX, spawnPosition, Quaternion.identity);

                    validPositionFound = true;

                    yield return new WaitForSeconds(spawnInterval);

                    // �X�|�[��VFX��j��
                    Destroy(vfx, spawnInterval);

                    // �G�𐶐�
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
        currentWave = 0;
        currentEnemyCount = 0;
        spawnEnemyCount = 0;
        enemiesDefeated = 0;
        waveInProgress = false;
    }

    // �����_���ȃX�|�[���ʒu���擾
    Vector3 GetRandomSpawnPosition()
    {
        Vector3 spawnAreaSize = stageDatas[GameManager.Instance.GetCurrentStage()].spawnAreaSize;

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

        return stageDatas[GameManager.Instance.GetCurrentStage()].spawnAreaCenter + randomPosition;
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

            // �c�G����UI���X�V
            UIManager.Instance.UpdateEnemyGauge(GetWaveDefeatedCount(), GetWaveEnemyCount());
        }
    }

    // �v���C���[�̃X�|�[���|�C���g���擾
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

    // Spawn Area�̉����iScene�r���[�j
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);  // �ΐF�A�����x30%
        Gizmos.DrawCube(_spawnAreaCenter, _spawnAreaSize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_spawnAreaCenter, _spawnAreaSize);
    }
}
