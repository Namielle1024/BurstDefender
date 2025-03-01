Shader "Custom/Outline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1) // アウトラインの色
        _OutlineWidth ("Outline Width", Range(0.001, 0.03)) = 0.005 // 基本アウトライン幅
        _MinOutline ("Min Outline Width", Range(0.001, 0.02)) = 0.002 // 最小アウトライン幅
        _MaxOutline ("Max Outline Width", Range(0.01, 0.1)) = 0.03 // 最大アウトライン幅
        _DistanceFactor ("Distance Factor", Range(1.0, 10.0)) = 5.0 // 距離スケーリング
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Cull Front
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float4 _OutlineColor;
            float _OutlineWidth;
            float _MinOutline;
            float _MaxOutline;
            float _DistanceFactor;
            float3 _CameraPosition;

            v2f vert (appdata_t v)
            {
                v2f o;
                
                // カメラの位置からオブジェクトまでの距離を取得
                float3 worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
                float distance = length(_CameraPosition - worldPosition);
                
                // ワールド空間の法線を取得
                float3 worldNormal = normalize(UnityObjectToWorldNormal(v.normal));

                // 視線方向と法線の角度を考慮し、アウトライン幅をスケール
                float outlineScale = lerp(_MinOutline, _MaxOutline, pow(distance / _DistanceFactor, 0.5));

                // 法線方向に頂点を拡張
                v.vertex.xyz += worldNormal * outlineScale; 

                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _OutlineColor; // **アウトラインカラー**
            }
            ENDCG
        }
    }
}
