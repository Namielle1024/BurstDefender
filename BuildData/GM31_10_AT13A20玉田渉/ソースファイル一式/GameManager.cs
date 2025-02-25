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
        // �J�[�\���L��������
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetCursorState(true); // �J�[�\���\��
        }
        else if (Input.GetMouseButtonDown(0) && bCursorActive)
        {
            SetCursorState(false); // �J�[�\����\��
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
