using UnityEngine;
using UnityEngine.VFX;

public class PlayerEffect : MonoBehaviour
{
    [SerializeField] 
    VisualEffect jump;
    [SerializeField] 
    VisualEffect sprint;
    [SerializeField] 
    VisualEffect weakAttack;
    [SerializeField] 
    VisualEffect strongAttack;
    [SerializeField] 
    VisualEffect damage;
    [SerializeField] 
    VisualEffect death;

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

    public void PlayWeakAttackEffect()
    {
        if (weakAttack != null)
        {
            weakAttack.Play();
        }
    }

    public void PlayStrongAttackEffect()
    {
        if (strongAttack != null)
        {
            //Instantiate(strongAttack, transform.position, Quaternion.identity);
            strongAttack.Play();
        }
    }

    public void PlayDamageEffect()
    {
        if(damage != null)
        {
            damage.Play();
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
