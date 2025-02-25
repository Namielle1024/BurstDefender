Shader "Custom/TreeBark"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {} // 幹のテクスチャ
        _BarkColor ("Bark Color", Color) = (0.6, 0.4, 0.2, 1) // 幹の色
    }

    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            fixed4 _BarkColor;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                return fixed4(tex.rgb * _BarkColor.rgb, 1.0);
            }
            ENDCG
        }
    }
}
