Shader "Custom/Outline"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1, 1, 1, 1)
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineThickness ("Outline Thickness", Float) = 0.02
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "Outline"
            Cull Front   // 裏面を描画
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // プロパティ
            float _OutlineThickness;
            float4 _OutlineColor;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            // 頂点シェーダー
            v2f vert(appdata_t v)
            {
                v2f o;
                // 頂点位置を外側に押し出す
                float3 worldNormal = normalize(UnityObjectToWorldNormal(v.normal));
                v.vertex.xyz += worldNormal * _OutlineThickness;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            // フラグメントシェーダー
            fixed4 frag(v2f i) : SV_Target
            {
                return _OutlineColor; // 輪郭線の色
            }
            ENDCG
        }

        Pass
        {
            Name "Main"
            Cull Back // 通常の表面描画
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragMain
            #include "UnityCG.cginc"

            float4 _MainColor;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 fragMain(v2f i) : SV_Target
            {
                return _MainColor; // メインカラー
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
