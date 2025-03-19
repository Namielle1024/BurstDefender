using UnityEngine;
using UnityEngine.VFX;

public class PlayerEffect : MonoBehaviour
{
    [SerializeField] 
    VisualEffect jump;
    [SerializeField] 
    VisualEffect sprint;
    [SerializeField]
    VisualEffect attackTrail;
    [SerializeField] 
    GameObject weakAttack;
    [SerializeField]
    Transform weakAttacksSpawnPoint;
    [SerializeField] 
    GameObject strongAttack;
    [SerializeField]
    Transform strongAttackSpawnPoint;
    [SerializeField] 
    GameObject damage;
    [SerializeField]
    Transform damageSpawnPoint;
    [SerializeField] 
    VisualEffect death;

    GameObject strongAttackInstance;
    VisualEffect vfx;
    GroundAligner[] groundAligners;
    Collider attackCollider;
    bool isFollowing = true; // 追従中フラグ
    float attackEffectDestroyTime = 3.0f;
    float damageEffectDestroyTime = 2.0f;

    void Start()
    {
        GameManager.Instance.StopVFXEffects();
    }

    void Update()
    {
        // 追従中ならプレイヤーの前方に追従させる
        if (isFollowing && strongAttackInstance != null)
        {
            strongAttackInstance.transform.position = strongAttackSpawnPoint.position;
            strongAttackInstance.transform.rotation = strongAttackSpawnPoint.rotation;
        }
    }

    public void PlayJumpEffect()
    {
        if (jump != null)
        {
            Instantiate(jump, transform.position, Quaternion.identity);
        }
    }

    public void PlaySprintEffect(bool isSprinting)
    {
        if (sprint != null)
        {
            if (isSprinting)
            {
                Instantiate(sprint, transform.position, Quaternion.identity);
            }
        }
    }

    /// <summary>
    /// プレイヤーの弱攻撃エフェクト再生
    /// </summary>
    void PlayWeakAttackEffect()
    {
        if (weakAttack != null)
        {
            GameObject weak = Instantiate(weakAttack, weakAttacksSpawnPoint.position, weakAttacksSpawnPoint.rotation);
            Destroy(weak, attackEffectDestroyTime);
        }
    }

    /// <summary>
    /// アニメーション開始時に呼ばれる(親オブジェクト生成＆VFX停止)
    /// </summary>
    void PlayStrongAttackEffect_Start()
    {
        if (strongAttack != null)
        {
            // 親オブジェクトをインスタンス化
            strongAttackInstance = Instantiate(strongAttack, strongAttackSpawnPoint.position, strongAttackSpawnPoint.rotation);

            isFollowing = true; // 追従開始

            // 子オブジェクトの VFX を取得
            vfx = strongAttackInstance.GetComponentInChildren<VisualEffect>();

            if (vfx != null)
            {
                vfx.Stop(); // エフェクトを停止
            }

            // 子オブジェクトの GroundAligner を取得
            groundAligners = strongAttackInstance.GetComponentsInChildren<GroundAligner>();
        }
    }

    /// <summary>
    /// アニメーションのインパクト時に呼ばれる(地形調整＆VFX再生＆Collider有効化)
    /// </summary>
    void PlayStrongAttackEffect_Impact()
    {
        if (strongAttackInstance != null && vfx != null)
        {
            isFollowing = false; // 追従を停止

            if (groundAligners != null)
            {
                for (int i = 0; i < groundAligners.Length; i++)
                {
                    // 地面に瞬時にフィットさせる
                    groundAligners[i].AdjustToGround();

                    // 攻撃判定を有効化
                    groundAligners[i].EnableCollider();
                }
            }

            vfx.Play(); // エフェクト再生

            Destroy(strongAttackInstance, attackEffectDestroyTime);
        }
    }

    /// <summary>
    /// プレイヤーアタック時のトレイルエフェクト再生
    /// </summary>
    void PlayTrailEffect()
    {
        if(attackTrail != null)
        {
            attackTrail.Play();
        }
    }

    /// <summary>
    /// プレイヤーアタック時のトレイルエフェクト停止
    /// </summary>
    public void StopTrailEffect()
    {
        if (attackTrail != null)
        {
            attackTrail.Stop();
        }
    }

    /// <summary>
    /// プレイヤーのダメージエフェクト再生
    /// </summary>
    public void PlayDamageEffect()
    {
        if(damage != null)
        {
            GameObject damageEffect = Instantiate(damage, damageSpawnPoint.position, damageSpawnPoint.rotation, damageSpawnPoint);
            Destroy(damageEffect, damageEffectDestroyTime);
        }
    }

    /// <summary>
    /// プレイヤーの死亡エフェクト再生
    /// </summary>
    public void PlayDeathEffect()
    {
        if(death != null)
        {
            death.Play();
        }
    }
}
