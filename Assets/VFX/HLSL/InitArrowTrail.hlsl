#include "VFXProxCommon.hlsl"

// VFX Graph ����󂯎��p�����[�^
StructuredBuffer<float3> VertexBuffer;
uint VertexCount;
float RadiusOffset;

void InitSphereParticles(inout VFXAttributes attributes, uint index)
{
    if (VertexCount == 0)
        return;
    if (index >= VertexCount)
        return;

    // ���_�o�b�t�@������W��擾
    float3 vertexPos = VertexBuffer[index];

    // �@������߂�
    float3 normal = normalize(vertexPos);

    // �@�������ɃI�t�Z�b�g������Ĕz�u
    attributes.position = vertexPos + normal * RadiusOffset;

    // �߂��̗��q��T��
    float3 nearest1, nearest2;
    VFXProx_LookUpNearestPair(attributes.position, nearest1, nearest2);

    // 2�̍ŋߐڗ��q�̊Ԃ̃x�N�g����������x�ɂ���
    attributes.velocity = normalize(nearest2 - nearest1) * 0.5;
}