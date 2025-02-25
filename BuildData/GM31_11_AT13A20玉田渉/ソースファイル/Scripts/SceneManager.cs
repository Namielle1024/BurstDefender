using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance;

    const int TITLE_SCENE_INDEX = 0;
    const int GAME_SCENE_INDEX_START = 1;
    const int RESULT_SCENE_INDEX = 2;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // �V�[�����[�h���̃C�x���g�o�^
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // �V�[�������[�h���ꂽ�Ƃ��Ɏ��s����郁�\�b�h
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"�V�[�������[�h����܂���: {scene.name}");

        if (scene.buildIndex >= GAME_SCENE_INDEX_START && scene.buildIndex < RESULT_SCENE_INDEX)
        {
            GameManager.Instance.SpawnPlayer();
            VisualEffect[] effects = FindObjectsByType<VisualEffect>(FindObjectsSortMode.None);
            foreach (var effect in effects)
            {
                effect.Stop();  // �SVFX���~
            }
            Camera gameCamera = FindAnyObjectByType<Camera>();
            if (gameCamera != null)
            {
                PlayerManager.Instance.SetCamera(gameCamera);
                Debug.Log("�J������ݒ肵�܂����B");
            }
            else
            {
                Debug.LogWarning("�J������������܂���I");
            }
        }
        else
        {
            GameManager.Instance.RemovePlayer();
        }

        // GameManager�̃V�[���^�C�v��ݒ�
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
        // �V�[���A�����[�h���ɃC�x���g������
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // �V�[�������[�h
    public void LoadScene(int sceneIndex)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }

    // �^�C�g���V�[���֑J�ځi��ɃZ���N�g���[�h�ցj
    public void LoadTitleScene()
    {
        LoadScene(TITLE_SCENE_INDEX);
    }

    // �w�肵���X�e�[�W�Ɉړ�
    public void LoadStage(int stageIndex)
    {
        GameManager.Instance.SetSceneType(SceneType.Game);
        LoadScene(stageIndex);
    }

    // ���U���g�V�[���ֈړ�
    public void LoadResultScene()
    {
        LoadScene(RESULT_SCENE_INDEX);
    }
}
