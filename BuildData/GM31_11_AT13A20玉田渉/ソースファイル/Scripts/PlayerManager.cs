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
    int maxHP = 100; // 最大HP
    [SerializeField] 
    float despawnDelay = 5.0f;
    int currentHP; // 現在のHP

    [Header("Attack Settings")]
    public int weakAttackDamage = 10; // Hand攻撃のダメージ量
    public int strongAttackDamage = 5; // JumpAttackのダメージ量

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
        playerInput.Player.Move.performed += ctx => playerAction.OnMove(ctx.ReadValue<Vector2>(), true);
        playerInput.Player.Move.canceled += ctx => playerAction.OnMove(Vector2.zero, false);

        // ジャンプの入力処理
        playerInput.Player.Jump.performed += ctx => playerAction.OnJump();

        // ダッシュの入力処理
        playerInput.Player.Sprint.performed += ctx => playerAction.OnSprint(true);
        playerInput.Player.Sprint.canceled += ctx => playerAction.OnSprint(false);

        // インタラクトの入力処理
        playerInput.Player.Interact.performed += ctx => playerAction.OnInteract();

        // 弱攻撃の入力処理
        playerInput.Player.Weak_Attack.performed += ctx => playerAction.OnWeakAttack(true);
        playerInput.Player.Weak_Attack.canceled += ctx => playerAction.OnWeakAttack(false);

        // 強攻撃の入力処理
        playerInput.Player.Strong_Attack.performed += ctx => playerAction.OnStrongAttack(true);
        playerInput.Player.Strong_Attack.canceled += ctx => playerAction.OnStrongAttack(false);

    }

    void Update()
    {
        // ここでゲームの状態や進行に応じた処理を記述
    }

    public void SetGameOver(bool state)
    {
        //isGameOver = state;
        // 必要に応じて、ゲームオーバー時の処理を追加
    }

    void OnTriggerEnter(Collider other)
    {
        // 敵の矢が当たった場合
        if (other.CompareTag("Arrow"))
        {
            //TakeDamage(enemyManager.attackDamage);
            TakeDamage(10);
        }
    }

    // プレイヤーがダメージを受けたときの処理
    public void TakeDamage(int damage)
    {
        // HPを減少
        currentHP -= damage;
        Debug.Log($"プレイヤーは{damage}のダメージを受けた。 現在のHP: {currentHP}");

        playerAction.OnDamage();

        // ダメージエフェクトの再生（仮実装）
        if (playerEffect != null)
        {
            playerEffect.PlayDamageEffect();
        }

        // HPが0以下の場合、ゲームオーバー処理を実行
        if (currentHP <= 0)
        {
            currentHP = 0;
            Death();
        }
    }

    // プレイヤーが死亡したときの処理
    void Death()
    {
        //SetGameOver(true);

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
        Debug.Log("Reviving Player...");

        // HPを最大値に回復
        currentHP = maxHP;

        // プレイヤーの位置をリスポーン地点に移動
        transform.position = respawnPosition;

        // リスポーンアニメーション
        playerAction.OnRevived();

        // 操作を有効化
        playerInput.Player.Enable();

        //// PlayerActionのリセット
        //if (playerAction != null)
        //{
        //    playerAction.OnRevive();
        //}

        //// エフェクトの再生（復活時）
        //if (playerEffect != null)
        //{
        //    playerEffect.PlayReviveEffect();
        //}
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
    /// シーン遷移時にプレイヤーカメラをセットするメソッド
    /// (playerActionへの橋渡し)
    /// </summary>
    /// <param name="camera"></param>
    public void SetCamera(Camera camera)
    {
        playerAction.SetCamera(camera);
    }
}
