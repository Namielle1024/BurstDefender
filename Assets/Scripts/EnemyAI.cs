using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // 敵のマネージャーコンポーネント
    EnemyManager enemyManager;

    // アニメーターコンポーネント
    Animator animator;

    // 敵の行動状態
    enum State { Idle, Run, Draw }
    State currentState = State.Idle;

    [Header("Detection Settings")]
    [SerializeField] float detectionRange = 15.0f; // プレイヤーを見つける範囲
    [SerializeField] float attackRange = 5.0f;     // Draw状態に移行する範囲

    [Header("Movement Settings")]
    [SerializeField] float runMoveSpeed = 2.0f;    // 走る速度
    [SerializeField] float rotationSpeed = 5.0f;  // 回転速度
    [SerializeField] float walkMoveSpeed = 0.01f; // Draw状態時の左右移動速度

    // 状態の経過時間
    float stateTimer;

    // Draw状態の際の射撃確率
    float shotProbability = 0.2f; // 初期確率（近づくほど上がる）

    [Header("Animation Control")]
    bool isShooting;   // Shotアニメーション中
    bool isWalking;    // Walkアニメーション中
    bool isAnimationLocked; // Shot後の硬直
    float animationLockTime = 3.0f; // Shot後の硬直時間

    void Start()
    {
        animator = GetComponent<Animator>();
        enemyManager = GetComponent<EnemyManager>();

        // 初期状態では矢を非アクティブ化
        enemyManager.ClearArrow();
    }

    void Update()
    {
        // プレイヤーとの距離を計算
        float distanceToPlayer = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);

        // 現在の状態ごとに処理を分岐
        switch (currentState)
        {
            case State.Idle:
                IdleBehavior(distanceToPlayer);
                break;
            case State.Run:
                RunBehavior(distanceToPlayer);
                break;
            case State.Draw:
                DrawBehavior(distanceToPlayer);
                break;
        }
    }

    /// <summary>
    /// 待機（Idle）状態の挙動
    /// </summary>
    /// <param name="distanceToPlayer">プレイヤーとの距離</param>
    void IdleBehavior(float distanceToPlayer)
    {
        // すでに死亡していたら処理をしない
        if (enemyManager.IsDead()) return;

        // アニメーション設定
        animator.SetBool("Idle", true);
        animator.SetBool("Run", false);
        animator.SetBool("DrawIdle", false);

        // 矢を持っている場合はクリアする
        enemyManager.ClearArrow();

        // プレイヤーを検知したら走る状態へ遷移
        if (distanceToPlayer <= detectionRange && !isAnimationLocked)
        {
            TransitionToState(State.Run);
        }
    }

    /// <summary>
    /// 走る（Run）状態の挙動
    /// </summary>
    /// <param name="distanceToPlayer">プレイヤーとの距離</param>
    void RunBehavior(float distanceToPlayer)
    {
        if (enemyManager.IsDead()) return;

        // アニメーション設定
        animator.SetBool("Idle", false);
        animator.SetBool("Run", true);
        animator.SetBool("DrawIdle", false);

        // 矢を持っている場合はクリアする
        enemyManager.ClearArrow();

        // プレイヤーの方向を向く
        LookAtPlayer();

        // プレイヤーの方向に向かって移動
        transform.position += transform.forward * runMoveSpeed * Time.deltaTime;

        // プレイヤーに近づいたらDraw状態へ
        if (distanceToPlayer <= attackRange)
        {
            animator.SetTrigger("Draw");
            TransitionToState(State.Draw);
        }
        // プレイヤーが離れたらIdle状態へ
        else if (distanceToPlayer >= detectionRange)
        {
            TransitionToState(State.Idle);
        }
    }

    /// <summary>
    /// 攻撃準備（Draw）状態の挙動
    /// </summary>
    /// <param name="distanceToPlayer">プレイヤーとの距離</param>
    void DrawBehavior(float distanceToPlayer)
    {
        if (enemyManager.IsDead()) return;

        // アニメーション設定
        animator.SetBool("Idle", false);
        animator.SetBool("Run", false);
        animator.SetBool("DrawIdle", true);

        // 矢をセットする
        enemyManager.SetArrow();

        // 状態タイマーを更新
        stateTimer += Time.deltaTime;

        // 射撃アニメーション中は向きを変えない
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Shot"))
        {
            LookAtPlayer();
        }

        // 1秒ごとにアクションを決定
        if (stateTimer >= 1.0f && !isShooting && !isWalking)
        {
            stateTimer = 0f;

            // プレイヤーが近いほど射撃確率を上げる
            shotProbability = Mathf.Clamp01((attackRange - distanceToPlayer) / attackRange);
            float action = Random.value;

            if (action <= shotProbability)
            {
                isShooting = true;
                animator.SetTrigger("Shot");
                StartCoroutine(HandleShot());
            }
            else if (action > 0.5f)
            {
                isWalking = true;
                animator.SetTrigger("WalkLeft");
                StartCoroutine(WalkLeftOrRight(-1));
            }
            else
            {
                isWalking = true;
                animator.SetTrigger("WalkRight");
                StartCoroutine(WalkLeftOrRight(1));
            }
        }

        // プレイヤーが離れたらRun状態へ
        if (distanceToPlayer > attackRange)
        {
            TransitionToState(State.Run);
        }
    }

    /// <summary>
    /// プレイヤーの方向を向く
    /// </summary>
    void LookAtPlayer()
    {
        // プレイヤーの方向を計算
        Vector3 directionToPlayer = (PlayerManager.Instance.transform.position - transform.position).normalized;

        // Y軸回転のみ行う
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));

        // スムーズに回転
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    /// <summary>
    /// 矢を射た後の処理
    /// </summary>
    IEnumerator HandleShot()
    {
        yield return new WaitForSeconds(0.25f);
        isShooting = false;
        TransitionToState(State.Idle);
        isAnimationLocked = true;
        yield return new WaitForSeconds(animationLockTime);
        isAnimationLocked = false;
    }

    /// <summary>
    /// 左右移動アクション
    /// </summary>
    /// <param name="direction">-1なら左移動、1なら右移動</param>
    IEnumerator WalkLeftOrRight(int direction)
    {
        float duration = 1.0f;
        float elapsed = 0f;
        Vector3 moveDirection = transform.right * direction;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position += moveDirection * walkMoveSpeed * Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.25f);
        isWalking = false;
    }

    /// <summary>
    /// 敵の状態遷移
    /// </summary>
    void TransitionToState(State newState)
    {
        currentState = newState;
        stateTimer = 0f;
    }

    void ShootArrowAnimation()
    {
        enemyManager.ShootArrow();
    }
}
