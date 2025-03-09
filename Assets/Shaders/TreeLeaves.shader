Shader "Custom/TreeLeaves_Shadow"
{
    Properties
    {
        _BaseMap ("Albedo (RGB)", 2D) = "white" {}  // 葉のカラーテクスチャ
        _AlphaMap ("Alpha Mask", 2D) = "white" {}   // 葉の透明度マスク
        _LeafColor ("Leaf Color", Color) = (0.3, 0.8, 0.3, 1) // 葉の基本色
        _Alpha ("Leaf Transparency", Range(0,1)) = 0.8 // 葉の透明度
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.3 // アルファテストの閾値（これ以下のα値は描画しない）

        // Tree Editor 用のプロパティ
        _TranslucencyColor ("Translucency Color", Color) = (0.5, 0.5, 0.5, 1) // 半透明光の色
        _TranslucencyViewDependency ("Translucency View Dependency", Range(0,1)) = 0.5 // 視点依存の透過効果
        _ShadowStrength ("Shadow Strength", Range(0,1)) = 0.8 // 影の濃さ
    }

    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" } // 透過マスク付きのレンダリング設定
        LOD 300
        Cull Off // 両面描画（葉は裏面も見える）
        ZWrite On
        AlphaToMask On // MSAA（マルチサンプルアンチエイリアス）対応のアルファカット
        Blend SrcAlpha OneMinusSrcAlpha // アルファブレンド設定

        // 通常描画パス（ライティングあり）
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
                float4 positionOS : POSITION; // オブジェクト空間での頂点座標
                float3 normalOS : NORMAL; // オブジェクト空間での法線
                float2 uv : TEXCOORD0; // UV座標
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0; // フラグメントシェーダー用のUV座標
                float4 positionCS : SV_POSITION; // クリップ空間の座標（描画位置）
                float shellFactor : TEXCOORD1; // 葉の厚み（シェル法による処理用）
                float3 normalWS : TEXCOORD2; // ワールド空間での法線
            };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_AlphaMap); SAMPLER(sampler_AlphaMap);

            float4 _LeafColor; // 葉の色
            float _Alpha; // 透明度
            float _Cutoff; // アルファカット閾値
            float4 _TranslucencyColor; // 半透明光の色
            float _TranslucencyViewDependency; // 視点依存の透過効果

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                // 風による葉の揺れ（X, Y軸方向にオフセットを加える）
                float shellIndex = fmod(IN.positionOS.x + IN.positionOS.y, 5); // 葉の個別性を持たせる
                float shellOffset = shellIndex * 0.05; // 葉の厚みを考慮
                float wind = sin(_Time.y * 1.0 + IN.positionOS.x * 0.1 + shellIndex * 0.5) * 0.2; // 風の揺れ計算
                IN.positionOS.x += wind; // X軸方向の揺れ
                IN.positionOS.y += wind * 0.3; // Y軸方向の揺れ（少し小さめ）
                IN.positionOS.xyz += IN.normalOS * shellOffset; // 法線方向にもわずかにオフセット（厚み表現）

                // クリップ空間座標への変換
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                OUT.shellFactor = shellIndex / 5.0; // シェルの厚み割合
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS); // ワールド空間の法線

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // テクスチャカラー取得
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);

                // アルファマスク取得
                half alphaMask = SAMPLE_TEXTURE2D(_AlphaMap, sampler_AlphaMap, IN.uv).r;
                float alpha = _Alpha * alphaMask; // 透明度適用
                clip(alpha - _Cutoff); // 一定値以下はカット（透過処理）

                // 半透明表現の計算
                float3 viewDir = normalize(GetWorldSpaceViewDir(IN.positionCS)); // 視線方向（ワールド空間）
                float translucency = dot(viewDir, IN.normalWS) * _TranslucencyViewDependency; // 法線と視線の角度による透過表現
                float3 finalColor = texColor.rgb * _LeafColor.rgb + _TranslucencyColor.rgb * translucency; // 最終色

                return half4(finalColor, alpha); // 出力カラー
            }
            ENDHLSL
        }

        // 影の描画（ShadowCaster パス）
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
                float4 positionOS : POSITION; // オブジェクト空間での頂点座標
                float2 uv : TEXCOORD0; // UV座標
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION; // クリップ空間の座標（影の描画位置）
                float2 uv : TEXCOORD0; // UV座標
            };

            TEXTURE2D(_AlphaMap); SAMPLER(sampler_AlphaMap);
            float _Alpha;
            float _Cutoff;

            Varyings shadowVert(Attributes IN)
            {
                Varyings OUT;
                // オブジェクト空間の座標をワールド空間へ変換し、さらにクリップ空間へ変換
                OUT.positionCS = TransformWorldToHClip(TransformObjectToWorld(IN.positionOS.xyz));
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 shadowFrag(Varyings IN) : SV_Target
            {
                // アルファマスク取得（影用）
                half alphaMask = SAMPLE_TEXTURE2D(_AlphaMap, sampler_AlphaMap, IN.uv).r;
                float alpha = _Alpha * alphaMask;
                clip(alpha - _Cutoff); // 透明部分の影をカット

                return 0; // 影のピクセルは黒（シャドウマップ用）
            }
            ENDHLSL
        }
    }
}
