using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] 
    int maxHP = 100; // 敵の最大HP
    int currentHP; // 現在のHP
    public int attackDamage = 20; // プレイヤーに与えるダメージ量

    [Header("Arrow Settings")]
    [SerializeField] 
    GameObject arrowPrefab; // 矢のエフェクトプレハブ
    [SerializeField] 
    Transform arrowSpawnPoint; // 矢の生成位置
    [SerializeField] 
    float arrowSpeed = 10.0f; // 矢の速度

    Queue<GameObject> arrowPool = new Queue<GameObject>();
    int poolSize = 10;

    [Header("Death Settings")]
    [SerializeField] 
    float despawnDelay = 5.0f; // 死亡後にデスポーンするまでの時間

    [Header("References")]
    Animator animator; // 敵のアニメーター
    bool isDead = false; // 敵が死亡したかどうか

    void Start()
    {
        // コンポーネント取得
        animator = GetComponent<Animator>();

        // HPを初期化
        currentHP = maxHP;

        // 事前に矢をプールに作成
        for (int i = 0; i < poolSize; i++)
        {
            GameObject arrow = Instantiate(arrowPrefab);
            arrow.SetActive(false);
            arrowPool.Enqueue(arrow);
        }
    }

    // ダメージを受けたときの処理
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        //Debug.Log($"敵は{damage}のダメージを受けた。 現在のHP: {currentHP}");

        // ダメージアニメーションを再生
        animator.Play("Bow Damage");

        // HPが0以下になった場合は死亡処理を実行
        if (currentHP <= 0)
        {
            Death();
        }
    }

    // 死亡処理
    void Death()
    {
        if (isDead) return;

        isDead = true;

        // 死亡アニメーションを再生
        animator.Play("Bow Death");

        // 一定時間後にデスポーン
        StartCoroutine(DespawnAfterDelay());
    }

    IEnumerator DespawnAfterDelay()
    {
        yield return new WaitForSeconds(despawnDelay);

        StageManager.Instance.OnEnemyDefeated(gameObject);
    }

    public void SetArrow()
    {
        if (arrowPool.Count == 0) return;

        GameObject arrow = arrowPool.Dequeue();
        arrow.transform.position = arrowSpawnPoint.position;
        arrow.transform.rotation = arrowSpawnPoint.rotation;
        arrow.SetActive(true);
    }

    public void DestroyArrow(GameObject arrow)
    {
        arrow.SetActive(false);
        arrowPool.Enqueue(arrow);
    }

    // Shot時に矢を生成して飛ばす処理
    public void ShootArrow(Transform target)
    {
        if (arrowPool.Count == 0) return;

        GameObject arrow = arrowPool.Dequeue();
        arrow.transform.position = arrowSpawnPoint.position;
        arrow.transform.rotation = arrowSpawnPoint.rotation;
        arrow.SetActive(true);

        Vector3 direction = (target.position - arrowSpawnPoint.position).normalized;
        Rigidbody rb = arrow.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = direction * arrowSpeed;
        }

        // 矢を一定時間後に戻す
        StartCoroutine(ReturnArrowToPool(arrow));
    }

    private IEnumerator ReturnArrowToPool(GameObject arrow)
    {
        yield return new WaitForSeconds(5.0f);
        arrow.SetActive(false);
        arrowPool.Enqueue(arrow);
    }


    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        // HandまたはJumpAttackタグを確認
        if (other.CompareTag("Hand") && PlayerAction.isWeakAttackingJudgement)
        {
            TakeDamage(PlayerManager.Instance.weakAttackDamage); // 弱攻撃のダメージ
            //Debug.Log("敵は弱攻撃を喰らった");
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("JumpAttack") && PlayerAction.isStrongAttackingJudgement)
        {
            TakeDamage(PlayerManager.Instance.strongAttackDamage); // 強攻撃のダメージ
            //Debug.Log("敵は強攻撃を喰らった");
        }
    }
}
