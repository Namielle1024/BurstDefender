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
    public float bgmVolume = 0.5f; // BGM�̉���
    public float seVolume = 0.5f; // SE�̉���
    public SceneType currentSceneType; // ���݂̃V�[���^�C�v

    [Header("Game Settings")]
    [SerializeField] // �ő�X�e�[�W��
    int maxStages = 2;
    [SerializeField] // ������ꂽ�X�e�[�W��
    int unlockedStages = 1;
    GameResult lastGameResult = GameResult.None;
    int currentStage = 0; // ���݂̃X�e�[�W�ԍ�

    [Header("Player Settings")]
    [SerializeField]
    GameObject playerPrefab;
    GameObject currentPlayer;
    [SerializeField] 
    int playerLives = 3;       // �v���C���[�̎c�@
    [SerializeField] 
    float respawnDelay = 3.0f; // ���X�|�[���̑ҋ@����

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
                SetCursorState(true); // �J�[�\���\��
                UIManager.Instance.OnPause(true);
                Time.timeScale = 0;
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && Cursor.visible == true)
            {
                SetCursorState(false); // �J�[�\����\��
                UIManager.Instance.OnPause(false);
                Time.timeScale = 1;
            }
        }

#if DEBUG /// �f�o�b�O
        // Enter�L�[�Ŏ��̃V�[���ցi�e�X�g�p�j
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.Instance.LoadResultScene();
        }

        // R�L�[�Ń��X�|�[���e�X�g
        if (Input.GetKeyDown(KeyCode.R) && currentSceneType == SceneType.Game)
        {
            StartCoroutine(RespawnPlayer());
        }

        // G�L�[�ŃQ�[���I�[�o�[�e�X�g
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
            SetCursorState(true); // �J�[�\���\��
            UIManager.Instance.OnPause(true);
            Time.timeScale = 0;
        }
    }

    // ���݂̃V�[���^�C�v�ɉ�����������
    public void InitializeScene()
    {
       // currentSceneType = (SceneType)UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;

        // �V�[���^�C�v���Ƃ̏�����
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

    // �^�C�g���V�[���̏�����
    void InitializeTitleScene()
    {
        Debug.Log("Title Scene Initialized");
        UnlockCursor();
    }

    // �Q�[���V�[���̏�����
    void InitializeGameScene()
    {
        Debug.Log("Game Scene Initialized");

        // �J�[�\�������b�N����\��
        LockCursor();

        // �X�e�[�W�̏�����
        StageManager.Instance.SetupStage(currentStage);

        // �v���C���[�X�|�[��
        PlayerManager.Instance.Revive(StageManager.Instance.GetPlayerSpawnPoint());
    }

    // ���U���g�V�[���̏�����
    void InitializeResultScene()
    {
        Debug.Log("Result Scene Initialized");
        UnlockCursor();
    }

    public void SpawnPlayer()
    {
        if (currentPlayer == null)
        {
            Debug.Log("�v���C���[���X�|�[�����܂�...");
            currentPlayer = Instantiate(playerPrefab, StageManager.Instance.GetPlayerSpawnPoint(), Quaternion.identity);
            DontDestroyOnLoad(currentPlayer);
            PlayerManager.Instance.ResetPlayer();
            PlayerManager.Instance.inputEnable();
        }
        else
        {
            Debug.Log("�����̃v���C���[���A�N�e�B�u�����܂��B");
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
            Debug.Log("�v���C���[���A�N�e�B�u�����܂�...");
            currentPlayer.SetActive(false);
        }
    }

    public void ResetPlayerLives()
    {
        Debug.Log("�v���C���[�̎c�@�����Z�b�g���܂�...");
        playerLives = 3;
    }

    // �v���C���[�����S�����Ƃ��̏���
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

    // �v���C���[�̃��X�|�[������
    IEnumerator RespawnPlayer()
    {
        Debug.Log("Respawning Player...");
        yield return new WaitForSeconds(respawnDelay);

        PlayerManager.Instance.Revive(StageManager.Instance.GetPlayerSpawnPoint());
    }

    // �X�e�[�W�N���A����
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

    // �Q�[���I�[�o�[����
    public void OnGameOver()
    {
        lastGameResult = GameResult.GameOver;
        currentSceneType = SceneType.Result;
        SceneManager.Instance.LoadResultScene();
    }

    // �^�C�g���V�[���֖߂�ۂɃZ���N�g��ʂ�\������
    public void ReturnToTitle()
    {
        isSelectMode = true;
        currentSceneType = SceneType.Title;
        SceneManager.Instance.LoadTitleScene();
    }

    // �J�[�\�������b�N
    void LockCursor()
    {
        Debug.Log("�J�[�\����\��");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // �J�[�\��������
    void UnlockCursor()
    {
        Debug.Log("�J�[�\���\��");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StopVFXEffects()
    {
        VisualEffect[] effects = FindObjectsByType<VisualEffect>(FindObjectsSortMode.None);
        foreach (var effect in effects)
        {
            effect.Stop();  // �SVFX���~
        }
    }

    // �J�[�\���̏�Ԃ��擾���郁�\�b�h
    public bool GetCursorActive()
    {
        return Cursor.visible;
    }

    // ���݂̃X�e�[�W���擾���郁�\�b�h
    public int GetCurrentStage()
    {
        return currentStage;
    }

    public void SetCurrentStage(int stage)
    {
        currentStage = stage;
    }

    // �^�C�g���V�[���̏�Ԃ��擾���郁�\�b�h
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
