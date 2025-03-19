using UnityEngine;

public class GroundAligner : MonoBehaviour
{
    [SerializeField] private float yOffset = 0.1f; // Y座標の補正
    private Collider attackCollider; // Posごとの攻撃判定用コライダー
    private bool isColliderEnabled = false;

    void Start()
    {
        attackCollider = GetComponent<Collider>();

        if (attackCollider != null)
        {
            attackCollider.enabled = false; // 初期状態では無効
        }
    }

    /// <summary>
    /// 地面にTransformを合わせるメソッド
    /// </summary>
    public void AdjustToGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 1f, Vector3.down, out hit, 10f, LayerMask.GetMask("Ground")))
        {
            transform.position = hit.point + new Vector3(0, yOffset, 0); // 瞬時に地面に配置
            transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal); // 地面の法線に合わせて回転
        }
    }

    /// <summary>
    /// インパクト時にColliderを有効化
    /// </summary>
    public void EnableCollider()
    {
        if (attackCollider != null && !isColliderEnabled)
        {
            attackCollider.enabled = true; // コライダーを有効化
            isColliderEnabled = true;
        }
    }
}
