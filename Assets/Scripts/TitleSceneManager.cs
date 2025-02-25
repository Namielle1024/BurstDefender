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
    /// 初期画面表示
    /// </summary>
    void ShowStartScreen()
    {
        startScreenUI.SetActive(true);
        stageSelectUI.SetActive(false);
    }

    /// <summary> 
    /// ステージ選択画面表示
    /// </summary>
    void ShowStageSelect()
    {
        SetupStageButtons();
        startScreenUI.SetActive(false);
        stageSelectUI.SetActive(true);
    }

    /// <summary> 
    /// スタートボタン押下時の処理
    /// </summary>
    public void OnExitButtonPressed()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false; // エディタを停止
#else
        Application.Quit(); // ビルド時にアプリを終了
#endif
    }

    /// <summary>
    /// ステージボタンの初期設定
    /// </summary>
    private void SetupStageButtons()
    {
        for (int i = 0; i < stageSelectButtons.Length; i++)
        {
            // サンプルボタンは最後のボタンと仮定
            bool isSampleButton = (i == stageSelectButtons.Length - 1);

            if (isSampleButton)
            {
#if UNITY_EDITOR
                int sampleStageIndex = i + 2;
                // エディター上では常にアクティブで遷移可能
                stageSelectButtons[i].gameObject.SetActive(true);
                stageSelectButtons[i].interactable = true;
                stageSelectButtons[i].onClick.RemoveAllListeners();
                stageSelectButtons[i].onClick.AddListener(() => SceneManager.Instance.LoadStage(sampleStageIndex)); // サンプルシーンのステージID
#else
            // ビルド時は非表示
            stageSelectButtons[i].gameObject.SetActive(false);
#endif
            }
            else
            {
                // 通常のステージボタンはクリア状況に応じて解放
                if (i < GameManager.Instance.GetUnlockedStages())
                {
                    int stageIndex = i + 2; // ステージ番号を適宜設定
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
