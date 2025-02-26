using UnityEngine;

public class WallFadeController : MonoBehaviour
{
    [SerializeField] // 完全に透明になる距離
    float fadeDistance = 50f; 
    [SerializeField] // 完全に見える距離
    float fullVisibleDistance = 10f; 
    [SerializeField] 
    Renderer wallRenderer;

    Material wallMaterial;
    Transform player;
    Collider wallCollider;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        wallMaterial = wallRenderer.material;
        wallCollider = GetComponent<Collider>(); // 壁のコライダーを取得
    }

    void Update()
    {
        if (player == null || wallCollider == null) return;

        // プレイヤーの位置から壁の最も近い位置を取得
        Vector3 closestPoint = wallCollider.ClosestPoint(player.position);
        float distance = Vector3.Distance(player.position, closestPoint); // 最近接点との距離を計算

        float alpha = Mathf.InverseLerp(fadeDistance, fullVisibleDistance, distance); // 透明度を計算

        // 透明度を設定
        wallMaterial.SetFloat("_Alpha", alpha);
    }
}
