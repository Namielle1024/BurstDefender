using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] // 敵の最大HP
    int maxHP = 100;
    int currentHP; // 現在のHP
    int attackDamage = 10; // プレイヤーに与えるダメージ量

    [Header("HP UI")]
    [SerializeField] // World Space Canvas
    Canvas hpCanvas;
    [SerializeField] // HPバーのImage
    Image hpBar;
    [SerializeField] // HPバーが表示される時間
    float hpBarVisibleDuration = 1.5f;

    [Header("Arrow Settings")]
    [SerializeField] // 矢のプレハブ
    GameObject arrowPrefab;
    [SerializeField] // 矢をセットした時のVFX
    VisualEffect arrowSetVFX;
    [SerializeField] // 矢を発射するときのVFXプレハブ
    GameObject shootVFXPrefab;
    [SerializeField] // 矢の生成位置
    Transform arrowSpawnPoint;
    [SerializeField] // 矢の速度
    float arrowSpeed = 10.0f;
    [SerializeField] // 矢の寿命
    float arrowLifetime = 5.0f;

    [Header("Damage Effect")]
    [SerializeField]
    GameObject damageVFX;
    [SerializeField]
    Transform damageSpawnPoint;
    float damageEffectDestroyTime = 2.0f;

    [Header("Death Settings")]
    [SerializeField] // 死亡後にデスポーンするまでの時間
    float despawnDelay = 5.0f;

    [Header("References")]
    Animator animator; // 敵のアニメーター
    bool isDead = false; // 敵が死亡したかどうか
    bool isArrowSet = false;

    Camera mainCamera;
    Task hideHPBarTask;
    bool isHiding = false;

    void Start()
    {
        // コンポーネント取得
        animator = GetComponent<Animator>();

        // HPを初期化
        currentHP = maxHP;

        // 初期状態では矢を非アクティブ化
        ClearArrow();

        mainCamera = Camera.main;

        // HPの初期設定
        UpdateHPBar();

        // 初期状態では非表示
        if (hpCanvas != null)
            hpCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (hpCanvas != null && mainCamera != null)
        {
            // HPバーをカメラの方向に向ける（常に正面）
            hpCanvas.transform.rotation = Quaternion.LookRotation(hpCanvas.transform.position - mainCamera.transform.position);
        }
    }

    /// <summary>
    /// ダメージを受けたときの処理
    /// </summary>
    /// <param name="damage">プレイヤーから受け取る</param>
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        UpdateHPBar();

        // ダメージアニメーションを再生
        animator.Play("Bow Damage");

        if (damageVFX != null && !IsDamageEffectActive())
        {
            GameObject damageEffect = Instantiate(damageVFX, damageSpawnPoint.position, damageSpawnPoint.rotation, damageSpawnPoint);
            Destroy(damageEffect, damageEffectDestroyTime);
        }

        // HPが0以下になった場合は死亡処理を実行
        if (currentHP <= 0)
        {
            Death();
        }
    }

    /// <summary>
    /// ダメージエフェクトが既に存在しているか確認
    /// </summary>
    bool IsDamageEffectActive()
    {
        // damageSpawnPoint の子オブジェクトにエフェクトがあるか確認
        foreach (Transform child in damageSpawnPoint)
        {
            if (child.CompareTag("DamageEffect")) // DamageEffect タグを付ける
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// HPバーの更新
    /// </summary>
    void UpdateHPBar()
    {
        if (hpBar != null)
        {
            float fillAmount = (float)currentHP / maxHP;
            hpBar.fillAmount = fillAmount;

            // ダメージを受けたらHPバーを表示
            ShowHPBar();

            // HPゼロならHPバーを非表示
            hpCanvas.gameObject.SetActive(currentHP > 0);
        }
    }

    /// <summary>
    /// HPバーを表示し、一定時間後に非表示
    /// </summary>
    async void ShowHPBar()
    {
        if (hpCanvas == null) return;

        hpCanvas.gameObject.SetActive(true);

        // 非表示タスクが実行中ならリセット
        isHiding = false;

        // 新たに非表示処理を開始
        hideHPBarTask = HideHPBarAfterDelay();
        await hideHPBarTask;
    }

    /// <summary>
    /// HPバーを一定時間後に非表示にする
    /// </summary>
    async Task HideHPBarAfterDelay()
    {
        isHiding = true;

        // 一定時間待機
        float elapsed = 0f;
        while (elapsed < hpBarVisibleDuration)
        {
            if (!isHiding) return; // 新しいダメージがあれば中断
            elapsed += Time.deltaTime;
            await Task.Yield();
        }

        if (currentHP > 0 && hpCanvas != null)
        {
            hpCanvas.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 死亡処理
    /// </summary>
    void Death()
    {
        if (isDead) return;

        isDead = true;

        // レイヤーを「DeadEnemy」に変更
        gameObject.layer = LayerMask.NameToLayer("DeadEnemy");

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
        if (isDead || isArrowSet) return;

        isArrowSet = true;
        if (arrowPrefab != null)
        {
            arrowPrefab.SetActive(true);
        }
        if (arrowSetVFX != null)
        {
            arrowSetVFX.Play();
        }
    }

    // 矢を非アクティブにする（Draw 以外の状態になったとき）
    public void ClearArrow()
    {
        isArrowSet = false;
        if (arrowPrefab != null)
        {
            arrowPrefab.SetActive(false);
        }
        if (arrowSetVFX != null)
        {
            arrowSetVFX.Stop();
        }
    }

    // 矢を発射する
    public void ShootArrow()
    {
        if (isDead || shootVFXPrefab == null || arrowSpawnPoint == null) return;

        // 発射エフェクトを生成
        GameObject shootVFX = Instantiate(shootVFXPrefab, arrowSpawnPoint.position, transform.rotation);

        ArrowVFX arrowVFX = shootVFX.GetComponent<ArrowVFX>();
        if (arrowVFX != null)
        {
            Vector3 shootDirection = transform.forward;
            arrowVFX.Initialize(shootDirection, arrowSpeed, arrowLifetime);
        }

        // 矢をクリア
        ClearArrow();
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

    public int GetDamage()
    {
        return attackDamage;
    }

    public bool IsDead()
    {
        return isDead;
    }
}
