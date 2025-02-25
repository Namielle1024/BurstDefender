using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Game/StageData")]
public class StageData : ScriptableObject
{
    public int enemyCount;              // 出現する敵の総数
    public int maxEnemiesOnField;       // フィールド上の最大敵数
    public float spawnInterval;         // 敵のスポーン間隔
    public Vector3 playerSpawnPoint;    // プレイヤーの初期位置
}
