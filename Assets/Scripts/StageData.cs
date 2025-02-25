using System;
using UnityEngine;

[Serializable]
public class WaveData
{
    [Header("����Wave�ŃX�|�[������G�̑���")]
    public int enemyCount;
    [Header("�t�B�[���h�ɓ����ɏo���ł���ő吔")]
    public int maxEnemiesOnField;
    [Header("�ŏ��̓G���X�|�[������܂ł̎���")]
    public float firstSpawnDelay;
    [Header("�G���X�|�[������Ԋu")]
    public float spawnInterval;
}


[CreateAssetMenu(fileName = "StageData", menuName = "Game/StageData")]
public class StageData : ScriptableObject
{
    [NamedArray("Wave1", "Wave2", "LastWave", "����ȏ�ݒ�͂ł��܂���")]
    public WaveData[] waves; // �eWave�̃f�[�^
    [Header("�v���C���[�̃X�|�[���ʒu")]
    public Vector3 playerSpawnPoint; // �v���C���[�̃X�|�[���ʒu
}
