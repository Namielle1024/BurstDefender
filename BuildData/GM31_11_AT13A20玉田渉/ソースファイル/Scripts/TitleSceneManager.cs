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

    // 初期画面表示
    public void ShowStartScreen()
    {
        startScreenUI.SetActive(true);
        stageSelectUI.SetActive(false);
    }

    // ステージ選択画面表示
    public void ShowStageSelect()
    {
        SetupStageButtons();
        startScreenUI.SetActive(false);
        stageSelectUI.SetActive(true);
    }

    // スタートボタン押下時の処理
    public void OnStartButtonPressed()
    {
        ScreenMode = true;
        ShowStageSelect();
    }

    // ステージボタンの初期設定
    private void SetupStageButtons()
    {
        for (int i = 0; i < stageButtons.Length; i++)
        {
            if (i < GameManager.Instance.GetUnlockedStages())
            {
                int stageIndex = i + 1; // ステージ番号
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
