using System;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    public enum PlayerState { Idle, Running, Jumping, Attacking, Damaged, Dead, Reviving }
    PlayerState currentState = PlayerState.Idle;

    Rigidbody rb;
    Animator animator;

    [Header("Movement Settings")]
    [SerializeField] // 移動速度
    float moveSpeed = 5f;
    [SerializeField] // スプリント時の速度
    float sprintSpeed = 8f;
    [SerializeField] // 攻撃時の移動速度
    float attackMoveSpeed = 2f;
    [SerializeField] // ジャンプの高さ
    float jumpHeight = 10f;
    [SerializeField] // ジャンプの持続時間
    float jumpDuration = 1.0f;
    [SerializeField] // 重力
    float gravity = 9.8f;
    [SerializeField] // ジャンプの動きを制御するカーブ
    AnimationCurve jumpCurve;
    [SerializeField] // 蘇生時間
    float reviveTime = 2.0f;
    [SerializeField] // 地面レイヤー
    LayerMask groundLayer;
    [SerializeField] // カメラ
    new Camera camera;

    [Header("Landing Settings")]
    [SerializeField] // 地面までの距離閾値
    float landingThreshold = 1.0f;
    [SerializeField] // 着地判定の間隔
    float landingCheckInterval = 0.5f;

    Vector2 moveInput;
    bool isJumping = false;
    bool isAttacking = false;
    bool isReviving = false;
    bool isLanding = false;
    float groundDistance = 0.5f;
    float idleTime = 0.0f;
    float idleThreshold = 10.0f;

    [NonSerialized]
    public static bool isWeakAttackingJudgement = false;   // 弱攻撃当たり判定制御用フラグ
    [NonSerialized]
    public static bool isStrongAttackingJudgement = false; // 強攻撃当たり判定制御用フラグ

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (currentState == PlayerState.Dead || isReviving) return;

        HandleMovement();
        ApplyGravity();
        HandleIdleState();

        if (isJumping && !isLanding) // ジャンプ中に着地判定を開始
        {
            CheckLanding();
        }
    }

    void HandleMovement()
    {
        bool isGrounded = CheckIfGrounded();
        Vector3 moveDir = GetCameraRelativeMovement(moveInput);

        if (isAttacking)
        {
            // 攻撃中は前入力(Wキー)のみ有効
            if (moveInput.y > 0) // Wキーが押されている場合
            {
                Vector3 forwardDir = transform.forward;

                // 前進速度を設定
                rb.linearVelocity = new Vector3(forwardDir.x * attackMoveSpeed, rb.linearVelocity.y, forwardDir.z * attackMoveSpeed);
            }
            else
            {
                // 前入力がない場合、その場で攻撃
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }

            // カメラの向きで回転を固定
            transform.rotation = Quaternion.Euler(0, camera.transform.eulerAngles.y, 0);
        }
        else
        {
            rb.linearVelocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);
            if (moveDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(moveDir);
                ChangeState(PlayerState.Running);
            }
            else if (isGrounded)
            {
                ChangeState(PlayerState.Idle);
            }
        }
    }

    void ApplyGravity()
    {
        if (!CheckIfGrounded())
        {
            rb.linearVelocity += Vector3.down * gravity * Time.deltaTime;
        }
    }

    void HandleIdleState()
    {
        if (moveInput == Vector2.zero)
        {
            idleTime += Time.deltaTime;

            if (idleTime >= idleThreshold)
            {
                idleTime = 0;
                animator.SetTrigger("Idle");
            }
        }
        else
        {
            idleTime = 0;
        }
    }

    bool CheckIfGrounded()
    {
        // プレイヤーの位置から真下にレイキャストを飛ばして、地面接地をチェック
        Vector3 rayPos = transform.position + new Vector3(0.0f, 0.1f, 0.0f);
        Ray ray = new Ray(rayPos, Vector3.down);
        Debug.DrawRay(rayPos, Vector3.down * groundDistance, Color.red);
        return Physics.Raycast(ray, groundDistance, groundLayer);
    }

    Vector3 GetCameraRelativeMovement(Vector2 input)
    {
        Vector3 forward = camera.transform.forward;
        Vector3 right = camera.transform.right;
        forward.y = 0; right.y = 0;
        return (forward * input.y + right * input.x).normalized;
    }

    public void OnMove(Vector2 direction, bool walk)
    {
        if (isReviving) return;
        ChangeState(PlayerState.Running);
        moveInput = direction;
        animator.SetBool("Walk", walk);
    }

    public void OnSprint(bool isSprinting)
    {
        if (isReviving) return;
        ChangeState(PlayerState.Running);
        moveSpeed = isSprinting ? sprintSpeed : 5f;
        animator.SetBool("Run", isSprinting);
    }

    public async Task OnJump()
    {
        if (isReviving || isJumping || isAttacking || !CheckIfGrounded()) return;

        isJumping = true;
        ChangeState(PlayerState.Jumping);

        animator.ResetTrigger("Jump");
        animator.ResetTrigger("Land");

        // すでにジャンプアニメーションが再生中でない場合のみ再生
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
        {
            animator.SetTrigger("Jump");
        }

        await JumpAsync();

        if (!isJumping) return;
        isJumping = false;

        // 着地アニメーションを再生
        animator.SetTrigger("Land");

        ChangeState(PlayerState.Idle);
    }

    async Task JumpAsync()
    {
        float startTime = Time.time;
        float startY = transform.position.y;

        while (Time.time - startTime < jumpDuration)
        {
            if (!isJumping) return; // 他のイベントでキャンセル可能

            float progress = (Time.time - startTime) / jumpDuration;
            float heightOffset = jumpCurve.Evaluate(progress) * jumpHeight;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, heightOffset, rb.linearVelocity.z);

            await Task.Yield();
        }
    }

    /// <summary>
    /// Rayを使用して着地判定を行う
    /// </summary>
    void CheckLanding()
    {
        Vector3 rayPos = transform.position + new Vector3(0.0f, 1.0f, 0.0f); // 少し高めからRayを飛ばす
        Ray ray = new Ray(rayPos, Vector3.down);
        RaycastHit hit;

        Debug.DrawRay(rayPos, Vector3.down * 2.0f, Color.green);

        if (Physics.Raycast(ray, out hit, 2.0f, groundLayer))
        {
            float distanceToGround = hit.distance; // 地面までの距離

            if (distanceToGround < landingThreshold)  // 閾値以下になったら着地
            {
                isLanding = true;
                Debug.Log("着地");
                animator.SetTrigger("Land");
                Invoke(nameof(EndLanding), landingCheckInterval);
            }
        }
    }

    /// <summary>
    /// 着地アニメーション終了後の処理
    /// </summary>
    void EndLanding()
    {
        isLanding = false;
        ChangeState(PlayerState.Idle);
    }

    public void OnInteract()
    {
        if (isReviving) return;
        Debug.Log("Interact action triggered");
    }

    public void OnAttack(bool isStrong)
    {
        if (currentState == PlayerState.Attacking || currentState == PlayerState.Damaged) return;

        isAttacking = true;

        // アニメーションのトリガーをリセットしてから再設定
        animator.ResetTrigger("WeakAttack");
        animator.ResetTrigger("StrongAttack");

        if (isStrong)
        {
            animator.SetTrigger("StrongAttack");
        }
        else
        {
            animator.SetTrigger("WeakAttack");
        }

        ChangeState(PlayerState.Attacking);
    }

    public void OnDamage()
    {
        if (isJumping) isJumping = false;
        if (isAttacking) isAttacking = false;

        PlayerManager.Instance.inputDisable();
        ChangeState(PlayerState.Damaged);
        animator.SetTrigger("Damage");
    }

    public void OnDeath()
    {
        ChangeState(PlayerState.Dead);
        animator.Play("Mutant Dying");
    }

    public void OnRevived()
    {
        PlayerManager.Instance.inputDisable();
        ChangeState(PlayerState.Reviving);
        animator.Play("Mutant Revived");
        Invoke(nameof(EndRevive), reviveTime);
    }

    void EndRevive()
    {
        isReviving = false;
        PlayerManager.Instance.inputEnable();
        ChangeState(PlayerState.Idle);
    }

    /// <summary>
    /// プレイヤーをアクティブ/非アクティブにする際の状態リセット
    /// </summary>
    public void ResetState()
    {
        isJumping = false;
        isAttacking = false;
        isReviving = false;
        moveInput = Vector2.zero;

        ChangeState(PlayerState.Idle);
        rb.linearVelocity = Vector3.zero;
        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);
        animator.ResetTrigger("Jump");
        animator.ResetTrigger("Land");
    }

    /// <summary>
    /// プレイヤーの状態を変更する
    /// </summary>
    /// <param name="newState"></param>
    void ChangeState(PlayerState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
    }

    // アニメーションイベント用メソッド
    void WeakAttackEvent() => PlayerManager.Instance.SetAttackState(true, false);
    void StrongAttackEvent() => PlayerManager.Instance.SetAttackState(false, true);
    void EndAttackEvent()
    {
        isAttacking = false;
        PlayerManager.Instance.SetAttackState(false, false);

        // 移動入力がある場合、すぐにWalkまたはRunに移行(入力がない場合はIdleへ)
        if (moveInput != Vector2.zero)
        {
            if (moveSpeed == sprintSpeed)
            {
                ChangeState(PlayerState.Running);
                animator.SetBool("Run", true);
                animator.SetBool("Walk", false);
            }
            else
            {
                ChangeState(PlayerState.Running);
                animator.SetBool("Walk", true);
                animator.SetBool("Run", false);
            }
        }
        else
        {
            ChangeState(PlayerState.Idle);
            animator.SetBool("Walk", false);
            animator.SetBool("Run", false);
        }
    }

    void EndDamageEvent()
    {
        PlayerManager.Instance.inputEnable();
        ChangeState(PlayerState.Idle);
    }

    /// <summary>
    /// シーン遷移時にカメラを切り替えるメソッド
    /// </summary>
    /// <param name="newCamera"></param>
    public void SetCamera(Camera newCamera)
    {
        camera = newCamera;
    }
}
