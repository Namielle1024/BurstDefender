using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    PlayerInput playerInput;
    PlayerAction playerAction;
    PlayerEffect playerEffect;

    // �Q�[����ԊǗ��t���O
    [NonSerialized]
    public bool isGameOver = false;

    void Awake()
    {
        // �V���O���g���p�^�[��
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // PlayerInput�̏�����
        playerInput = new PlayerInput();
        playerInput.Player.Enable();

        // PlayerAction��PlayerEffect�̃R���|�[�l���g���擾
        playerAction = GetComponent<PlayerAction>();
        playerEffect = GetComponent<PlayerEffect>();

        // InputSystem�̃A�N�V������ݒ�
        SetupInputActions();
    }

    private void SetupInputActions()
    {
        // �ړ��̓��͏���
        playerInput.Player.Move.performed += ctx => playerAction.OnMove(ctx.ReadValue<Vector2>(), true);
        playerInput.Player.Move.canceled += ctx => playerAction.OnMove(Vector2.zero, false);

        // �W�����v�̓��͏���
        playerInput.Player.Jump.performed += ctx => playerAction.OnJump();

        // ���̑��̃A�N�V����
        playerInput.Player.Sprint.performed += ctx => playerAction.OnSprint(true);
        playerInput.Player.Sprint.canceled += ctx => playerAction.OnSprint(false);

        // �C���^���N�g�̓��͏���
        playerInput.Player.Interact.performed += ctx => playerAction.OnInteract();

        // ��U���̓��͏���
        playerInput.Player.Weak_Attack.performed += ctx => playerAction.OnWeakAttack(true);
        playerInput.Player.Weak_Attack.canceled += ctx => playerAction.OnWeakAttack(false);

        // ���U���̓��͏���
        playerInput.Player.Strong_Attack.performed += ctx => playerAction.OnStrongAttack(true);
        playerInput.Player.Strong_Attack.canceled += ctx => playerAction.OnStrongAttack(false);

    }

    private void Update()
    {
        // �Q�[���I�[�o�[�̏�ԂōX�V���~
        if (isGameOver)
            return;

        // �����ŃQ�[���̏�Ԃ�i�s�ɉ������������L�q�ł��܂�
    }

    public void SetGameOver(bool state)
    {
        isGameOver = state;
        // �K�v�ɉ����āA�Q�[���I�[�o�[���̏�����ǉ�
    }
}
