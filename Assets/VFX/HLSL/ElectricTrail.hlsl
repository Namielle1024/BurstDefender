float3 ElectricTrail(float time, float2 uv)
{
    float wave = sin(uv.y * 10.0 + time * 5.0);
    float intensity = smoothstep(0.4, 0.6, wave);
    return float3(0.3, 0.7, 1.0) * intensity;
}
