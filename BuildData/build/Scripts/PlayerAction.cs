using System;
using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    Rigidbody rb;
    Animator animator;

    [SerializeField] float moveSpeed;           // 移動速度
    [SerializeField] float attackMoveSpeed;     // 攻撃中の移動速度
    [SerializeField] float jumpForce;           // ジャンプ力
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform cameraTransform; // カメラのTransform    

    Vector2 moveInput;
    float nowMoveSpeed;
    float groundDistance = 0.5f;
    bool bJumping = false;     // ジャンプ中のフラグ   
    bool bAttacking = false;   // 攻撃中フラグ
    [NonSerialized]
    public static bool bWeakAttackJudgment = false;   // 弱攻撃当たり判定制御用フラグ
    [NonSerialized]
    public static bool bStrongAttackJudgment = false; // 強攻撃当たり判定制御用フラグ
    Quaternion attackDirection; // 攻撃中の向き

    // 無操作監視用の変数
    float idleTime = 0.0f;
    float idleThreshold = 5.0f; // 無操作状態が続く時間（秒）
    bool bIdle = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        nowMoveSpeed = moveSpeed;
    }

    void FixedUpdate()
    {
        // レイキャストで接地判定を実施
        bool bGrounded = CheckIfGrounded();
        Debug.Log("接地判定" + bGrounded);

        // カメラの方向に基づいて移動方向を計算
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // 水平移動のためY成分を0に
        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        // カメラの向きに基づいて移動方向を決定
        Vector3 moveDir = (forward * moveInput.y + right * moveInput.x).normalized;

        if (bAttacking)
        {
            // 攻撃中は移動速度を遅くし、向きを固定
            rb.linearVelocity = attackDirection * moveDir * nowMoveSpeed + new Vector3(0.0f, rb.linearVelocity.y, 0.0f);
        }
        else
        {
            // 通常の移動
            rb.linearVelocity = moveDir * nowMoveSpeed + new Vector3(0.0f, rb.linearVelocity.y, 0.0f);
            if (moveDir != Vector3.zero)
            {
                // プレイヤーの向きを移動方向に合わせる
                transform.rotation = Quaternion.LookRotation(moveDir);
            }
        }

        if (moveInput == Vector2.zero)
        {
            bIdle = true;
        }

        // プレイヤーが地面に接地しているかを判定
        if (bJumping && bGrounded)
        {
            bJumping = false;
            animator.SetBool("Jump", false);
        }

        // 無操作状態のカウント
        if (bIdle)
        {
            idleTime += Time.deltaTime;
            if (idleTime >= idleThreshold)
            {
                animator.SetBool("Idle", true);
            }
            if (idleTime >= idleThreshold * 2)
            {
                animator.SetBool("Idle", false);
                bIdle = false;
            }
        }
    }

    public void OnMove(Vector2 direction, bool walk)
    {
        moveInput = direction;

        // アニメーションの設定
        if (animator != null)
        {
            animator.SetBool("Walk", walk);
        }

        // プレイヤーが操作を開始した場合、無操作状態をリセット
        if (moveInput != Vector2.zero)
        {
            ResetIdleState();
        }
    }

    public void OnJump()
    {
        // ジャンプ処理
        if (!bJumping && !bAttacking && CheckIfGrounded())
        {
            Debug.Log("ジャンプ");
            ResetIdleState();
            bJumping = true;
            if (animator != null)
            {
                animator.SetBool("Jump", true);
            }
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void OnInteract()
    {
        // インタラクトのアクションをトリガー
        Debug.Log("Interact action triggered");
        // ここで、エフェクトやアニメーションを開始するなどの処理が可能
    }

    public void OnSprint(bool isSprinting)
    {
        if (!bAttacking) // 攻撃中はスプリントできない
        {
            ResetIdleState();
            Debug.Log("走る");
            // 移動速度を一時的に上げる
            nowMoveSpeed = isSprinting ? 8f : 5f;
            if (animator != null)
            {
                animator.SetBool("Run", isSprinting);
            }
        }
    }

    public void OnWeakAttack(bool weak)
    {
        if (weak)
        {
            StartAttack();
        }
        Debug.Log("弱攻撃");
        if (animator != null)
        {
            animator.SetBool("WeakAttack", weak);
        }
    }

    public void OnStrongAttack(bool strong)
    {
        if (strong)
        {
            StartAttack();
        }
        Debug.Log("強攻撃");
        if (animator != null)
        {
            animator.SetBool("StrongAttack", strong);
        }
    }

    /// <summary>
    /// 攻撃ボタンが押されたときに呼び出すメソッド(AnimatorEventではない)
    /// </summary>
    void StartAttack()
    {
        ResetIdleState();
        bAttacking = true;
        attackDirection = transform.rotation;  // 現在の向きを固定
        nowMoveSpeed = attackMoveSpeed;  // 移動速度を遅くする
    }

    /// <summary>
    /// 地面接地判定用のメソッド
    /// </summary>
    private bool CheckIfGrounded()
    {
        // プレイヤーの位置から真下にレイキャストを飛ばして、地面接地をチェック
        Vector3 rayPos = transform.position + new Vector3(0.0f, 0.1f, 0.0f);
        Ray ray = new Ray(rayPos, Vector3.down);
        Debug.DrawRay(rayPos, Vector3.down * groundDistance, Color.red);
        return Physics.Raycast(ray, groundDistance, groundLayer);
    }

    /// <summary>
    /// 操作が行われたら無操作タイマーとフラグをリセットするメソッド
    /// </summary>
    void ResetIdleState()
    {
        idleTime = 0f;
        bIdle = false;
        animator.SetBool("Idle", false);
    }

    /// <summary>
    /// Animatorで呼び出すメソッド(弱攻撃)
    /// </summary>
    void WeakActionEvent()
    {
        bWeakAttackJudgment = true;
    }

    /// <summary>
    /// Animatorで呼び出すメソッド(強攻撃)
    /// </summary>
    void StrongActionEvent()
    {
        bStrongAttackJudgment = true;
    }

    /// <summary>
    /// Animatorで呼び出すメソッド
    /// 攻撃終了時に呼び出す
    /// </summary>
    void EndAttackEvent()
    {
        bAttacking = false;
        bWeakAttackJudgment = false;
        bStrongAttackJudgment = false;
        nowMoveSpeed = moveSpeed;
    }
}
