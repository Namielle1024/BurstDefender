using System;
using UnityEngine;

[Serializable]
public class WaveData
{
    [Header("このWaveでスポーンする敵の総数")]
    public int enemyCount;
    [Header("フィールドに同時に出現できる最大数")]
    public int maxEnemiesOnField;
    [Header("最初の敵がスポーンするまでの時間")]
    public float firstSpawnDelay;
    [Header("敵がスポーンする間隔")]
    public float spawnInterval;
}


[CreateAssetMenu(fileName = "StageData", menuName = "Game/StageData")]
public class StageData : ScriptableObject
{
    [NamedArray("Wave1", "Wave2", "LastWave", "これ以上設定はできません")]
    public WaveData[] waves; // 各Waveのデータ
    [Header("プレイヤーのスポーン位置")]
    public Vector3 playerSpawnPoint; // プレイヤーのスポーン位置
}
