Shader "Custom/TreeBark"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {} // 幹のベーステクスチャ
        _BarkColor ("Bark Color", Color) = (0.6, 0.4, 0.2, 1) // 幹の色
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" } // 不透明オブジェクトとして描画
        LOD 200 // シェーダーの詳細度（低スペック向け最適化用）

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // 頂点データの構造体
            struct appdata 
            {
                float4 vertex : POSITION; // 頂点座標（オブジェクト空間）
                float2 uv : TEXCOORD0; // UV座標
            };

            // 頂点シェーダーからフラグメントシェーダーに渡すデータの構造体
            struct v2f 
            {
                float2 uv : TEXCOORD0; // UV座標
                float4 vertex : SV_POSITION; // クリップ空間の座標
            };

            sampler2D _MainTex; // 幹のテクスチャ
            float4 _MainTex_ST; // UV座標のスケールとオフセット

            v2f vert (appdata v) 
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // オブジェクト空間からクリップ空間へ変換
                o.uv = TRANSFORM_TEX(v.uv, _MainTex); // UV座標にスケールとオフセットを適用
                return o;
            }

            fixed4 frag (v2f i) : SV_Target 
            {
                fixed4 col = tex2D(_MainTex, i.uv); // テクスチャカラー取得
                return col; // 出力カラー
            }
            ENDCG
        }

        // 影の描画パス
        Pass {
            Tags { "LightMode"="ShadowCaster" } // シャドウマップ生成用のパス
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster // シャドウマップ生成のためのマルチコンパイル

            #include "UnityCG.cginc"

            // 頂点データの構造体
            struct appdata {
                float4 vertex : POSITION; // 頂点座標（オブジェクト空間）
                float3 normal : NORMAL; // 法線データ
                float2 uv : TEXCOORD0; // UV座標（影には不要だが統一のため）
            };

            // 頂点シェーダーからフラグメントシェーダーに渡すデータの構造体
            struct v2f {
                V2F_SHADOW_CASTER; // シャドウキャスター用の構造体（影を正しく計算するためのデータ）
            };

            v2f vert (appdata v) {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o); // 影を適切にキャストするための変換
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                SHADOW_CASTER_FRAGMENT(i) // 影の描画処理
            }
            ENDCG
        }
    }
}
