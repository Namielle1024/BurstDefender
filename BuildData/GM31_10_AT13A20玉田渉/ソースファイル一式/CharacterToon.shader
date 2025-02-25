Shader "Custom/CharacterToon"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _ShadowColor ("Shadow Color", Color) = (0, 0, 0, 1)
        _LightThresholds ("Light Thresholds", Vector) = (0.4, 0.7, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // プロパティ
            sampler2D _MainTex;
            float4 _ShadowColor;
            float4 _LightThresholds;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // テクスチャカラー取得
                fixed4 baseColor = tex2D(_MainTex, i.uv);

                // ライティング計算
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float diff = max(0, dot(i.worldNormal, lightDir));

                // トゥーンステップ
                float stepValue = diff < _LightThresholds.x ? 0.0 :
                                  diff < _LightThresholds.y ? 0.5 : 1.0;

                // 最終色
                float3 finalColor = lerp(_ShadowColor.rgb, baseColor.rgb, stepValue);

                return fixed4(finalColor, baseColor.a);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
