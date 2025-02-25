Shader "Custom/TreeLeaves_Shadow"
{
    Properties
    {
        _BaseMap ("Albedo (RGB)", 2D) = "white" {}  // 葉のカラーテクスチャ
        _AlphaMap ("Alpha Mask", 2D) = "white" {}   // 葉の透明度マスク
        _LeafColor ("Leaf Color", Color) = (0.3, 0.8, 0.3, 1)
        _Alpha ("Leaf Transparency", Range(0,1)) = 0.8
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.3

        // **Tree Editor 用のプロパティ（エラー防止）**
        _TranslucencyColor ("Translucency Color", Color) = (0.5, 0.5, 0.5, 1)
        _TranslucencyViewDependency ("Translucency View Dependency", Range(0,1)) = 0.5
        _ShadowStrength ("Shadow Strength", Range(0,1)) = 0.8
    }

    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        LOD 300
        Cull Off
        ZWrite On
        AlphaToMask On
        Blend SrcAlpha OneMinusSrcAlpha

        // **通常描画（ForwardLit）**
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float shellFactor : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_AlphaMap); SAMPLER(sampler_AlphaMap);

            float4 _LeafColor;
            float _Alpha;
            float _Cutoff;
            float4 _TranslucencyColor;
            float _TranslucencyViewDependency;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float shellIndex = fmod(IN.positionOS.x + IN.positionOS.y, 5);
                float shellOffset = shellIndex * 0.05;
                float wind = sin(_Time.y * 1.0 + IN.positionOS.x * 0.1 + shellIndex * 0.5) * 0.2;
                IN.positionOS.x += wind;
                IN.positionOS.y += wind * 0.3;
                IN.positionOS.xyz += IN.normalOS * shellOffset;

                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                OUT.shellFactor = shellIndex / 5.0;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half alphaMask = SAMPLE_TEXTURE2D(_AlphaMap, sampler_AlphaMap, IN.uv).r;
                float alpha = _Alpha * alphaMask;
                clip(alpha - _Cutoff);

                float3 viewDir = normalize(GetWorldSpaceViewDir(IN.positionCS));
                float translucency = dot(viewDir, IN.normalWS) * _TranslucencyViewDependency;
                float3 finalColor = texColor.rgb * _LeafColor.rgb + _TranslucencyColor.rgb * translucency;

                return half4(finalColor, alpha);
            }
            ENDHLSL
        }

        // **影の描画（ShadowCaster パス）**
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            HLSLPROGRAM
            #pragma vertex shadowVert
            #pragma fragment shadowFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_AlphaMap); SAMPLER(sampler_AlphaMap);
            float _Alpha;
            float _Cutoff;

            Varyings shadowVert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformWorldToHClip(TransformObjectToWorld(IN.positionOS.xyz));
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 shadowFrag(Varyings IN) : SV_Target
            {
                half alphaMask = SAMPLE_TEXTURE2D(_AlphaMap, sampler_AlphaMap, IN.uv).r;
                float alpha = _Alpha * alphaMask;
                clip(alpha - _Cutoff); // **透明な部分の影を削除**

                return 0;
            }
            ENDHLSL
        }
    }
}
