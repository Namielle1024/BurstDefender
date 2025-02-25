using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    PlayerInput playerInput;
    PlayerAction playerAction;
    PlayerEffect playerEffect;

    // ゲーム状態管理フラグ
    [NonSerialized]
    public bool isGameOver = false;

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

        // PlayerActionとPlayerEffectのコンポーネントを取得
        playerAction = GetComponent<PlayerAction>();
        playerEffect = GetComponent<PlayerEffect>();

        // InputSystemのアクションを設定
        SetupInputActions();
    }

    private void SetupInputActions()
    {
        // 移動の入力処理
        playerInput.Player.Move.performed += ctx => playerAction.OnMove(ctx.ReadValue<Vector2>(), true);
        playerInput.Player.Move.canceled += ctx => playerAction.OnMove(Vector2.zero, false);

        // ジャンプの入力処理
        playerInput.Player.Jump.performed += ctx => playerAction.OnJump();

        // その他のアクション
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

    private void Update()
    {
        // ゲームオーバーの状態で更新を停止
        if (isGameOver)
            return;

        // ここでゲームの状態や進行に応じた処理を記述できます
    }

    public void SetGameOver(bool state)
    {
        isGameOver = state;
        // 必要に応じて、ゲームオーバー時の処理を追加
    }
}
