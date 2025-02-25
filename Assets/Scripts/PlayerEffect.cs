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

    float attackEffectDestroyTime = 3.0f;
    float damageEffectDestroyTime = 2.0f;

    void Start()
    {
        GameManager.Instance.StopVFXEffects();
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

    void PlayWeakAttackEffect()
    {
        if (weakAttack != null)
        {
            GameObject weak = Instantiate(weakAttack, weakAttacksSpawnPoint.position, weakAttacksSpawnPoint.rotation);
            Destroy(weak, attackEffectDestroyTime);
        }
    }

    void PlayStrongAttackEffect()
    {
        if (strongAttack != null)
        {
            GameObject strong = Instantiate(strongAttack, strongAttackSpawnPoint.position, strongAttackSpawnPoint.rotation);
            Destroy(strong, attackEffectDestroyTime);
        }
    }

    void PlayTrailEffect()
    {
        if(attackTrail != null)
        {
            attackTrail.Play();
        }
    }

    public void StopTrailEffect()
    {
        if (attackTrail != null)
        {
            attackTrail.Stop();
        }
    }

    public void PlayDamageEffect()
    {
        if(damage != null)
        {
            GameObject damageEffect = Instantiate(damage, damageSpawnPoint.position, damageSpawnPoint.rotation, damageSpawnPoint);
            Destroy(damageEffect, damageEffectDestroyTime);
        }
    }

    public void PlayDeathEffect()
    {
        if(death != null)
        {
            death.Play();
        }
    }
}
