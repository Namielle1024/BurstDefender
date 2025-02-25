using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    bool bCursorActive = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (instance == null)
            instance = this;

        SetCursorState(false);
    }

    // Update is called once per frame
    void Update()
    {
        // カーソル有効化処理
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetCursorState(true); // カーソル表示
        }
        else if (Input.GetMouseButtonDown(0) && bCursorActive)
        {
            SetCursorState(false); // カーソル非表示
        }
    }

    public bool GetCursorActive()
    {
        return bCursorActive;
    }

    private void SetCursorState(bool active)
    {
        bCursorActive = active;
        Cursor.visible = active;
        Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
