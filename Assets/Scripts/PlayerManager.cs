using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    PlayerInput playerInput;
    PlayerAction playerAction;
    PlayerEffect playerEffect;

    [Header("Player Stats")]
    [SerializeField] // プレイヤーの最大HP
    int maxHP = 100;
    int currentHP; // 現在のHP

    [Header("Attack Settings")]
    public int weakAttackDamage = 10; // Hand攻撃のダメージ量
    public int strongAttackDamage = 5; // JumpAttackのダメージ量

    bool isDead = false; // プレイヤーが死亡したかどうか
    bool isPaused = false;
    SceneType currentSceneType;

    void Awake()
    {
        // シングルトンパターン
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // ゲームシーン以外では InputSystem を無効化
        if (SceneType.Game == currentSceneType) // ← ゲームシーン名に合わせて変更
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

        // PlayerInputの初期化
        playerInput = new PlayerInput();
        playerInput.Player.Enable();

        // コンポーネント取得
        playerAction = GetComponent<PlayerAction>();
        playerEffect = GetComponent<PlayerEffect>();

        // InputSystemのアクションを設定
        SetupInputActions();


        // HPの初期化
        currentHP = maxHP;
    }

    void SetupInputActions()
    {
        // 移動の入力処理
        playerInput.Player.Move.performed += ctx => { if (IsInputEnabled()) playerAction.OnMove(ctx.ReadValue<Vector2>(), true); };
        playerInput.Player.Move.canceled += ctx => { if (IsInputEnabled()) playerAction.OnMove(Vector2.zero, false); };

        // ジャンプ
        playerInput.Player.Jump.performed += ctx => { if (IsInputEnabled()) playerAction.OnJump(); };

        // ダッシュ
        playerInput.Player.Sprint.performed += ctx => { if (IsInputEnabled()) playerAction.OnSprint(true); };
        playerInput.Player.Sprint.canceled += ctx => { if (IsInputEnabled()) playerAction.OnSprint(false); };

        // インタラクト
        playerInput.Player.Interact.performed += ctx => { if (IsInputEnabled()) playerAction.OnInteract(); };

        // 攻撃
        playerInput.Player.Weak_Attack.performed += ctx => { if (IsInputEnabled()) playerAction.OnAttack(false); };
        playerInput.Player.Strong_Attack.performed += ctx => { if (IsInputEnabled()) playerAction.OnAttack(true); };
    }


    // プレイヤーがダメージを受けたときの処理
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        playerEffect.StopTrailEffect();

        // HPを減少
        currentHP -= damage;

        // HPゲージ更新
        UIManager.Instance.UpdateHPBar(currentHP, maxHP);
        UIManager.Instance.ShowDamageEffect();
        playerAction.OnDamage();

        // ダメージエフェクトの再生
        if (playerEffect != null)
        {
            playerEffect.PlayDamageEffect();
        }

        // HPが0以下の場合、ゲームオーバー処理を実行
        if (currentHP <= 0 && !isDead)
        {
            isDead = true;
            currentHP = 0;
            Death();
        }
    }

    // プレイヤーが死亡したときの処理
    void Death()
    {
        playerAction.OnDeath();
        GameManager.Instance.OnPlayerDeath();

        // 操作を無効化
        playerInput.Player.Disable();

        // 死亡エフェクトの再生（仮実装）
        if (playerEffect != null)
        {
            playerEffect.PlayDeathEffect();
        }
    }

    // プレイヤーを復活させる処理
    public void Revive(Vector3 respawnPosition)
    {
        // 死亡フラグを解除
        isDead = false;

        // HPを最大値に回復
        currentHP = maxHP;

        // プレイヤーの位置をリスポーン地点に移動
        transform.position = respawnPosition;

        // リスポーンアニメーション
        playerAction.OnRevived();

        // HPゲージをフルに回復
        UIManager.Instance.UpdateHPBar(currentHP, maxHP);

        // エフェクトの再生（復活時）
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
    /// InputSystemを有効化にするメソッド
    /// </summary>
    public void inputEnable()
    {
        playerInput.Player.Enable();
    }

    /// <summary>
    /// InputSystemを無効化にするメソッド
    /// </summary>
    public void inputDisable()
    {
        playerInput.Player.Disable();
    }

    /// <summary>
    /// HPのゲッター
    /// </summary>
    /// <returns> currentHP </returns>
    public int GetCurrentHP()
    {
        return currentHP;
    }

    /// <summary>
    /// 攻撃状態を設定するメソッド
    /// </summary>
    /// <param name="isWeak"></param>
    /// <param name="isStrong"></param>
    public void SetAttackState(bool isWeak, bool isStrong)
    {
        PlayerAction.isWeakAttackingJudgement = isWeak;
        PlayerAction.isStrongAttackingJudgement = isStrong;
    }

    /// <summary>
    /// プレイヤーの入力状態をリセット
    /// </summary>
    public void ResetPlayerInput()
    {
        // 移動リセット
        playerAction.OnMove(Vector2.zero, false);

        // スプリントリセット
        playerAction.OnSprint(false);

        // PlayerInputの再有効化（必要なら）
        playerInput.Player.Disable();
        playerInput.Player.Enable();
    }

    /// <summary>
    /// 入力を受け付ける状態かを判定
    /// </summary>
    bool IsInputEnabled()
    {
        return !UIManager.Instance.IsPauseActive(); // Pause中は操作無効
    }

    /// <summary>
    /// シーン遷移時にプレイヤーカメラをセットするメソッド
    /// (playerActionへの橋渡し)
    /// </summary>
    /// <param name="camera"></param>
    public void SetCamera(Camera camera)
    {
        playerAction.SetCamera(camera);
    }
}
