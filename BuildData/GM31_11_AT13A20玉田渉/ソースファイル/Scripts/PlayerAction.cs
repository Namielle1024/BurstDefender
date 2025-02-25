using System;
using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    Rigidbody rb;
    Animator animator;

    [Header("Movement Settings")]
    [SerializeField] 
    float moveSpeed = 5f;
    [SerializeField] 
    float attackMoveSpeed = 2f;
    [SerializeField] 
    float jumpForce = 10f; // 基本ジャンプ力
    [SerializeField] 
    float jumpDuration = 0.5f; // 加速的に上昇するジャンプの持続時間
    [SerializeField] 
    float gravity = 20f; // 重力補正
    [SerializeField] 
    LayerMask groundLayer;
    [SerializeField] 
    new Camera camera;

    Vector2 moveInput;
    float nowMoveSpeed;
    float groundDistance = 0.5f;
    float currentJumpTime;
    bool isJumping = false;     // ジャンプ中のフラグ   
    bool isAttacking = false;   // 攻撃中フラグ
    bool isStrongAttacking = false;   // 強攻撃中フラグ
    bool isDamaging = false; // 被ダメージ中フラグ
    [NonSerialized]
    public static bool isWeakAttackingJudgement = false;   // 弱攻撃当たり判定制御用フラグ
    [NonSerialized]
    public static bool isStrongAttackingJudgement = false; // 強攻撃当たり判定制御用フラグ
    Quaternion attackDirection; // 攻撃中の向き

    // 無操作監視用の変数
    float idleTime = 0.0f;
    float idleThreshold = 5.0f; // 無操作状態が続く時間（秒）
    bool isIdling = false;

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
        //Debug.Log("接地判定" + bGrounded);

        // カメラの方向に基づいて移動方向を計算
        Vector3 forward = camera.transform.forward;
        Vector3 right = camera.transform.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        // カメラの向きに基づいて移動方向を決定
        Vector3 moveDir = (forward * moveInput.y + right * moveInput.x).normalized;
        Vector3 attackMoveDir = (transform.forward * moveInput.x + transform.right * -moveInput.y).normalized;
        if (isStrongAttacking)
        {
            rb.linearVelocity = attackDirection * attackMoveDir * nowMoveSpeed + new Vector3(0.0f, rb.linearVelocity.y, 0.0f);
        }
        else
        {
            // 通常の移動
            rb.linearVelocity = moveDir * nowMoveSpeed + new Vector3(0.0f, rb.linearVelocity.y, 0.0f);
            if (moveDir != Vector3.zero && !isAttacking)
            {
                // プレイヤーの向きを移動方向に合わせる
                transform.rotation = Quaternion.LookRotation(moveDir);
            }
            if(isAttacking)
            {
                transform.rotation = Quaternion.LookRotation(forward);
            }
        }

        if (moveInput == Vector2.zero)
        {
            isIdling = true;
        }

        // ジャンプ処理
        if (isJumping)
        {
            HandleJump();
        }

        // 着地処理
        if (isJumping && bGrounded)
        {
            isJumping = false;
            animator.SetBool("Jump", false);
        }

        // 無操作状態のカウント
        if (isIdling)
        {
            idleTime += Time.deltaTime;
            if (idleTime >= idleThreshold)
            {
                animator.SetBool("Idle", true);
            }
            if (idleTime >= idleThreshold * 2)
            {
                animator.SetBool("Idle", false);
                isIdling = false;
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
        if (isDamaging) return;

        // ジャンプ処理
        if (!isJumping && !isAttacking && CheckIfGrounded())
        {
            //Debug.Log("ジャンプ");
            ResetIdleState();
            isJumping = true;
            currentJumpTime = 0.0f;
            if (animator != null)
            {
                animator.SetBool("Jump", true);
            }
        }
    }

    public void OnInteract()
    {
        if(isDamaging) return;

        // インタラクトのアクションをトリガー
        Debug.Log("Interact action triggered");
        // ここで、エフェクトやアニメーションを開始するなどの処理が可能
    }

    public void OnSprint(bool isSprinting)
    {
        if (isDamaging) return;

        if (!isAttacking) // 攻撃中はスプリントできない
        {
            ResetIdleState();
            //Debug.Log("走る");
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
        if (isDamaging) return;

        if (weak)
        {
            StartAttack();
        }
        //Debug.Log("弱攻撃");
        if (animator != null)
        {
            animator.SetBool("WeakAttack", weak);
        }
    }

    public void OnStrongAttack(bool strong)
    {
        if (isDamaging) return;

        if (strong)
        {
            isStrongAttacking = strong;
            attackDirection = transform.rotation;  // 現在の向きを固定
            StartAttack();
        }
        //Debug.Log("強攻撃");
        if (animator != null)
        {
            animator.SetBool("StrongAttack", strong);
        }
    }

    /// <summary>
    /// ダメージを受けたときに呼び出すメソッド
    /// </summary>
    public void OnDamage()
    {
        if (animator != null)
        {
            isDamaging = true;
            animator.Play("Mutant Damage");
        }
    }

    /// <summary>
    /// HPが0になったときに呼び出すメソッド
    /// </summary>
    public void OnDeath()
    {
        if (animator != null)
        {
            animator.Play("Mutant Dying");
        }
    }

    /// <summary>
    /// プレイヤーがリスポーンするときに呼ばれるメソッド
    /// </summary>
    public void OnRevived()
    {
        if(animator != null)
        {
            animator.Play("Mutant Revived");
        }
    }

    /// <summary>
    /// 攻撃ボタンが押されたときに呼び出すメソッド(AnimatorEventではない)
    /// </summary>
    void StartAttack()
    {
        ResetIdleState();
        isAttacking = true;
        nowMoveSpeed = attackMoveSpeed;  // 移動速度を遅くする
    }

    /// <summary>
    /// 地面接地判定用のメソッド
    /// </summary>
    bool CheckIfGrounded()
    {
        // プレイヤーの位置から真下にレイキャストを飛ばして、地面接地をチェック
        Vector3 rayPos = transform.position + new Vector3(0.0f, 0.1f, 0.0f);
        Ray ray = new Ray(rayPos, Vector3.down);
        Debug.DrawRay(rayPos, Vector3.down * groundDistance, Color.red);
        return Physics.Raycast(ray, groundDistance, groundLayer);
    }
    
    /// <summary>
    /// ジャンプ時に呼ばれるメソッド
    /// </summary>
    void HandleJump()
    {
        if (currentJumpTime < jumpDuration)
        {
            currentJumpTime += Time.deltaTime;
            float jumpProgress = currentJumpTime / jumpDuration;
            float jumpSpeed = Mathf.Lerp(jumpForce, 0, jumpProgress); // 加速度的に減少
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpSpeed, rb.linearVelocity.z);
        }
        else
        {
            rb.linearVelocity += Vector3.down * gravity * Time.deltaTime; // 重力適用
        }
    }

    /// <summary>
    /// 操作が行われたら無操作タイマーとフラグをリセットするメソッド
    /// </summary>
    void ResetIdleState()
    {
        idleTime = 0f;
        isIdling = false;
        animator.SetBool("Idle", false);
    }

    /// <summary>
    /// Animatorで呼び出すメソッド(弱攻撃)
    /// </summary>
    void WeakActionEvent()
    {
        isWeakAttackingJudgement = true;
    }

    /// <summary>
    /// Animatorで呼び出すメソッド(強攻撃)
    /// </summary>
    void StrongActionEvent()
    {
        isStrongAttackingJudgement = true;
    }

    /// <summary>
    /// Animatorで呼び出すメソッド
    /// 攻撃終了時に呼び出す
    /// </summary>
    void EndAttackEvent()
    {
        isAttacking = false;
        isStrongAttacking = false;
        isWeakAttackingJudgement = false;
        isStrongAttackingJudgement = false;
        nowMoveSpeed = moveSpeed;
    }

    /// <summary>
    /// Animatorで呼び出すメソッド
    /// 被ダメージ後に呼び出す
    /// </summary>
    void EndDamage()
    {
        isDamaging = false;
    }

    // シーン遷移時にカメラを切り替えるメソッド
    public void SetCamera(Camera newCamera)
    {
        camera = newCamera;
    }
}
