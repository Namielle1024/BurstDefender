using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    EnemyManager enemyManager;

    // アニメーターとNavMeshエージェント
    Animator animator;

    // 状態管理
    enum State { Idle, Run, Draw }
    State currentState = State.Idle;

    // 距離のしきい値
    [SerializeField] 
    float detectionRange = 15.0f;  // プレイヤーを見つける範囲
    [SerializeField] 
    float attackRange = 5.0f;      // Draw状態に移行する範囲

    // Run状態：移動速度や回転速度
    [SerializeField] 
    float runMoveSpeed = 2.0f;
    [SerializeField] 
    float rotationSpeed = 5.0f;

    // Draw状態：左右移動速度
    [SerializeField] 
    float walkMoveSpeed = 0.01f;

    // 時間制御
    float stateTimer;

    // Draw状態の行動確率
    float shotProbability = 0.2f; // 初期確率（近づくほど上がる）

    // 行動管理フラグ
    bool isShooting;   // Shotアニメーション中にtrueになる
    bool isWalking;    // Walkアニメーション中にtrueになる
    bool isIdleLocked; // Shotアニメーション後にtrueになリ、一定時間後にfalseになる

    void Start()
    {
        // コンポーネント取得
        animator = GetComponent<Animator>();
        enemyManager = GetComponent<EnemyManager>();
    }

    void Update()
    {
        // プレイヤーとの距離を測定
        float distanceToPlayer = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);

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

    // Idleの動作
    void IdleBehavior(float distanceToPlayer)
    {
        animator.SetBool("Idle", true);
        animator.SetBool("Run", false);
        animator.SetBool("DrawIdle", false);

        if (distanceToPlayer <= detectionRange && !isIdleLocked)
        {
            // プレイヤーを検知してRun状態へ遷移
            TransitionToState(State.Run);
            //Debug.Log("現在の状態" + currentState);
        }
    }

    // Runの動作
    void RunBehavior(float distanceToPlayer)
    {
        animator.SetBool("Idle", false);
        animator.SetBool("Run", true);
        animator.SetBool("DrawIdle", false);

        // プレイヤーの方向を向く
        LookAtPlayer();

        // 敵を前方に移動
        transform.position += transform.forward * runMoveSpeed * Time.deltaTime;

        if (distanceToPlayer <= attackRange)
        {
            // 一定距離まで接近したらDraw状態へ遷移
            animator.SetTrigger("Draw");
            enemyManager.SetArrow();
            TransitionToState(State.Draw);
            //Debug.Log("現在の状態" + currentState);
        }

        if (distanceToPlayer >= detectionRange)
        {
            
            // 距離が離れたらIdle状態に戻る
            TransitionToState(State.Idle);
            //Debug.Log("現在の状態" + currentState);
        }
    }

    // Drawの動作
    private void DrawBehavior(float distanceToPlayer)
    {
        animator.SetBool("Idle", false);
        animator.SetBool("Run", false);
        animator.SetBool("DrawIdle", true);

        // 状態タイマーの更新
        stateTimer += Time.deltaTime;

        // プレイヤーの方向を向く（Shot以外）
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Shot"))
        {
            LookAtPlayer();
        }

        // 1秒ごとに行動を選択する
        if (stateTimer >= 1.0f && !isShooting && !isWalking)
        {
            stateTimer = 0f;

            // プレイヤーが近づくほどShot確率を上げる
            shotProbability = Mathf.Clamp01((attackRange - distanceToPlayer) / attackRange);

            // ランダムな行動を選択
            float action = Random.value;
            if (action <= shotProbability)
            {
                // 弓を射る
                isShooting = true;
                animator.SetTrigger("Shot");
                enemyManager.ShootArrow(PlayerManager.Instance.transform);
                StartCoroutine(HandleShot());
            }
            else if (action > 0.5f)
            {
                // 左へ歩く
                isWalking = true;
                animator.SetTrigger("WalkLeft");
                StartCoroutine(WalkLeftOrRight(-1)); // 左移動
            }
            else
            {
                // 右へ歩く
                isWalking = true;
                animator.SetTrigger("WalkRight");
                StartCoroutine(WalkLeftOrRight(1)); // 右移動
            }
        }

        // プレイヤーから離れた場合はRun状態に戻る
        if (distanceToPlayer > attackRange)
        {
            TransitionToState(State.Run);
            //Debug.Log("現在の状態" + currentState);
        }
    }

    // プレイヤーを注視する
    void LookAtPlayer()
    {
        Vector3 directionToPlayer = (PlayerManager.Instance.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z)); // 水平回転のみ
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    // 弓を射た後の処理
    IEnumerator HandleShot()
    {
        // Shotアニメーションが終了するまで待機
        yield return new WaitForSeconds(0.25f); // アニメーションの時間に合わせる
        isShooting = false; // フラグを解除

        // Idle状態に移行し、〇秒間固定
        TransitionToState(State.Idle);
        //Debug.Log("現在の状態" + currentState);
        isIdleLocked = true; // Idle状態を固定
        yield return new WaitForSeconds(2.0f); // 〇秒間Idleを維持
        isIdleLocked = false; // Idle固定解除
    }

    // 左右に1秒かけて徐々に移動
    IEnumerator WalkLeftOrRight(int direction)
    {
        float duration = 1.0f; // 移動時間
        float elapsed = 0f; // 経過時間

        // 左（-1）または右（1）方向に移動
        Vector3 moveDirection = transform.right * direction;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            transform.position += moveDirection * walkMoveSpeed * Time.deltaTime;

            yield return null; // 次のフレームまで待機
        }

        yield return new WaitForSeconds(0.25f);
        isWalking = false;
    }

    // 状態遷移
    void TransitionToState(State newState)
    {
        currentState = newState;
        stateTimer = 0f; // 状態タイマーをリセット
    }
}
