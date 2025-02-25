using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] 
    GameObject startScreenUI;
    [SerializeField] 
    GameObject stageSelectUI;
    [SerializeField] 
    Button startButton;
    [SerializeField]
    Button exitButton;
    [SerializeField]
    Button backButton;
    [SerializeField]
    [NamedArray("Stage1 Button", "Stage2 Button", "Sample Button")]
    Button[] stageSelectButtons;

    bool ScreenMode;

    void Start()
    {
        ScreenMode = GameManager.Instance.GetSelectMode();

        if (ScreenMode)
        {
            ShowStageSelect();
        }
        else
        {
            ShowStartScreen();
        }

        startButton.onClick.AddListener(ShowStageSelect);
        exitButton.onClick.AddListener(OnExitButtonPressed);
        backButton.onClick.AddListener(ShowStartScreen);
    }

    /// <summary> 
    /// ������ʕ\��
    /// </summary>
    void ShowStartScreen()
    {
        startScreenUI.SetActive(true);
        stageSelectUI.SetActive(false);
    }

    /// <summary> 
    /// �X�e�[�W�I����ʕ\��
    /// </summary>
    void ShowStageSelect()
    {
        SetupStageButtons();
        startScreenUI.SetActive(false);
        stageSelectUI.SetActive(true);
    }

    /// <summary> 
    /// �X�^�[�g�{�^���������̏���
    /// </summary>
    public void OnExitButtonPressed()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false; // �G�f�B�^���~
#else
        Application.Quit(); // �r���h���ɃA�v�����I��
#endif
    }

    /// <summary>
    /// �X�e�[�W�{�^���̏����ݒ�
    /// </summary>
    private void SetupStageButtons()
    {
        for (int i = 0; i < stageSelectButtons.Length; i++)
        {
            // �T���v���{�^���͍Ō�̃{�^���Ɖ���
            bool isSampleButton = (i == stageSelectButtons.Length - 1);

            if (isSampleButton)
            {
#if UNITY_EDITOR
                int sampleStageIndex = i + 2;
                // �G�f�B�^�[��ł͏�ɃA�N�e�B�u�őJ�ډ\
                stageSelectButtons[i].gameObject.SetActive(true);
                stageSelectButtons[i].interactable = true;
                stageSelectButtons[i].onClick.RemoveAllListeners();
                stageSelectButtons[i].onClick.AddListener(() => SceneManager.Instance.LoadStage(sampleStageIndex)); // �T���v���V�[���̃X�e�[�WID
#else
            // �r���h���͔�\��
            stageSelectButtons[i].gameObject.SetActive(false);
#endif
            }
            else
            {
                // �ʏ�̃X�e�[�W�{�^���̓N���A�󋵂ɉ����ĉ��
                if (i < GameManager.Instance.GetUnlockedStages())
                {
                    int stageIndex = i + 2; // �X�e�[�W�ԍ���K�X�ݒ�
                    stageSelectButtons[i].interactable = true;
                    stageSelectButtons[i].onClick.RemoveAllListeners();
                    stageSelectButtons[i].onClick.AddListener(() => SceneManager.Instance.LoadStage(stageIndex));
                }
                else
                {
                    stageSelectButtons[i].interactable = false;
                }
            }
        }
    }
}
