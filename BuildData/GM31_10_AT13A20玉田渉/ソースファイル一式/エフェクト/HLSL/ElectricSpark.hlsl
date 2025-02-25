// ElectricSpark.hlsl
float3 ElectricEffect(float2 uv, float time, float intensity) {
    // パーリンノイズでメッシュの揺らぎを表現
    float noise = frac(sin(dot(uv * 20.0f, float2(12.9898f, 78.233f))) * 43758.5453f);
    
    // UVスクロールで電撃の流れを表現
    float wave = sin(uv.y * 10.0f + time * intensity + noise * 5.0f);
    
    // 電撃カラー：青白い色
    float r = 0.0f;
    float g = 0.8f;
    float b = 1.0f;

    return float3(r, g, b);
}
