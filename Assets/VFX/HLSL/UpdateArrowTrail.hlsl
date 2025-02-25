#include "VFXProxCommon.hlsl"

float TimeFactor;
float MoveStrength;
float FlickerSpeed;

void UpdateSphereParticles(in float age, in float lifetime, inout VFXAttributes attributes)
{
    // �@�����v�Z�i���̒��S����Ɂj
    float3 normal = normalize(attributes.position);
        
    // �߂��̗��q��T�����āA�e�����󂯂�
    float3 nearest1, nearest2;
    VFXProx_LookUpNearestPair(attributes.position, nearest1, nearest2);

    // �ŋߐړ_�̊Ԃ̃x�N�g������Ƀ����_���Ȉړ���������
    float3 moveDir = normalize(cross(nearest2 - nearest1, normal));
    attributes.velocity += moveDir * MoveStrength;
    // �ʒu���X�V�i���̕\�ʂɗ��܂�悤�ɕ␳�j
    attributes.position += attributes.velocity * 0.016;
    attributes.position = normalize(attributes.position) * length(attributes.position);

    // ������G�t�F�N�g
    attributes.alpha = 0.5 + 0.5 * sin(age * FlickerSpeed);

    // ���x�ɉ����Ĕ���
    float speedFactor = length(attributes.velocity);
    attributes.color = lerp(float3(1, 0.5, 0), float3(1, 1, 0), speedFactor);
}