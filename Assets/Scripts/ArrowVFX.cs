using Unity.Cinemachine;
using UnityEngine;

public class ArrowVFX : MonoBehaviour
{
    Vector3 shootDirection;
    float speed;
    float lifetime;
    bool hasDealtDamage = false; // �_���[�W��d�����h�~

    public void Initialize(Vector3 shootDirection, float speed, float lifetime)
    {
        this.shootDirection = shootDirection.normalized;
        this.speed = speed;
        this.lifetime = lifetime;
        Destroy(gameObject, lifetime); // 10�b��Ɏ�������
    }

    void Update()
    {
        // �v���C���[�Ɍ������Ē����I�Ɉړ�
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
