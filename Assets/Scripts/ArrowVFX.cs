using Unity.Cinemachine;
using UnityEngine;

public class ArrowVFX : MonoBehaviour
{
    Vector3 shootDirection;
    float speed;
    float lifetime;
    bool hasDealtDamage = false; // ダメージ二重処理防止

    public void Initialize(Vector3 shootDirection, float speed, float lifetime)
    {
        this.shootDirection = shootDirection.normalized;
        this.speed = speed;
        this.lifetime = lifetime;
        Destroy(gameObject, lifetime); // 10秒後に自動消滅
    }

    void Update()
    {
        // プレイヤーに向かって直線的に移動
        transform.position += shootDirection * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if(!hasDealtDamage && other.CompareTag("Player"))
        {
            hasDealtDamage = true;
            PlayerManager.Instance.TakeDamage(10);
            Destroy(gameObject);
        }
    }
}
