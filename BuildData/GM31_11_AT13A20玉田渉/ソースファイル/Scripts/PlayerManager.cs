using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    PlayerInput playerInput;
    PlayerAction playerAction;
    PlayerEffect playerEffect;
    EnemyManager enemyManager;

    [Header("Player Stats")]
    [SerializeField] 
    int maxHP = 100; // �ő�HP
    [SerializeField] 
    float despawnDelay = 5.0f;
    int currentHP; // ���݂�HP

    [Header("Attack Settings")]
    public int weakAttackDamage = 10; // Hand�U���̃_���[�W��
    public int strongAttackDamage = 5; // JumpAttack�̃_���[�W��

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

        // �R���|�[�l���g�擾
        playerAction = GetComponent<PlayerAction>();
        playerEffect = GetComponent<PlayerEffect>();

        // InputSystem�̃A�N�V������ݒ�
        SetupInputActions();


        // HP�̏�����
        currentHP = maxHP;
    }

    void SetupInputActions()
    {
        // �ړ��̓��͏���
        playerInput.Player.Move.performed += ctx => playerAction.OnMove(ctx.ReadValue<Vector2>(), true);
        playerInput.Player.Move.canceled += ctx => playerAction.OnMove(Vector2.zero, false);

        // �W�����v�̓��͏���
        playerInput.Player.Jump.performed += ctx => playerAction.OnJump();

        // �_�b�V���̓��͏���
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

    void Update()
    {
        // �����ŃQ�[���̏�Ԃ�i�s�ɉ������������L�q
    }

    public void SetGameOver(bool state)
    {
        //isGameOver = state;
        // �K�v�ɉ����āA�Q�[���I�[�o�[���̏�����ǉ�
    }

    void OnTriggerEnter(Collider other)
    {
        // �G�̖���������ꍇ
        if (other.CompareTag("Arrow"))
        {
            //TakeDamage(enemyManager.attackDamage);
            TakeDamage(10);
        }
    }

    // �v���C���[���_���[�W���󂯂��Ƃ��̏���
    public void TakeDamage(int damage)
    {
        // HP������
        currentHP -= damage;
        Debug.Log($"�v���C���[��{damage}�̃_���[�W���󂯂��B ���݂�HP: {currentHP}");

        playerAction.OnDamage();

        // �_���[�W�G�t�F�N�g�̍Đ��i�������j
        if (playerEffect != null)
        {
            playerEffect.PlayDamageEffect();
        }

        // HP��0�ȉ��̏ꍇ�A�Q�[���I�[�o�[���������s
        if (currentHP <= 0)
        {
            currentHP = 0;
            Death();
        }
    }

    // �v���C���[�����S�����Ƃ��̏���
    void Death()
    {
        //SetGameOver(true);

        playerAction.OnDeath();
        GameManager.Instance.OnPlayerDeath();

        // ����𖳌���
        playerInput.Player.Disable();

        // ���S�G�t�F�N�g�̍Đ��i�������j
        if (playerEffect != null)
        {
            playerEffect.PlayDeathEffect();
        }
    }

    // �v���C���[�𕜊������鏈��
    public void Revive(Vector3 respawnPosition)
    {
        Debug.Log("Reviving Player...");

        // HP���ő�l�ɉ�
        currentHP = maxHP;

        // �v���C���[�̈ʒu�����X�|�[���n�_�Ɉړ�
        transform.position = respawnPosition;

        // ���X�|�[���A�j���[�V����
        playerAction.OnRevived();

        // �����L����
        playerInput.Player.Enable();

        //// PlayerAction�̃��Z�b�g
        //if (playerAction != null)
        //{
        //    playerAction.OnRevive();
        //}

        //// �G�t�F�N�g�̍Đ��i�������j
        //if (playerEffect != null)
        //{
        //    playerEffect.PlayReviveEffect();
        //}
    }

    /// <summary>
    /// InputSystem��L�����ɂ��郁�\�b�h
    /// </summary>
    public void inputEnable()
    {
        playerInput.Player.Enable();
    }

    /// <summary>
    /// InputSystem�𖳌����ɂ��郁�\�b�h
    /// </summary>
    public void inputDisable()
    {
        playerInput.Player.Disable();
    }

    /// <summary>
    /// HP�̃Q�b�^�[
    /// </summary>
    /// <returns> currentHP </returns>
    public int GetCurrentHP()
    {
        return currentHP;
    }

    /// <summary>
    /// �V�[���J�ڎ��Ƀv���C���[�J�������Z�b�g���郁�\�b�h
    /// (playerAction�ւ̋��n��)
    /// </summary>
    /// <param name="camera"></param>
    public void SetCamera(Camera camera)
    {
        playerAction.SetCamera(camera);
    }
}
