using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    PlayerInput playerInput;
    PlayerAction playerAction;
    PlayerEffect playerEffect;

    [Header("Player Stats")]
    [SerializeField] // �v���C���[�̍ő�HP
    int maxHP = 100;
    int currentHP; // ���݂�HP

    [Header("Attack Settings")]
    public int weakAttackDamage = 10; // Hand�U���̃_���[�W��
    public int strongAttackDamage = 5; // JumpAttack�̃_���[�W��

    bool isDead = false; // �v���C���[�����S�������ǂ���
    bool isPaused = false;
    SceneType currentSceneType;

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

        // �Q�[���V�[���ȊO�ł� InputSystem �𖳌���
        if (SceneType.Game == currentSceneType) // �� �Q�[���V�[�����ɍ��킹�ĕύX
        {
            if (playerInput != null)
            {
                playerInput.Enable();
            }
        }
        else
        {
            if (playerInput != null)
            {
                playerInput.Disable();
            }
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
        playerInput.Player.Move.performed += ctx => { if (IsInputEnabled()) playerAction.OnMove(ctx.ReadValue<Vector2>(), true); };
        playerInput.Player.Move.canceled += ctx => { if (IsInputEnabled()) playerAction.OnMove(Vector2.zero, false); };

        // �W�����v
        playerInput.Player.Jump.performed += ctx => { if (IsInputEnabled()) playerAction.OnJump(); };

        // �_�b�V��
        playerInput.Player.Sprint.performed += ctx => { if (IsInputEnabled()) playerAction.OnSprint(true); };
        playerInput.Player.Sprint.canceled += ctx => { if (IsInputEnabled()) playerAction.OnSprint(false); };

        // �C���^���N�g
        playerInput.Player.Interact.performed += ctx => { if (IsInputEnabled()) playerAction.OnInteract(); };

        // �U��
        playerInput.Player.Weak_Attack.performed += ctx => { if (IsInputEnabled()) playerAction.OnAttack(false); };
        playerInput.Player.Strong_Attack.performed += ctx => { if (IsInputEnabled()) playerAction.OnAttack(true); };
    }


    // �v���C���[���_���[�W���󂯂��Ƃ��̏���
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        playerEffect.StopTrailEffect();

        // HP������
        currentHP -= damage;

        // HP�Q�[�W�X�V
        UIManager.Instance.UpdateHPBar(currentHP, maxHP);
        UIManager.Instance.ShowDamageEffect();
        playerAction.OnDamage();

        // �_���[�W�G�t�F�N�g�̍Đ�
        if (playerEffect != null)
        {
            playerEffect.PlayDamageEffect();
        }

        // HP��0�ȉ��̏ꍇ�A�Q�[���I�[�o�[���������s
        if (currentHP <= 0 && !isDead)
        {
            isDead = true;
            currentHP = 0;
            Death();
        }
    }

    // �v���C���[�����S�����Ƃ��̏���
    void Death()
    {
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
        // ���S�t���O������
        isDead = false;

        // HP���ő�l�ɉ�
        currentHP = maxHP;

        // �v���C���[�̈ʒu�����X�|�[���n�_�Ɉړ�
        transform.position = respawnPosition;

        // ���X�|�[���A�j���[�V����
        playerAction.OnRevived();

        // HP�Q�[�W���t���ɉ�
        UIManager.Instance.UpdateHPBar(currentHP, maxHP);

        // �G�t�F�N�g�̍Đ��i�������j
        if (playerEffect != null)
        {
        }
    }

    public void ResetPlayer()
    {
        currentHP = maxHP;
        playerAction.ResetState();
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
    /// �U����Ԃ�ݒ肷�郁�\�b�h
    /// </summary>
    /// <param name="isWeak"></param>
    /// <param name="isStrong"></param>
    public void SetAttackState(bool isWeak, bool isStrong)
    {
        PlayerAction.isWeakAttackingJudgement = isWeak;
        PlayerAction.isStrongAttackingJudgement = isStrong;
    }

    /// <summary>
    /// �v���C���[�̓��͏�Ԃ����Z�b�g
    /// </summary>
    public void ResetPlayerInput()
    {
        // �ړ����Z�b�g
        playerAction.OnMove(Vector2.zero, false);

        // �X�v�����g���Z�b�g
        playerAction.OnSprint(false);

        // PlayerInput�̍ėL�����i�K�v�Ȃ�j
        playerInput.Player.Disable();
        playerInput.Player.Enable();
    }

    /// <summary>
    /// ���͂��󂯕t�����Ԃ��𔻒�
    /// </summary>
    bool IsInputEnabled()
    {
        return !UIManager.Instance.IsPauseActive(); // Pause���͑��얳��
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
