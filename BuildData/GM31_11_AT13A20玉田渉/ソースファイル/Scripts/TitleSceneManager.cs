using UnityEngine;
using UnityEngine.UI;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] 
    GameObject startScreenUI;
    [SerializeField] 
    GameObject stageSelectUI;
    [SerializeField] 
    Button StartButton;
    [SerializeField] 
    Button[] stageButtons;

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

        StartButton.onClick.AddListener(ShowStageSelect);
    }

    // ������ʕ\��
    public void ShowStartScreen()
    {
        startScreenUI.SetActive(true);
        stageSelectUI.SetActive(false);
    }

    // �X�e�[�W�I����ʕ\��
    public void ShowStageSelect()
    {
        SetupStageButtons();
        startScreenUI.SetActive(false);
        stageSelectUI.SetActive(true);
    }

    // �X�^�[�g�{�^���������̏���
    public void OnStartButtonPressed()
    {
        ScreenMode = true;
        ShowStageSelect();
    }

    // �X�e�[�W�{�^���̏����ݒ�
    private void SetupStageButtons()
    {
        for (int i = 0; i < stageButtons.Length; i++)
        {
            if (i < GameManager.Instance.GetUnlockedStages())
            {
                int stageIndex = i + 1; // �X�e�[�W�ԍ�
                stageButtons[i].interactable = true;
                stageButtons[i].onClick.AddListener(() => SceneManager.Instance.LoadStage(stageIndex));
            }
            else
            {
                stageButtons[i].interactable = false;
            }
        }
    }
}
