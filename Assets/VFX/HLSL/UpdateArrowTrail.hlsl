#include "VFXProxCommon.hlsl"

float TimeFactor;
float MoveStrength;
float FlickerSpeed;

void UpdateSphereParticles(in float age, in float lifetime, inout VFXAttributes attributes)
{
    // 法線を計算（球の中心を基準に）
    float3 normal = normalize(attributes.position);
        
    // 近くの粒子を探索して、影響を受ける
    float3 nearest1, nearest2;
    VFXProx_LookUpNearestPair(attributes.position, nearest1, nearest2);

    // 最近接点の間のベクトルを基にランダムな移動を加える
    float3 moveDir = normalize(cross(nearest2 - nearest1, normal));
    attributes.velocity += moveDir * MoveStrength;
    // 位置を更新（球の表面に留まるように補正）
    attributes.position += attributes.velocity * 0.016;
    attributes.position = normalize(attributes.position) * length(attributes.position);

    // ちらつきエフェクト
    attributes.alpha = 0.5 + 0.5 * sin(age * FlickerSpeed);

    // 速度に応じて発光
    float speedFactor = length(attributes.velocity);
    attributes.color = lerp(float3(1, 0.5, 0), float3(1, 1, 0), speedFactor);
}