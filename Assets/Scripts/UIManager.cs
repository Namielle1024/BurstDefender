using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    bool isGameScene = false; // �Q�[���V�[�����𔻕ʂ���t���O

    [Header("UI Prefabs")]
    [SerializeField] // ���ׂĂ�UI���܂Ƃ߂�Prefab
    GameObject uiPrefab;
    GameObject instantiatedUI;
    UIController uiController;

    /// <summary>
    /// �V���O���g����
    /// </summary>
    void Awake()
    {
        // �V���O���g����
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// �V�[�������[�h���ꂽ�Ƃ��ɌĂ΂�郁�\�b�h
    /// false : �Q�[��UI��j������
    /// true  : �Q�[��UI�𐶐�����
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // �Q�[���V�[�����ǂ����𔻒�
        isGameScene = GameManager.Instance.currentSceneType == SceneType.Game;

        if (isGameScene)
        {
            InitializeUI();
        }
        else
        {
            DestroyUI();
        }

        if (instantiatedUI != null)
        {
            instantiatedUI.SetActive(isGameScene);
        }
    }

    /// <summary>
    /// UI�̐���
    /// </summary>
    void InitializeUI()
    {
        if (instantiatedUI == null)
        {
            instantiatedUI = Instantiate(uiPrefab, transform);

            // UIController ���擾
            uiController = instantiatedUI.GetComponent<UIController>();

            if (uiController == null)
            {
                Debug.LogError("UIController��uiPrefab�ɃA�^�b�`����Ă��܂���");
            }
        }
    }

    /// <summary>
    /// UI�̔j��
    /// </summary>
    void DestroyUI()
    {
        if (instantiatedUI != null)
        {
            Destroy(instantiatedUI);
            instantiatedUI = null;
            uiController = null;
            Debug.Log("UIManager: �Q�[���V�[���ȊO�̂���UI���폜���܂���");
        }
    }

    /// <summary>
    /// �|�[�Y����UI��\���A��\���ݒ�
    /// </summary>
    /// <param name="pause"></param>
    public void OnPause(bool pause)
    {
        if (instantiatedUI != null)
        {
            uiController.OnPause(pause);
            PlayerManager.Instance.ResetPlayerInput();
        }
    }

    /// <summary>
    /// �Q�[���V�[����UI���A�N�e�B�u�ɂ���
    /// false: �SUI���\��
    /// true : StartUI�ȊO��\��
    /// </summary>
    /// <param name="active"></param>
    public void SetActiveGameUI(bool active)
    {
        if (instantiatedUI != null)
        {
            uiController.SetActiveGameUI(active);
        }
    }

    /// <summary>
    /// �E�F�[�u�J�n����UI��\��
    /// </summary>
    /// <param name="waveNumber"></param>
    public void ShowWaveStart(int waveNumber)
    {
        if (instantiatedUI != null)
        {
            uiController.ShowWaveStart(waveNumber);
        }
    }

    /// <summary>
    /// �E�F�[�u�N���A����UI��\��
    /// </summary>
    /// <param name="waveNumber"></param>
    public void ShowWaveComplete(int waveNumber)
    {
        if (instantiatedUI != null)
        {
            uiController.ShowWaveComplete(waveNumber);
        }
    }

    /// <summary>
    /// ���݃E�F�[�u����ݒ�
    /// </summary>
    /// <param name="waveNumber"></param>
    public void SetCurrentWave(int waveNumber)
    {
        if (instantiatedUI != null)
        {
            uiController.SetCurrentWave(waveNumber);
        }
    }

    /// <summary>
    /// �v���C���[��HP�o�[���X�V
    /// </summary>
    /// <param name="currentHP"></param>
    /// <param name="maxHP"></param>
    public void UpdateHPBar(float currentHP, float maxHP)
    {
        if (instantiatedUI != null)
        {
            uiController.UpdateHPBar(currentHP, maxHP);
        }
    }

    /// <summary>
    /// �_���[�W�G�t�F�N�g��\��
    /// </summary>
    public void ShowDamageEffect()
    {
        if (instantiatedUI != null)
        {
            uiController.ShowDamageEffect();
        }
    }

    /// <summary>
    /// �G�̎c�萔UI���X�V
    /// </summary>
    /// <param name="remaining"></param>
    /// <param name="total"></param>
    public void UpdateEnemyGauge(int remaining, int total)
    {
        if (instantiatedUI != null)
        {
            uiController.UpdateEnemyGauge(remaining, total);
        }
    }

    /// <summary>
    /// �|�[�Y��ʂ��A�N�e�B�u���ǂ�����Ԃ�
    /// true  : �A�N�e�B�u
    /// false : ��A�N�e�B�u
    /// </summary>
    /// <returns></returns>
    public bool IsPauseActive()
    {
        return uiController.IsPauseActive();
    }
}
