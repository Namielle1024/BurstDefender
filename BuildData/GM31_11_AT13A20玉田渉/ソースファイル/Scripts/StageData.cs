using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Game/StageData")]
public class StageData : ScriptableObject
{
    public int enemyCount;              // �o������G�̑���
    public int maxEnemiesOnField;       // �t�B�[���h��̍ő�G��
    public float spawnInterval;         // �G�̃X�|�[���Ԋu
    public Vector3 playerSpawnPoint;    // �v���C���[�̏����ʒu
}
