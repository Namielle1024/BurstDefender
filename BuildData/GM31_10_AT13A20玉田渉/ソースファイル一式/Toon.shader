Shader "Custom/Toon"
{
    Properties
    {
        _Color ("Base Color", Color) = (1, 1, 1, 1)
        _MainTex ("Base Texture", 2D) = "white" {}
        _ShadeThreshold ("Shade Threshold", Range(0.0, 1.0)) = 0.5
        _ShadeColor ("Shade Color", Color) = (0, 0, 0, 1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // テクスチャとサンプラーの定義
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // マテリアルプロパティの定義
            CBUFFER_START(UnityPerMaterial)
            float4 _Color;           // ベースカラー
            float _ShadeThreshold;   // トゥーン影閾値
            float4 _ShadeColor;      // 影の色
            CBUFFER_END

            struct Attributes
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : NORMAL;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.position = TransformObjectToHClip(input.position);
                output.uv = input.uv;
                output.normalWS = TransformObjectToWorldNormal(input.normal);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // ベースカラーを取得
                float4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;

                // 法線を正規化
                float3 normal = normalize(input.normalWS);

                // メインライトの方向と色を取得
                Light mainLight = GetMainLight();
                float3 mainLightDir = normalize(mainLight.direction);
                float3 mainLightColor = mainLight.color.rgb;
                float mainLightIntensity = max(0.0, dot(normal, -mainLightDir)) * mainLightColor;

                // 追加ライトの計算
                float3 additionalLightsColor = float3(0.0, 0.0, 0.0);
                for (int i = 0; i < GetAdditionalLightsCount(); i++)
                {
                    float3 lightDir = normalize(GetAdditionalLightDirection(i));
                    float3 lightColor = GetAdditionalLightColor(i).rgb;
                    float lightIntensity = max(0.0, dot(normal, -lightDir)) * lightColor;
                    additionalLightsColor += lightIntensity;
                }

                // 合計ライト強度
                float combinedIntensity = length(mainLightIntensity + additionalLightsColor);

                // トゥーンの明暗切り替え
                float toonStep = step(_ShadeThreshold, combinedIntensity);

                // 明るい部分と影の色を分離
                float3 finalColor = lerp(_ShadeColor.rgb, baseColor.rgb, toonStep);

                return float4(finalColor, baseColor.a);
            }
            ENDHLSL
        }
    }
}
