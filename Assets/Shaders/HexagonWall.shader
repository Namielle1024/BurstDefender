Shader "Custom/HexagonWallShader"
{
    Properties
    {
        _MainTex ("Hexagon Texture", 2D) = "white" {} // ハニカムテクスチャ
        _EmissionColor ("Emission Color", Color) = (0.0, 1.0, 2.0, 1.0) // 発光色
        _EmissionStrength ("Emission Strength", Float) = 2.0 // 発光強度
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5 // 透過しきい値
        _CutoffMin ("Min Alpha Cutoff", Range(0,1)) = 0.2 // 透過の最小値
        _CutoffMax ("Max Alpha Cutoff", Range(0,1)) = 0.8 // 透過の最大値
        _Speed ("Cutoff Change Speed", Float) = 1.0 // 透過変化の速度
        _Alpha ("Alpha Transparent", Range(0, 1)) = 1.0 // 透明度
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // アルファブレンド（透過処理）
            ZWrite Off // 透過を正しく描画するため
            Cull Off // 両面描画

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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
            float4 _EmissionColor;
            float _EmissionStrength;
            float _CutoffMin;
            float _CutoffMax;
            float _Speed;
            float _Alpha;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // 時間を利用して Cutoff 値を動的に変化させる
                float timeValue = abs(sin(_Time.y * _Speed)); // 0～1 の範囲で振動
                float dynamicCutoff = lerp(_CutoffMin, _CutoffMax, timeValue);

                // カットオフ値より小さいアルファは透明にする
                if (texColor.a < dynamicCutoff)
                    discard;

                // Emission（発光処理）
                fixed4 emission = texColor * _EmissionColor * _EmissionStrength;
                emission.a = _Alpha;
                return emission;
            }
            ENDCG
        }
    }
}
